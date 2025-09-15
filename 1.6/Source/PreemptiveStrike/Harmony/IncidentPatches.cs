using HarmonyLib;
using PES.RW_JustUtils;
using PreemptiveStrike.Interceptor;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.Noise;

namespace PreemptiveStrike.Harmony
{
	[HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute))]
	public static class Patch_IncidentWorker_TryExecute
	{
		public static bool Prefix(IncidentWorker __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"\n[IncidentWorker: TryExecute] Prefix.");
			using var _ = Logger.Scope();
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"Worker Class [{__instance.GetType()}]");
				Debug.DebugParms(parms, __instance);
			}

			if (Helper.IsQuest(parms))
				return true;

			if (__instance.def == null)
			{
				Logger.Log_Error($"[Patch_IncidentWorker_TryExecute: Prefix] Unexpected null __instance.def.");
				Logger.Log_Warning($"Please report it to the mod author.");
				Verse.Log.Message($"Type [{__instance.GetType()}]");
				return true;
			}

			if (__instance.def.defName == "RRY_PowerCut_Xenomorph")  //Lt.Bob - Handling for AvP powercut event
			{
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"RRY_PowerCut_Xenomorph");
				return true;
			}

			//TODO: This is for the ship part incident
			//I have no choice but do the patch like this
			//'cause the incidentworker for shippart is an internal class
			//and manual patching doesn't work
			var def = __instance.def;
			if (def != DefDatabase<IncidentDef>.GetNamed("PsychicEmanatorShipPartCrash") && def != DefDatabase<IncidentDef>.GetNamed("DefoliatorShipPartCrash"))    //Lt. Bob: 1.1 - Replaced PoisonShipPartCrash with DefoliatorShipPartCrash
				return true;
			if (IncidentInterceptorUtility.IsIntercepting_ShipPart == WorkerPatchType.ExecuteOrigin)
				return true;
			else
			{
				if (!IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_ShipPartCrash>(__instance.def, parms))
					return true;
				__result = true;
				return false;
			}
		}

		static void Postfix(ref bool __result)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[IncidentWorker: TryExecute] Postfix.");
				Logger.LogNL($"\tIsHoaxingStoryTeller [{IncidentInterceptorUtility.IsHoaxingStoryTeller}]");
			}

			if (IncidentInterceptorUtility.IsHoaxingStoryTeller)
			{
				__result = true;
				IncidentInterceptorUtility.IsHoaxingStoryTeller = false;
			}
		}
	}

	//This patch is made for all the raid incidents to help them get the right incidentDef
	[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
	class Patch_RaidEnemy_TryExecuteWorker
	{
		static void Prefix(IncidentWorker_RaidEnemy __instance)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[IncidentWorker_RaidEnemy.TryExecuteWorker] Prefix.");
				Logger.LogNL($"\tSave instance Def[{__instance.def}]");
			}

			IncidentInterceptorUtility.CurrentIncidentDef = __instance.def;
		}

		static void Postfix(IncidentWorker_RaidEnemy __instance, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[IncidentWorker_RaidEnemy.TryExecuteWorker] Postfix.");
				Logger.LogNL($"\traidArrivalMode [{parms.raidArrivalMode}]");
			}
		}
	}

	#region PawnsArrivalModeWorker patches
	/// <summary>
	/// Helper: list of classes to patch.
	/// </summary>
	internal static class Helper_PawnsArrivalModeWorker_Classes
	{
		internal static readonly Type[] TargetTypes = new[]
		{
			typeof(PawnsArrivalModeWorker_EdgeWalkIn),
			typeof(PawnsArrivalModeWorker_EdgeWalkInGroups),
			typeof(PawnsArrivalModeWorker_EdgeWalkInDarkness),
			typeof(PawnsArrivalModeWorker_EdgeWalkInDistributed),
			typeof(PawnsArrivalModeWorker_EdgeWalkInDistributedGroups),
			typeof(PawnsArrivalModeWorker_EdgeWalkInHateChanters),
		};
	}

	/// <summary>
	/// Logging purpose.
	/// </summary>
	[HarmonyPatch]
	public static class Patch_PawnsArrivalModeWorker_Arrive
	{
		private static IEnumerable<MethodBase> TargetMethods()
		{
			foreach (var type in Helper_PawnsArrivalModeWorker_Classes.TargetTypes)
				yield return AccessTools.Method(type, "Arrive");
		}

		public static bool Prefix(object __instance, MethodBase __originalMethod, List<Pawn> pawns)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[{__instance.GetType().Name}.{__originalMethod.Name}] Prefix.");
				Logger.LogNL($"\tIs about to generate [{pawns?.Count ?? 0}] pawns.");
			}

			return true;
		}
	}

	//---------------------------------------------------------
	[HarmonyPatch]
	public static class Patch_PawnsArrivalModeWorker_WalkIn_Common
	{
		private static IEnumerable<MethodBase> TargetMethods()
		{
			foreach (var type in Helper_PawnsArrivalModeWorker_Classes.TargetTypes)
				yield return AccessTools.Method(type, "TryResolveRaidSpawnCenter");
		}

		/// <summary>
		/// Some Workers now can re-calculate the spawnCenter based on surroundings conditions.
		/// By the time a caravan achieve our base, the calculations will drift.
		/// To avoid recalculations (we already shown the invasion location to the Player) use the same result as before.
		/// </summary>
		public static bool Prefix(PawnsArrivalModeWorker __instance, MethodBase __originalMethod, IncidentParms parms, ref bool __result)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[Patch_PawnsArrivalModeWorker_WalkIn_Common] Prefix.");
				Logger.LogNL($"\tMethod [{__instance.GetType().Name}.{__originalMethod.Name}]");
			}

			// If it is not the incident we are waiting, then continue.
			var active = IncidentInterceptorUtility.ActiveExecutionParms;
			if (!ReferenceEquals(parms, active)) return true;

			// Or continue if the spawnCenter is not valid (it might never have been and will not be).
			if (!parms.spawnCenter.IsValid) return true;

			// Otherwise - skip new calculations.
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"\tSkip it. Will use [{parms.spawnCenter}]");
			}
			// Report that we're good.
			__result = true;
			// But don't execute.
			return false;
		}

		public static void Postfix(PawnsArrivalModeWorker __instance, MethodBase __originalMethod, IncidentParms parms, ref bool __result)
		{
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"[Patch_PawnsArrivalModeWorker_WalkIn_Common] Postfix.");
			using var _ = Logger.Scope();
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"Method [{__instance.GetType().Name}.{__originalMethod.Name}]");
				if (parms.spawnCenter.IsValid)
					Logger.LogNL($"SpawnCenter [{parms.spawnCenter}]");
				else
					// It is ok for some arrival modes.
					Logger.LogNL($"No valid SpawnCenter");
			}

			if (Helper.IsQuest(parms))
				return;

			// This is a temporary fix for refugee chased.
			if (IncidentInterceptorUtility.IncidentInQueue(parms, IncidentDefOf.RaidEnemy))
			{
				if (PES_Settings.DebugModeOn)
					Logger.LogNL("Temporary fix for refugee chased.");
				return;
			}

			// Need feedback about gameplay to understand when does it happen exactly.
			if ( __instance is PawnsArrivalModeWorker_EdgeWalkInHateChanters)
			{
				Logger.Log_Warning($"Please report this data to mod author.");
				if (PES_Settings.DebugModeOn)
				{
					Logger.LogNL($"[PawnsArrivalModeWorker_EdgeWalkInHateChanters] Strategy[{parms.raidStrategy.Worker.GetType().Name}]");
					Debug.DebugParms(parms);
				}
				Verse.Log.Message($"[PawnsArrivalModeWorker_EdgeWalkInHateChanters] Strategy[{parms.raidStrategy.Worker.GetType().Name}]");
				Debug.DebugParms(parms, toConsole: true);
				return;
			}

			if (IncidentInterceptorUtility.IsIntercepting_IncidentExecution)
			{
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"Intercepting...");

				if (IncidentInterceptorUtility.Intercept_Raid(parms, __instance))
					__result = false;
			}
			else if (PES_Settings.DebugModeOn)
				Logger.LogNL($"Not intercepting...");
		}
	}
	#endregion

	#region Generation Patches
	[HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
	static class Patch_PawnGroupMakerUtility_GeneratePawns
	{
		[HarmonyPrefix]
		static bool Prefix(ref IEnumerable<Pawn> __result)
		{
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"[PawnGroupMakerUtility.GeneratePawns] Prefix.");
			using var _ = Logger.Scope();
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"GeneratorPatchFlag [{IncidentInterceptorUtility.IsIntercepting_PawnGeneration}]");

			if (IncidentInterceptorUtility.IsIntercepting_PawnGeneration == GeneratorPatchFlag.Generate)
				return true;
			if (IncidentInterceptorUtility.IsIntercepting_PawnGeneration == GeneratorPatchFlag.ReturnTempList)
				__result = IncidentInterceptorUtility.tmpPawnList;
			else
				__result = new List<Pawn>();
			IncidentInterceptorUtility.IsIntercepting_PawnGeneration = GeneratorPatchFlag.Generate;
			return false;
		}
	}

	[HarmonyPatch(typeof(PawnsArrivalModeWorkerUtility), "SplitIntoRandomGroupsNearMapEdge")]
	static class Patch_PawnsArrivalModeWorkerUtility_SplitIntoRandomGroupsNearMapEdge
	{
		[HarmonyPrefix]
		static bool Prefix(ref List<Pair<List<Pawn>, IntVec3>> __result)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[PawnsArrivalModeWorkerUtility.SplitIntoRandomGroupsNearMapEdge] Prefix.");
				Logger.LogNL($"\t IsIntercepting_GroupSpliter flag [{IncidentInterceptorUtility.IsIntercepting_GroupSpliter}]");
			}

			if (IncidentInterceptorUtility.IsIntercepting_GroupSpliter == GeneratorPatchFlag.Generate)
				return true;
			if (IncidentInterceptorUtility.IsIntercepting_GroupSpliter == GeneratorPatchFlag.ReturnTempList)
				__result = IncidentInterceptorUtility.tempGroupList;
			else
				__result = new List<Pair<List<Pawn>, IntVec3>>();
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;
			return false;
		}
	}

	/// <summary>
	/// Patch private method in PawnsArrivalModeWorker_EdgeWalkInDistributedGroups.
	/// </summary>
	[HarmonyPatch(typeof(PawnsArrivalModeWorker_EdgeWalkInDistributedGroups), "SplitIntoRandomGroupsNearMapEdge")]
	static class Patch_PawnsArrivalModeWorker_EdgeWalkInDistributedGroups_SplitIntoRandomGroupsNearMapEdge
	{
		static bool Prefix(object __instance, MethodBase __originalMethod, ref List<Pair<List<Pawn>, IntVec3>> __result)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[{__instance.GetType().Name}.{__originalMethod.Name}] Prefix.");
				Logger.LogNL($"\t IsIntercepting_GroupSpliter flag [{IncidentInterceptorUtility.IsIntercepting_GroupSpliter}]");
			}

			if (IncidentInterceptorUtility.IsIntercepting_GroupSpliter == GeneratorPatchFlag.Generate)
				return true;
			if (IncidentInterceptorUtility.IsIntercepting_GroupSpliter == GeneratorPatchFlag.ReturnTempList)
				__result = IncidentInterceptorUtility.tempGroupList;
			else
				__result = new List<Pair<List<Pawn>, IntVec3>>();
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;
			return false;
		}

		/// <summary>
		/// This will allow to call this private method from our class.
		/// </summary>
		[HarmonyReversePatch]
		[HarmonyPatch(typeof(PawnsArrivalModeWorker_EdgeWalkInDistributedGroups), "SplitIntoRandomGroupsNearMapEdge")]
		public static List<Pair<List<Pawn>, IntVec3>> Call(
			PawnsArrivalModeWorker_EdgeWalkInDistributedGroups __instance,
			List<Pawn> pawns, Map map)
		{
			// This will be replaced at runtime with the private method body.
			throw new NotImplementedException("Stub for Harmony reverse patch");
		}
	}

	[HarmonyPatch(typeof(RCellFinder), "TryFindRandomPawnEntryCell")]
	public static class Patch_RCellFinder_TryFindRandomPawnEntryCell
	{
		/// <summary>
		/// It is used to generate random entry cell for pawns.
		/// If we pre-generated it, then we simple return the cells one by one.
		/// </summary>
		public static bool Prefix(ref bool __result, ref IntVec3 result, Map map)
		{
			if (IncidentInterceptorUtility.TryFindRandomPawnEntryCell == GeneratorPatchFlag.ReturnTempList &&
				IncidentInterceptorUtility.tempEntryCells != null &&
				IncidentInterceptorUtility.tempEntryCells.Count > 0 &&
				IncidentInterceptorUtility.ActiveExecutionParms?.target == map)
			{
				var list = IncidentInterceptorUtility.tempEntryCells;

				// Return first cell and remove it from the list. Next pawn will get the next saved cell.
				result = list[0];
				list.RemoveAt(0);
				__result = true;    // original return.
				IncidentInterceptorUtility.TryFindRandomPawnEntryCell = GeneratorPatchFlag.Generate;
				return false;       // skip original execution.
			}

			IncidentInterceptorUtility.TryFindRandomPawnEntryCell = GeneratorPatchFlag.Generate;
			return true;
		}
	}
	#endregion

	#region Human Netral Patch

	[HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "TryExecuteWorker")]
	static class Patch_IncidentWorker_TraderCaravanArrival_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_TraderCaravanArrival __instance, ref bool __result, IncidentParms parms)
		{
			using var _ = Logger.Scope();
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL(0, $"[IncidentWorker_TraderCaravanArrival.TryExecuteWorker] Prefix.");
				Debug.DebugParms(parms, __instance.ToString());
			}
			if (Helper.IsQuest(parms))
				return true;
			if (IncidentInterceptorUtility.isIntercepting_TraderCaravan_Worker)
				return !IncidentInterceptorUtility.CreateIncidentCaravan_HumanNeutral<InterceptedIncident_HumanCrowd_TraderCaravan>(__instance.def, parms);
			return true;
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_TravelerGroup), "TryExecuteWorker")]
	static class Patch_IncidentWorker_TravelerGroup_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_TravelerGroup __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_VisitorGroup_TryExecuteWorker Prefix");
				Debug.DebugParms(parms, __instance.ToString());
			}
			if (parms.quest != null || parms.questScriptDef != null)
			{
				Log.Message("-=PS=- It's a quest! Bailout! MAYDAY!");
				return true;
			}
			if (parms != null && parms.questTag != null)    //Lt.Bob - May be redundant
			{
				Log.Error("-=PS=- Not redundant");
				Log.Message("-=PS=- Patch_IncidentWorker_TravelerGroup_TryExecuteWorker - questTag!=Null == " + parms.questTag);
				Log.Message("-=PS=- Returning true");
				return true;
			}
			if (IncidentInterceptorUtility.isIntercepting_TravelerGroup)
				return !IncidentInterceptorUtility.CreateIncidentCaravan_HumanNeutral<InterceptedIncident_HumanCrowd_TravelerGroup>(__instance.def, parms);
			return true;
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "TryExecuteWorker")]
	static class Patch_IncidentWorker_VisitorGroup_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_VisitorGroup __instance, ref bool __result, IncidentParms parms)
		{
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_IncidentWorker_VisitorGroup_TryExecuteWorker Prefix"); //Lt. Bob - Logging
			if (parms != null && parms.questTag != null || parms.quest != null && parms.quest.ToString() == "RimWorld.Quest") //Lt. Bob - "Temporary" bypass fix? for Quest handling; 11/9 Added  parms.quest check
			{
				Log.Message("-=PS=- Patch_IncidentWorker_VisitorGroup_TryExecuteWorker - questTag!=Null == " + parms.questTag);
				Log.Message("-=PS=- Returning true");
				return true;
			}
			if (IncidentInterceptorUtility.isIntercepting_VisitorGroup)
				return !IncidentInterceptorUtility.CreateIncidentCaravan_HumanNeutral<InterceptedIncident_HumanCrowd_VisitorGroup>(__instance.def, parms);
			return true;
		}
	}
	#endregion

	#region  Animal Incident Patch
	[HarmonyPatch(typeof(IncidentWorker_FarmAnimalsWanderIn), "TryExecuteWorker")]
	static class Patch_IncidentWorker_FarmAnimalsWanderIn_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_FarmAnimalsWanderIn __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_FarmAnimalsWanderIn_TryExecuteWorker Prefix");
				Debug.DebugParms(parms, __instance.ToString());
			}

			if (IncidentInterceptorUtility.isIntercepting_FarmAnimalsWanderIn == WorkerPatchType.ExecuteOrigin)
				return true;
			if (IncidentInterceptorUtility.isIntercepting_FarmAnimalsWanderIn == WorkerPatchType.Forestall)
			{
				IncidentInterceptorUtility.CreateIncidentCaravan_Animal<InterceptedIncident_AnimalHerd_FarmAnimalsWanderIn>(__instance.def, parms);
				__result = true;
			}
			else
				__result = IncidentInterceptorUtility.tmpIncident.SubstituionWorkerExecution();
			return false;
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_HerdMigration), "TryExecuteWorker")]
	static class Patch_IncidentWorker_HerdMigration_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_HerdMigration __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_HerdMigration_TryExecuteWorker Prefix");
				Debug.DebugParms(parms, __instance.ToString());
			}

			if (IncidentInterceptorUtility.isIntercepting_HerdMigration == WorkerPatchType.ExecuteOrigin)
				return true;
			if (IncidentInterceptorUtility.isIntercepting_HerdMigration == WorkerPatchType.Forestall)
			{
				IncidentInterceptorUtility.CreateIncidentCaravan_Animal<InterceptedIncident_AnimalHerd_HerdMigration>(__instance.def, parms);
				__result = true;
			}
			else
				__result = IncidentInterceptorUtility.tmpIncident.SubstituionWorkerExecution();
			return false;
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_ThrumboPasses), "TryExecuteWorker")]
	static class Patch_IncidentWorker_ThrumboPasses_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_ThrumboPasses __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_ThrumboPasses_TryExecuteWorker Prefix");
				Debug.DebugParms(parms, __instance.ToString());
			}

			if (IncidentInterceptorUtility.isIntercepting_ThrumboPasses == WorkerPatchType.ExecuteOrigin)
				return true;
			if (IncidentInterceptorUtility.isIntercepting_ThrumboPasses == WorkerPatchType.Forestall)
			{
				IncidentInterceptorUtility.CreateIncidentCaravan_Animal<InterceptedIncident_AnimalHerd_ThrumboPasses>(__instance.def, parms);
				__result = true;
			}
			else
				__result = IncidentInterceptorUtility.tmpIncident.SubstituionWorkerExecution();
			return false;
		}
	}

	static class Patch_IncidentWorker_Alphabeavers_TryExecuteWorker
	{
		public static bool Prefix(IncidentWorker __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_Alphabeavers_TryExecuteWorker Prefix");
				Debug.DebugParms(parms, __instance.ToString());
			}

			if (IncidentInterceptorUtility.isIntercepting_Alphabeavers == WorkerPatchType.ExecuteOrigin)
				return true;
			if (IncidentInterceptorUtility.isIntercepting_Alphabeavers == WorkerPatchType.Forestall)
			{
				IncidentInterceptorUtility.CreateIncidentCaravan_Animal<InterceptedIncident_AnimalHerd_Alphabeavers>(__instance.def, parms);
				__result = true;
			}
			else
				__result = IncidentInterceptorUtility.tmpIncident.SubstituionWorkerExecution();
			return false;
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_AggressiveAnimals), "TryExecuteWorker")]
	static class Patch_IncidentWorker_ManhunterPack_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool Prefix(IncidentWorker_AggressiveAnimals __instance, ref bool __result, IncidentParms parms)
		{
			if (PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_ManhunterPack_TryExecuteWorker Prefix");
				Debug.DebugParms(parms, __instance.ToString());
			}
			if (parms.quest != null || parms.questScriptDef != null)
			{
				Log.Message("-=PS=- It's a quest! Bailout! MAYDAY!");
				return true;
			}
			if (IncidentInterceptorUtility.isIntercepting_ManhunterPack == WorkerPatchType.ExecuteOrigin)
				return true;
			if (IncidentInterceptorUtility.isIntercepting_ManhunterPack == WorkerPatchType.Forestall)
			{
				IncidentInterceptorUtility.CreateIncidentCaravan_Animal<InterceptedIncident_AnimalHerd_ManhunterPack>(__instance.def, parms);
				__result = true;
			}
			else
				__result = IncidentInterceptorUtility.tmpIncident.SubstituionWorkerExecution();
			return false;
		}
	}
	#endregion
}
