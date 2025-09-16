using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PES;
using PreemptiveStrike.Mod;
using PES.RW_JustUtils;
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
		internal static void Postfix(bool __result, FiringIncident fi, bool queued, MethodBase __originalMethod)
		{
			if (PES_Settings.DebugModeOn)
			{
				Logger.LogNL($"[{__originalMethod.DeclaringType.Name}.{__originalMethod.Name}] TickS[{Find.TickManager.TicksSinceSettle / 60}]");
				using var _ = Logger.Scope();

				Logger.LogNL($"Incident Def[{fi.def}] Queued[{queued}]");
				Logger.LogNL($"Parms [{fi.parms}]");
				if (__result) Logger.LogNL($"Fired [{__result}]");
			}
		}
	}
}
