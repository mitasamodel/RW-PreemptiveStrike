using HarmonyLib;
using PES.RW_JustUtils;
using PreemptiveStrike.Interceptor;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace PreemptiveStrike.Harmony
{
	internal static class Helper_PawnsArrivalModeWorker_Classes_Simple
	{
		internal static readonly Type[] TargetTypes = new[]
		{
			typeof(PawnsArrivalModeWorker_EdgeDrop),
			typeof(PawnsArrivalModeWorker_CenterDrop),
			typeof(PawnsArrivalModeWorker_EdgeDropGroups),
			typeof(PawnsArrivalModeWorker_RandomDrop),
		};
	}

	[HarmonyPatch]
	class Patch_TryResolveRaidSpawnCenter_Common
	{
		private static IEnumerable<MethodBase> TargetMethods()
		{
			foreach (var type in Helper_PawnsArrivalModeWorker_Classes_Simple.TargetTypes)
				yield return AccessTools.Method(type, "TryResolveRaidSpawnCenter");
		}

		[HarmonyPostfix]
		static void PostFix(object __instance, MethodBase __originalMethod, IncidentParms parms, ref bool __result)
		{
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"[{__instance.GetType().Name}.{__originalMethod.Name}] Postfix.");
			using var _ = Logger.Scope();
			if (PES_Settings.DebugModeOn)
				Debug.DebugParms(parms);

			if (Helper.IsQuest(parms))
				return;

			switch (__instance)
			{
				case PawnsArrivalModeWorker_RandomDrop _:
					if (IncidentInterceptorUtility.isIntercepting_RandomDrop)
						__result = !IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_RandomDrop>(
							IncidentInterceptorUtility.CurrentIncidentDef, parms, true, true);
					break;
				case PawnsArrivalModeWorker_EdgeDropGroups _:
					if (IncidentInterceptorUtility.isIntercepting_EdgeDropGroup)
						__result = !IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_EdgeDropGroup>(
							IncidentInterceptorUtility.CurrentIncidentDef, parms, true);
					break;
				case PawnsArrivalModeWorker_CenterDrop _:
					if (IncidentInterceptorUtility.isIntercepting_CenterDrop)
						__result = !IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_CenterDrop>(
							IncidentInterceptorUtility.CurrentIncidentDef, parms, true, true);
					break;
				case PawnsArrivalModeWorker_EdgeDrop _:
					if (IncidentInterceptorUtility.isIntercepting_EdgeDrop)
						__result = !IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_EdgeDrop>(
							IncidentInterceptorUtility.CurrentIncidentDef, parms, true, true);
					break;
				default:
					Logger.Log_Error($"Drop incident not implemented. Please report it to mod author\n" +
						$"[{__instance.GetType().Name}.{__originalMethod.Name}]");
					Debug.DebugParms(parms, toConsole: true);
					break;
			}
		}
	}

	[HarmonyPatch(typeof(CellFinderLoose), "TryFindSkyfallerCell")]
	class Patch_CellFinderLoose_TryFindSkyfallerCell
	{
		[HarmonyPrefix]
		static bool PreFix(ref IntVec3 cell, ref bool __result)
		{
			if (IncidentInterceptorUtility.IsIntercepting_SkyfallerCell_Loose == GeneratorPatchFlag.Generate)
			{
				return true;
			}
			else if (IncidentInterceptorUtility.IsIntercepting_SkyfallerCell_Loose == GeneratorPatchFlag.ReturnZero)
			{
				cell = IntVec3.Zero;
				__result = false;
				return false;
			}
			else
			{
				cell = IncidentInterceptorUtility.tempSkyfallerCellLoose;
				__result = true;
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(InfestationCellFinder), "TryFindCell")]
	class Patch_InfestationCellFinder_TryFindCell
	{
		[HarmonyPrefix]
		static bool Prefix(ref IntVec3 cell, ref bool __result)
		{
			if (IncidentInterceptorUtility.IsIntercepting_InfestationCell == GeneratorPatchFlag.Generate)
			{
				return true;
			}
			else if (IncidentInterceptorUtility.IsIntercepting_InfestationCell == GeneratorPatchFlag.ReturnZero)
			{
				cell = IntVec3.Zero;
				__result = false;
				return false;
			}
			else
			{
				cell = IncidentInterceptorUtility.tempInfestationCell;
				__result = true;
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(DropCellFinder), "RandomDropSpot")]
	class Patch_DropCellFinder_RandomDropSpot
	{
		[HarmonyPrefix]
		static bool Prefix(ref IntVec3 __result)
		{
			if (IncidentInterceptorUtility.IsIntercepting_RandomDropSpot == GeneratorPatchFlag.Generate)
			{
				return true;
			}
			else if (IncidentInterceptorUtility.IsIntercepting_RandomDropSpot == GeneratorPatchFlag.ReturnZero)
			{
				__result = IntVec3.Zero;
				return false;
			}
			else
			{
				__result = IncidentInterceptorUtility.tempRandomDropCell;
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_MeteoriteImpact), "TryExecuteWorker")]
	class Patch_MeteoriteImpact_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool PreFix(ref bool __result, IncidentParms parms)
		{
			if (IncidentInterceptorUtility.IsIntercepting_Meteorite == WorkerPatchType.ExecuteOrigin)
				return true;
			else
			{
				if (!IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_MeteoriteImpact>(DefDatabase<IncidentDef>.GetNamed("MeteoriteImpact"), parms))
					return true;
				__result = true;
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_ShipChunkDrop), "TryExecuteWorker")]
	class Patch_ShipChunkDrop_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool PreFix(ref bool __result, IncidentParms parms)
		{
			if (IncidentInterceptorUtility.IsIntercepting_ShipChunk == WorkerPatchType.ExecuteOrigin)
				return true;
			else
			{
				if (!IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_ShipChunk>(DefDatabase<IncidentDef>.GetNamed("ShipChunkDrop"), parms))
					return true;
				__result = true;
				return false;
			}
		}
	}

	//Disabled for V1.3
	/*[HarmonyPatch(typeof(IncidentWorker_TransportPodCrash), "TryExecuteWorker")]
    class Patch_TransportPod_TryExecuteWorker
    {
        [HarmonyPrefix]
        static bool PreFix(ref bool __result, IncidentParms parms)
        {
            if (IncidentInterceptorUtility.IsIntercepting_TransportPod == WorkerPatchType.ExecuteOrigin)
                return true;
            else
            {
                if (!IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_TransportPod>(DefDatabase<IncidentDef>.GetNamed("RefugeePodCrash"), parms))
                    return true;
                __result = true;
                return false;
            }
        }
    }*/

	[HarmonyPatch(typeof(IncidentWorker_ResourcePodCrash), "TryExecuteWorker")]
	class Patch_ResourcePod_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool PreFix(ref bool __result, IncidentParms parms)
		{
			if (IncidentInterceptorUtility.IsIntercepting_ResourcePod == WorkerPatchType.ExecuteOrigin)
				return true;
			else
			{
				if (!IncidentInterceptorUtility.Intercept_SkyFaller<InterceptedIncident_SkyFaller_ResourcePod>(DefDatabase<IncidentDef>.GetNamed("ResourcePodCrash"), parms))
					return true;
				__result = true;
				return false;
			}
		}
	}



	[HarmonyPatch(typeof(IncidentWorker_Infestation), "TryExecuteWorker")]
	class Patch_Infestation_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool PreFix(ref bool __result, IncidentParms parms)
		{
			if (IncidentInterceptorUtility.IsIntercepting_Infestation == WorkerPatchType.ExecuteOrigin)
				return true;
			else
			{
				if (!IncidentInterceptorUtility.Intercept_Infestation(parms))
					return true;
				__result = true;
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(IncidentWorker_MakeGameCondition), "TryExecuteWorker")]
	class Patch_MakeGameCondition_TryExecuteWorker
	{
		[HarmonyPrefix]
		static bool PreFix(IncidentWorker_MakeGameCondition __instance, ref bool __result, IncidentParms parms)
		{
			//return __instance.def != IncidentDefOf.SolarFlare || !IncidentInterceptorUtility.Intercept_SolarFlare(parms);
			if (__instance.def != IncidentDefOf.SolarFlare || IncidentInterceptorUtility.IsIntercepting_SolarFlare == WorkerPatchType.ExecuteOrigin)
			{
				return true;
			}
			{
				if (!IncidentInterceptorUtility.Intercept_SolarFlare(parms))
					return true;
				__result = true;
				return false;
			}
		}
	}

}
