using PES.RW_JustUtils;
using RimWorld;
using System.Text;
using Verse;

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
		public static void DebugParms(IncidentParms parms, Def def)
		{
			DebugParms(parms, def?.defName);
		}

		public static void DebugParms(IncidentParms parms, string name = null, bool toConsole = false)
		{
			if (!toConsole)
			{
				Logger.LogNL("IncidentParms:");
				Logger.LogNL($"\tFull[{parms}]");
				Logger.LogNL($"\tQuest: [{parms.quest}] " +
					$"Parts[{parms.quest?.PartsListForReading}] " +
					$"Tag[{parms.questTag}] " +
					$"ScriptDef[{parms.questScriptDef}]");
				Logger.LogNL($"\tIncidentDef[{name}]");
			}
			else
			{
				var sb = new StringBuilder();
				sb.Append("IncidentParms:\n" +
					$"Full[{parms}]\n" +
					$"Quest: [{parms.quest}] " +
					$"Parts[{parms.quest?.PartsListForReading}] " +
					$"Tag[{parms.questTag}] " +
					$"ScriptDef[{parms.questScriptDef}]\n" +
					$"IncidentDef[{name}]");

				Verse.Log.Message(sb.ToString());
			}
		}

	}
}
