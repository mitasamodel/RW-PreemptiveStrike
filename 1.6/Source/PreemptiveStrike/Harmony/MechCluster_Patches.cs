using HarmonyLib;
using PES.RW_JustUtils;
using PreemptiveStrike.Interceptor;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;
using static UnityEngine.Scripting.GarbageCollector;

namespace PreemptiveStrike.Harmony
{
	internal class MechCluster_Patches
	{
		#region Mech cluster

		/// <summary>
		/// Main entry location.
		/// </summary>
		[HarmonyPatch(typeof(IncidentWorker_MechCluster), "TryExecuteWorker")]
		internal static class Patch_IncidentWorker_MechCluster
		{
			internal static bool Prefix(IncidentWorker_MechCluster __instance, MethodBase __originalMethod, IncidentParms parms, ref bool __result)
			{
				if (PES_Settings.DebugModeOn)
				{
					Logger.LogNL($"[{__instance.GetType().Name}.{__originalMethod.Name}] Prefix.");
					Logger.LogNL($"\tWorker mode [{IncidentInterceptorUtility.Interception_MechCluster}]");
				}

				if (Helper.IsQuest(parms))
					return true;

				// Arm the interceptor.
				if (IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Ready)
					IncidentInterceptorUtility.Interception_MechCluster = MechClusterWorkerType.Steady;

				return true;
			}

			private static readonly MethodInfo MI_SpawnCluster =
				AccessTools.Method(typeof(MechClusterUtility), nameof(MechClusterUtility.SpawnCluster));

			private static readonly MethodInfo MI_GenerateClusterSketch =
				AccessTools.Method(typeof(MechClusterGenerator), nameof(MechClusterGenerator.GenerateClusterSketch));

			private static readonly MethodInfo MI_FindClusterPosition =
				AccessTools.Method(typeof(MechClusterUtility), nameof(MechClusterUtility.FindClusterPosition));

			private static readonly MethodInfo MI_EarlyGate =
				AccessTools.Method(typeof(Patch_IncidentWorker_MechCluster), nameof(EarlyGate));

			private static readonly MethodInfo MI_Processing =
				AccessTools.Method(typeof(Patch_IncidentWorker_MechCluster), nameof(Processing));

			[HarmonyTranspiler]
			internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
			{
				var matcher = new CodeMatcher(instructions, il);

				// Define label for jumping to second method.
				var labelProcessing = il.DefineLabel();

				// Insert call for first method and jump.
				matcher.Start();
				matcher.Insert(
					new CodeInstruction(OpCodes.Call, MI_EarlyGate),        // Returns bool.
					new CodeInstruction(OpCodes.Brtrue, labelProcessing)    // true -> jump.
				);

				// Find local variable for 'sketch'.
				//// MechClusterSketch sketch = MechClusterGenerator.GenerateClusterSketch(parms.points, map);
				//IL_000c: ldarg.1
				//IL_000d: ldfld float32 RimWorld.IncidentParms::points
				//IL_0012: ldloc.0
				//IL_0013: ldc.i4.1
				//IL_0014: ldc.i4.0
				//IL_0015: call class RimWorld.MechClusterSketch RimWorld.MechClusterGenerator::GenerateClusterSketch(float32, class Verse.Map, bool, bool)
				//IL_001a: stloc.1
				matcher.MatchEndForward(
					new CodeMatch(OpCodes.Call, MI_GenerateClusterSketch),  // Call GenerateClusterSketch().
					new CodeMatch(ci => ci.IsStloc())                       // Store result in local variable.
				).ThrowIfInvalid("Can not find location of 'sketch'");
				int sketchLocalIndex = matcher.InstructionAt(0).LocalIndex();

				// Find local variable for 'center'.
				//// IntVec3 center = MechClusterUtility.FindClusterPosition(map, sketch, 100, 0.5f);
				//IL_001b: ldloc.0
				//IL_001c: ldloc.1
				//IL_001d: ldc.i4.s 100
				//IL_001f: ldc.r4 0.5
				//IL_0024: call valuetype Verse.IntVec3 RimWorld.MechClusterUtility::FindClusterPosition(class Verse.Map, class RimWorld.MechClusterSketch, int32, float32)
				//IL_0029: stloc.2
				matcher.MatchEndForward(
					new CodeMatch(OpCodes.Call, MI_FindClusterPosition),    // Call FindClusterPosition().
					new CodeMatch(ci => ci.IsStloc())                       // Store result in local variable.
				).ThrowIfInvalid("Can not find location of 'center'");
				int centerLocalIndex = matcher.InstructionAt(0).LocalIndex();

				// Find the call SpawnCluster().
				matcher.MatchStartForward(
					new CodeMatch(OpCodes.Call, MI_SpawnCluster)            // Call.
				).ThrowIfInvalid("Can not find call SpawnCluster()");
				// But there are a bunch of variables on the stack for SpawnCluster().
				// Easiest way to deal with them is to go back until we find ldloc for 'center' (and we know the index).
				matcher.MatchStartBackwards(
					new CodeMatch(ci => IsLoadOfLocal(ci, centerLocalIndex))
				).ThrowIfInvalid("Can not find load 'center'");
				// Label for Spawn.
				var labelSpawn = il.DefineLabel();
				// Now we can insert our logic here.
				matcher.Insert(
					new CodeInstruction(OpCodes.Nop).WithLabels(labelProcessing),
					//new CodeInstruction(OpCodes.Ldarg_1).WithLabels(labelProcessing),   // Push 'parms'.
					new CodeInstruction(OpCodes.Ldarg_1),   // Push 'parms'.
					new CodeInstruction(OpCodes.Ldloca_S, (short)centerLocalIndex),     // Push address of 'center'.
					new CodeInstruction(OpCodes.Ldloca_S, (short)sketchLocalIndex),     // Push address of 'sketch'.
					new CodeInstruction(OpCodes.Ldarg_0),                               // Push '__instance'.
					new CodeInstruction(OpCodes.Call, MI_Processing),                   // Call Processing(). Returns bool.

					// If true, then continue to SpawnCluster().
					// If false, then return from the main method with false.
					new CodeInstruction(OpCodes.Brtrue_S, labelSpawn),                  // true -> jump to label for Spawn (params).
					new CodeInstruction(OpCodes.Ldc_I4_0),                              // Push '0'
					new CodeInstruction(OpCodes.Ret),                                   // Return
					new CodeInstruction(OpCodes.Nop).WithLabels(labelSpawn)             // label for Spawn.
				);

				Logger.LogNL($"sketch index [{sketchLocalIndex}]");
				Logger.LogNL($"center index [{centerLocalIndex}]");

				return matcher.InstructionEnumeration();
			}

			static bool IsLoadOfLocal(in CodeInstruction ci, int localIndex)
			{
				var op = ci.opcode;

				// 1. Handle hardcoded opcodes for the first 4 locals
				if (op == OpCodes.Ldloc_0) return localIndex == 0;
				if (op == OpCodes.Ldloc_1) return localIndex == 1;
				if (op == OpCodes.Ldloc_2) return localIndex == 2;
				if (op == OpCodes.Ldloc_3) return localIndex == 3;

				// 2. Handle "load local by value" instructions (generic form)
				if (op == OpCodes.Ldloc || op == OpCodes.Ldloc_S)
					return (ci.operand as LocalBuilder)?.LocalIndex == localIndex
						|| (ci.operand is short s && s == localIndex);

				// 3. Handle "load local by address" (ref/out) instructions
				if (op == OpCodes.Ldloca || op == OpCodes.Ldloca_S)
					return (ci.operand as LocalBuilder)?.LocalIndex == localIndex
						|| (ci.operand is short s2 && s2 == localIndex);

				// 4. Otherwise, not a load of a local
				return false;
			}


			/// <summary>
			/// Jump to second method if true.
			/// true => We are at Execution phase, skip calculating.
			/// </summary>
			/// <returns></returns>
			private static bool EarlyGate()
			{
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"[Patch_IncidentWorker_MechCluster.EarlyGate]");
				return IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Execute;
			}

			/// <summary>
			/// If this is the first run, then store data, set caravan and do not spawn anything.
			/// If this is the second (execution) run, then set values and return true.
			/// </summary>
			/// <param name="__instance"></param>
			/// <param name="center"></param>
			/// <param name="map"></param>
			/// <param name="sketch"></param>
			/// <returns></returns>
			private static bool Processing(IncidentParms parms, ref IntVec3 center, ref MechClusterSketch sketch, IncidentWorker_MechCluster __instance)
			{
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"[Patch_IncidentWorker_MechCluster.Processing]");
				using var _ = Logger.Scope();
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"Worker mode [{IncidentInterceptorUtility.Interception_MechCluster}].");

				// First run. Got values calculated.
				if (IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Steady)
				{

				}
				// Second run. Execute -> Spawn.
				else if (IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Execute)
				{
					// Sketch.
					if (IncidentInterceptorUtility.tempMechClusterSketch != null)
						sketch = IncidentInterceptorUtility.tempMechClusterSketch;
					else
					{
						if (PES_Settings.DebugModeOn)
							Logger.Log_Error($"Sketch is null.");
					}

					// Center.
					center = IncidentInterceptorUtility.tempCenter;

					return true;
				}

				return true;
			}

			internal static void Postfix(IncidentWorker_MechCluster __instance, MethodBase __originalMethod)
			{
				if (PES_Settings.DebugModeOn)
				{
					Logger.LogNL($"[{__instance.GetType().Name}.{__originalMethod.Name}] Postfix.");
					Logger.LogNL($"\tWorker mode [{IncidentInterceptorUtility.Interception_MechCluster}]. Will disarm");
				}

				// Disarm the interceptor.
				IncidentInterceptorUtility.Interception_MechCluster = MechClusterWorkerType.Ready;
			}
		}

		/// <summary>
		/// Use pre-calculated center.
		/// </summary>
		[HarmonyPatch(typeof(MechClusterUtility))]
		internal static class Patch_MechClusterUtility
		{
			[HarmonyPatch("FindClusterPosition")]
			internal static bool Prefix(ref IntVec3 __result, MethodBase __originalMethod)
			{
				if (PES_Settings.DebugModeOn)
				{
					Logger.LogNL($"[{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}] Prefix.");
					Logger.LogNL($"\tWorker mode [{IncidentInterceptorUtility.Interception_MechCluster}].");
				}

				if (IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Execute)
				{

					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(PawnsArrivalModeWorker_ClusterDrop), "TryResolveRaidSpawnCenter")]
		static class Patch_ClusterDrop
		{
			internal static bool Prefix(MethodBase __originalMethod, IncidentParms parms, ref bool __result)
			{
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"[{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}] Prefix.");
				using var _ = Logger.Scope();
				if (PES_Settings.DebugModeOn)
				{
					Logger.Log_Error($"Unimplemented method.");

					//if (parms.spawnCenter.IsValid)
					//	Logger.LogNL($"SpawnCenter [{parms.spawnCenter}]");
					//else
					//	// It is ok for some arrival modes.
					//	Logger.LogNL($"No valid SpawnCenter");
				}

				return true;
			}
		}
		#endregion
	}
}
