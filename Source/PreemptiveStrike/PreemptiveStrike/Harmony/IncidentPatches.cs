using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using PreemptiveStrike.Interceptor;
using Verse;

namespace PreemptiveStrike.Harmony
{
    [HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
    class Patch_IncidentWorker_TryExecute
    {

		[HarmonyPrefix]
		static bool Prefix(IncidentWorker __instance, ref bool __result, IncidentParms parms)
		{
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_TryExecute Prefix"); //Lt. Bob - Logging
				Log.Message("-=PS=- parms: " + parms.ToString());
				if (parms.quest != null)
					Log.Message("-=PS=- parms.quest: " + parms.quest.ToString());
				if (parms.questScriptDef != null)
					Log.Message("-=PS=- parms.questScriptDef: " + parms.questScriptDef.ToString());
				if (parms.questScriptDef != null)
					Log.Message("-=PS=- parms.questTag: " + parms.questTag.ToString());
				Log.Message("-=PS=- __instance= " + __instance.ToString());
			}

			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_TryExecute - questTag!=Null == " + parms.questTag);
			if (parms.quest != null)
			{
				Log.Message("-=PS=- It's a quest! Bailout! MAYDAY!");
				//__result = true;
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
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_IncidentWorker_TryExecute Postfix"); //Lt. Bob - Logging

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
        [HarmonyPrefix]
        static void Prefix(IncidentWorker_RaidEnemy __instance)
        {
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- IncidentWorker_RaidEnemy Prefix"); //Lt. Bob - Logging

			IncidentInterceptorUtility.CurrentIncidentDef = __instance.def;
        }
    }

    //----------------------------------------------------------

    #region Raid Patches
    [HarmonyPatch(typeof(PawnsArrivalModeWorker_EdgeWalkIn), "TryResolveRaidSpawnCenter")]
    static class Patch_EdgeWalkIn_TryResolveRaidSpawnCenter
    {
        [HarmonyPostfix]
        static void Postfix(PawnsArrivalModeWorker_EdgeWalkIn __instance, IncidentParms parms, ref bool __result)
        {
			//return;
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_EdgeWalkIn_TryResolveRaidSpawnCenter Postfix"); //Lt. Bob - Logging
				Log.Message("-=PS=- parms: " + parms.ToString());
				if(parms.quest != null)
					Log.Message("-=PS=- parms.quest: " + parms.quest.ToString());
				if(parms.questScriptDef != null)
					Log.Message("-=PS=- parms.questScriptDef: " + parms.questScriptDef.ToString());
				if(parms.questScriptDef != null)
					Log.Message("-=PS=- parms.questTag: " + parms.questTag.ToString());
				Log.Message("-=PS=- __instance= " + __instance.ToString());
				/*foreach (Quest questInt in Find.QuestManager.QuestsListForReading)
				{
					Log.Message(questInt.PartsListForReading.ToString());
				}*/
			}
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_EdgeWalkIn_TryResolveRaidSpawnCenter - questTag!=Null == " + parms.questTag);
			if (parms.faction == null || parms.faction.PlayerRelationKind != FactionRelationKind.Hostile)	//Lt. Bob - bypass handler for quest given neutral pawns; Why is faction ==null?
			{
				Log.Message("-=PS=- parms.faction == null or faction is nonhostile");
				return;
			}

			//This is a temporary fix for refugee chased
			if (IncidentInterceptorUtility.IncidentInQueue(parms, IncidentDefOf.RaidEnemy))
                return;

            if (IncidentInterceptorUtility.IsIntercepting_IncidentExcecution)
            {
                if (IncidentInterceptorUtility.Intercept_Raid(parms))
                    __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PawnsArrivalModeWorker_EdgeWalkInGroups), "TryResolveRaidSpawnCenter")]
    static class Patch_EdgeWalkInGroups_TryResolveRaidSpawnCenter
    {
        [HarmonyPostfix]
        static void Postfix(PawnsArrivalModeWorker_EdgeWalkIn __instance, IncidentParms parms, ref bool __result)
        {
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_EdgeWalkInGroups_TryResolveRaidSpawnCenter Postfix"); //Lt. Bob - Logging
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_EdgeWalkInGroups_TryResolveRaidSpawnCenter - questTag!=Null == " + parms.questTag);

			if (IncidentInterceptorUtility.IsIntercepting_IncidentExcecution)
            {
                if (IncidentInterceptorUtility.Intercept_Raid(parms, true))
                    __result = false;
            }
        }
    }
    #endregion

    #region Pawn Generation Patches
    [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
    static class Patch_PawnGroupMakerUtility_GeneratePawns
    {
        [HarmonyPrefix]
        static bool Prefix(ref IEnumerable<Pawn> __result)
        {
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_PawnGroupMakerUtility_GeneratePawns Prefix"); //Lt. Bob - Logging

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
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_PawnsArrivalModeWorkerUtility_SplitIntoRandomGroupsNearMapEdge Prefix"); //Lt. Bob - Logging

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
    #endregion

    #region Human Netral Patch

    [HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "TryExecuteWorker")]
    static class Patch_IncidentWorker_TraderCaravanArrival_TryExecuteWorker
    {
        [HarmonyPrefix]
        static bool Prefix(IncidentWorker_TraderCaravanArrival __instance, ref bool __result, IncidentParms parms)
        {
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_IncidentWorker_TraderCaravanArrival_TryExecuteWorker Prefix"); //Lt. Bob - Logging
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_TraderCaravanArrival_TryExecuteWorker - questTag!=Null == " + parms.questTag);

			if (IncidentInterceptorUtility.isIntercepting_TraderCaravan_Worker)
                return !IncidentInterceptorUtility.CreateIncidentCaraven_HumanNeutral<InterceptedIncident_HumanCrowd_TraderCaravan>(__instance.def, parms);
            return true;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_TravelerGroup), "TryExecuteWorker")]
    static class Patch_IncidentWorker_TravelerGroup_TryExecuteWorker
    {
        [HarmonyPrefix]
        static bool Prefix(IncidentWorker_TravelerGroup __instance, ref bool __result, IncidentParms parms)
        {
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
				Log.Message("-=PS=- Patch_IncidentWorker_TravelerGroup_TryExecuteWorker Prefix"); //Lt. Bob - Logging
			if (parms != null && parms.questTag != null)    //Lt. Bob - "Temporary" bypass fix? for Quest handling
			{
				Log.Message("-=PS=- Patch_IncidentWorker_TravelerGroup_TryExecuteWorker - questTag!=Null == " + parms.questTag);
				Log.Message("-=PS=- Returning true");
				return true;
			}
			if (IncidentInterceptorUtility.isIntercepting_TravelerGroup)
                return !IncidentInterceptorUtility.CreateIncidentCaraven_HumanNeutral<InterceptedIncident_HumanCrowd_TravelerGroup>(__instance.def, parms);
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
			if (parms != null && parms.questTag != null)    //Lt. Bob - "Temporary" bypass fix? for Quest handling
			{
				Log.Message("-=PS=- Patch_IncidentWorker_VisitorGroup_TryExecuteWorker - questTag!=Null == " + parms.questTag);
				Log.Message("-=PS=- Returning true");
				return true;
			}
			if (IncidentInterceptorUtility.isIntercepting_VisitorGroup)
                return !IncidentInterceptorUtility.CreateIncidentCaraven_HumanNeutral<InterceptedIncident_HumanCrowd_VisitorGroup>(__instance.def, parms);
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
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_FarmAnimalsWanderIn_TryExecuteWorker - questTag!=Null == " + parms.questTag);

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
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_HerdMigration_TryExecuteWorker - questTag!=Null == " + parms.questTag);

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
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_ThrumboPasses_TryExecuteWorker - questTag!=Null == " + parms.questTag);

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
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_Alphabeavers_TryExecuteWorker - questTag!=Null == " + parms.questTag);

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

    [HarmonyPatch(typeof(IncidentWorker_ManhunterPack), "TryExecuteWorker")]
    static class Patch_IncidentWorker_ManhunterPack_TryExecuteWorker
    {
        [HarmonyPrefix]
        static bool Prefix(IncidentWorker_ManhunterPack __instance, ref bool __result, IncidentParms parms)
        {
			if (PreemptiveStrike.Mod.PES_Settings.DebugModeOn)
			{
				Log.Message("-=PS=- Patch_IncidentWorker_ManhunterPack_TryExecuteWorker Prefix"); //Lt. Bob - Logging
				Log.Message("-=PS=- parms: " + parms.ToString());
				if (parms.quest != null)
					Log.Message("-=PS=- parms.quest: " + parms.quest.ToString());
				if (parms.questScriptDef != null)
					Log.Message("-=PS=- parms.questScriptDef: " + parms.questScriptDef.ToString());
				if (parms.questScriptDef != null)
					Log.Message("-=PS=- parms.questTag: " + parms.questTag.ToString());
				Log.Message("-=PS=- __instance= " + __instance.ToString());
			}
			if (parms != null && parms.questTag != null) //Lt. Bob - "Temporary" bypass fix? for Quest handling
				Log.Message("-=PS=- Patch_IncidentWorker_ManhunterPack_TryExecuteWorker - questTag!=Null == " + parms.questTag);
			if (parms != null && parms.questTag != null)    //Lt. Bob - "Temporary" bypass fix? for Quest handling
			{
				Log.Message("-=PS=- Patch_IncidentWorker_ManhunterPack_TryExecuteWorker - questTag!=Null == " + parms.questTag);
				Log.Message("-=PS=- Returning true");
				return true;
			}
			if (parms.quest != null)
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
