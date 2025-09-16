using HarmonyLib;
using PES.RW_JustUtils;
using PreemptiveStrike.IncidentCaravan;
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
				{
					if (PES_Settings.DebugModeOn)
						Logger.LogNL($"Change to [{MechClusterWorkerType.Forestall}]");
					IncidentInterceptorUtility.Interception_MechCluster = MechClusterWorkerType.Forestall;
				}

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
				//Logger.LogNL("==Instructions==");
				//foreach(var ins in instructions)
				//{
				//	Logger.LogNL(ins.ToString());
				//}

				var matcher = new CodeMatcher(instructions, il);
				matcher.Start();

				// Define label for jumping to second method.
				var labelProcessing = il.DefineLabel();

				// Start after the local variable 'map' is set.
				//// Map map = (Map)parms.target;
				//IL_0000: ldarg.1
				//IL_0001: ldfld class RimWorld.IIncidentTarget RimWorld.IncidentParms::target
				//IL_0006: castclass Verse.Map
				//IL_000b: stloc.0
				matcher.MatchEndForward(
					new CodeMatch(OpCodes.Castclass, typeof(Map)),		// Cast to 'Map'
					new CodeMatch(cm => cm.IsStloc())					// Store local variable.
				).ThrowIfInvalid("Can not find location of 'map'");
				matcher.Advance(1);

				// Insert call for first method and jump.
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

				// Place our label and Processing method right after that.
				matcher.Advance(1);
				// Label to cnotinue.
				var labelContinue = il.DefineLabel();
				// Now we can insert our logic here.
				matcher.Insert(
					new CodeInstruction(OpCodes.Ldarg_1).WithLabels(labelProcessing),   // Push 'parms'.
					new CodeInstruction(OpCodes.Ldloca_S, (short)centerLocalIndex),     // Push address of 'center'.
					new CodeInstruction(OpCodes.Ldloca_S, (short)sketchLocalIndex),     // Push address of 'sketch'.
					new CodeInstruction(OpCodes.Ldarg_0),                               // Push '__instance'.
					new CodeInstruction(OpCodes.Call, MI_Processing),                   // Call Processing(). Returns bool.

					// If true, then continue.
					// If false, then return from the main method with false.
					new CodeInstruction(OpCodes.Brtrue_S, labelContinue),                  // true -> jump to label.
					new CodeInstruction(OpCodes.Ldc_I4_0),                              // Push '0'
					new CodeInstruction(OpCodes.Ret),                                   // Return
					new CodeInstruction(OpCodes.Nop).WithLabels(labelContinue)             // label.
				);

				//Logger.LogNL($"sketch index [{sketchLocalIndex}]");
				//Logger.LogNL($"center index [{centerLocalIndex}]");

				//Logger.LogNL("==Matcher Instructions==");
				//foreach (var ins in matcher.InstructionEnumeration())
				//{
				//	Logger.LogNL(ins.ToString());
				//}

				return matcher.InstructionEnumeration();
			}

			/// <summary>
			/// This was sugested by chatGPT. I decided, that it is good enough, so I will not modify anything.
			/// Checks if the current instruction is a push of a local with a pre-defined index.
			/// </summary>
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
				if (IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Forestall)
				{
					// Intercept incident.
					var res = IncidentInterceptorUtility.Intercept_MechCluster(parms, center, sketch, __instance);

					// Stop if intercepted.
					return !res;

					// Transpiler added exit with 'false' from main method if we return 'false' here.
				}
				// Execute: set variables for spawn call.
				else if (IncidentInterceptorUtility.Interception_MechCluster == MechClusterWorkerType.Execute)
				{
					if (PES_Settings.DebugModeOn)
						Logger.LogNL($"Restore 'center' and 'sketch'.");
					center = IncidentInterceptorUtility.tempCenter;
					sketch = IncidentInterceptorUtility.tempMechClusterSketch;
				}

				return true;
			}

			/// <summary>
			/// Finished. Either intercepted or executed.
			/// </summary>
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
