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
		/// 
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="instance"></param>
		public static void DebugParms(IncidentParms parms, IncidentWorker instance)
		{
			DebugParms(parms, instance?.def?.defName);
		}

		/// <summary>
		/// Lt.Bob - Unifies debug information to central command (IncDef)
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="IncDef"></param>
		public static void DebugParms(IncidentParms parms, IncidentDef def)
		{
			DebugParms(parms, def?.defName);
		}

		public static void DebugParms(IncidentParms parms, string name = null)
		{
			Logger.LogNL("IncidentParms:");
			using var _ = Logger.Scope();
			Logger.LogNL($"Full[{parms}]");
			Logger.LogNL($"Quest: [{parms.quest}] " +
				$"Parts[{parms.quest?.PartsListForReading}] " +
				$"Tag[{parms.questTag}] " +
				$"ScriptDef[{parms.questScriptDef}]");
			Logger.LogNL($"IncidentDef[{name}]");
		}

	}
}
