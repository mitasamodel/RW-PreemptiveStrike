using PES.RW_JustUtils;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
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
				Logger.LogNL($"\tBypass[{parms.bypassStorytellerSettings}] Forced[{parms.forced}] Full[{parms}]");
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

		public static void IncidentTicks(IncidentParms parms, IncidentDef incDef)
		{
			int ticksGame = Find.TickManager.TicksGame;
			var lastFireTicks = parms.target.StoryState.lastFireTicks;
			var lastThreatBigTick = parms.target.StoryState.LastThreatBigTick;

			Logger.LogNL($"LastThreatBigTick [{lastThreatBigTick}] TicksGame[{ticksGame}] dd[{(ticksGame - lastThreatBigTick) / GenDate.TicksPerDay}]");

			if (lastFireTicks.TryGetValue(incDef, out var value))
				Logger.LogNL($"Last fired[{value}] TicksGame[{ticksGame}] dd[{(ticksGame - value) / GenDate.TicksPerDay}] min refire days[{incDef.minRefireDays}]");

			List<IncidentDef> refireCheckIncidents = incDef.RefireCheckIncidents;
			if (refireCheckIncidents != null)
			{
				for (int i = 0; i < refireCheckIncidents.Count; i++)
				{
					if (lastFireTicks.TryGetValue(refireCheckIncidents[i], out value))
						Logger.LogNL($"refire[{refireCheckIncidents[i].defName}]: Last fired[{value}] TicksGame[{ticksGame}] dd[{(ticksGame - value) / GenDate.TicksPerDay}] min refire days[{incDef.minRefireDays}]");
				}
			}
		}

		public static void LogCrowedSize(List<Pawn> pawnList)
		{
			Logger.LogNL($"CrowedSize revealed!!!");
			StringBuilder sb = new StringBuilder();
			sb.AppendTab($"Pawn number: {pawnList.Count}\n");
			foreach (var x in pawnList)
			{
				if (x.IsAnimal)
					sb.AppendTab($"{x.KindLabel}\n");
				else
					sb.AppendTab($"{x.Name}\n");
			}
			Logger.Log(sb.ToString());
			Logger.LogNL();
		}
	}
}
