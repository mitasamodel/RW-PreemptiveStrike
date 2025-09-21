using HarmonyLib;
using PES;
using PES.RW_JustUtils;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PreemptiveStrike.Harmony
{
	[HarmonyPatch]
	internal static class StorytellerPatches
	{
		/// <summary>
		/// Debugging only. Log Storeteller fires.
		/// </summary>
		[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.TryFire))]
		internal static bool Prefix(FiringIncident fi, bool queued, MethodBase __originalMethod)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL();
				Logger.LogNL($"[{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}] Prefix. TickS[{Find.TickManager.TicksSinceSettle / 60}]");
				using var _ = Logger.Scope();

				Logger.LogNL($"Incident Def[{fi.def}] Cat[{fi.def.category}] Queued[{queued}]");
				Logger.LogNL($"Parms [{fi.parms}]");

				Debug.IncidentTicks(fi.parms, fi.def);
			}
			return true;
		}

		[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.TryFire))]
		internal static void Postfix(bool __result, FiringIncident fi, bool queued, MethodBase __originalMethod)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}] Postfix.");
				if (__result) Logger.LogNL($"\tFired [{__result}] def[{fi.def.defName}]");

				Debug.IncidentTicks(fi.parms, fi.def);
			}
		}
	}
}
