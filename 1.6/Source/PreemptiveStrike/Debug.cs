using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PES.RW_JustUtils;

namespace PreemptiveStrike
{
	public static class Debug
	{
		public static void LogIfQuest(IncidentParms parms)
		{
			if (parms?.questTag != null || parms?.quest?.ToString() == "RimWorld.Quest")
				Logger.LogNL($"Quest: [{parms?.quest}] Tag[{parms?.questTag}]");
		}

		/// <summary>
		/// Lt.Bob - Unifies debug information to central command (__instance)
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="__instance"></param>
		public static void DebugParms(IncidentParms parms, string __instance = null)
		{
			Logger.LogNL("IncidentParms:");
			Logger.IncreaseTab();
			Logger.LogNL($"Full[{parms}]");
			Logger.LogNL($"Quest: [{parms.quest}] " +
				$"Parts[{parms.quest?.PartsListForReading}] " +
				$"Tag[{parms.questTag}] " +
				$"ScriptDef[{parms.questScriptDef}]");
			Logger.LogNL($"Instance[{__instance}]");
			Logger.DecreaseTab();
		}

		/// <summary>
		/// Lt.Bob - Unifies debug information to central command (IncDef)
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="IncDef"></param>
		public static void DebugParms(IncidentParms parms, IncidentDef IncDef = null)
		{
			Logger.LogNL("IncidentParms:");
			Logger.IncreaseTab();
			Logger.LogNL($"Full[{parms}]");
			Logger.LogNL($"Quest: [{parms.quest}] " +
				$"Parts[{parms.quest?.PartsListForReading}] " +
				$"Tag[{parms.questTag}] " +
				$"ScriptDef[{parms.questScriptDef}]");
			Logger.LogNL($"IncidentDef[{IncDef?.defName}]");
			Logger.DecreaseTab();
		}
	}
}
