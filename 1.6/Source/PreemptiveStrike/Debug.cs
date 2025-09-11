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
		/// <summary>
		/// Lt.Bob - Unifies debug information to central command (__instance)
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="__instance"></param>
		public static void DebugParms(IncidentParms parms, string __instance = null)
		{
			Logger.LogNL("\t IncidentParms:");
			Logger.LogNL($"\t\t Full[{parms}]");
			Logger.LogNL($"\t\t Quest: [{parms.quest}] " +
				$"Parts[{parms.quest?.PartsListForReading}] " +
				$"Tag[{parms.questTag}] " +
				$"ScriptDef[{parms.questScriptDef}]");
			Logger.LogNL($"\t\t Instance[{__instance}]");
		}

		/// <summary>
		/// Lt.Bob - Unifies debug information to central command (IncDef)
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="IncDef"></param>
		public static void DebugParms(IncidentParms parms, IncidentDef IncDef = null)
		{
			Logger.LogNL("\t IncidentParms:");
			Logger.LogNL($"\t\t Full[{parms}]");
			Logger.LogNL($"\t\t Quest: [{parms.quest}] " +
				$"Parts[{parms.quest?.PartsListForReading}] " +
				$"Tag[{parms.questTag}] " +
				$"ScriptDef[{parms.questScriptDef}]");
			Logger.LogNL($"\t\t IncidentDef[{IncDef?.defName}]");
		}
	}
}
