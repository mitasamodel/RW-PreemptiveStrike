using PreemptiveStrike.IncidentCaravan;
using PreemptiveStrike.Interceptor;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
//using UnityEngine;
using Verse;

using PES.RW_JustUtils;

namespace PreemptiveStrike.RaidGoal
{

	static class RaidingGoalUtility
	{
		public static void ResolveRaidGoal(InterceptedIncident_HumanCrowd_RaidEnemy incident)
		{
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"[RaidingGoalUtility.ResolveRaidGoal]");
			using var _ = Logger.Scope();
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"Faction[{incident.SourceFaction}]");

			if (incident.SourceFaction == Faction.OfMechanoids)
			{
				new RaidingGoal_Extermination().ApplyToIncident(incident);
				if (PES_Settings.DebugModeOn)
					Logger.LogNL($"Raid goal [{RaidGoalType.Extermination}]");
				return;
			}

			Map map = incident.parms.target as Map;
			Dictionary<RaidGoalType, float> weightDic = new Dictionary<RaidGoalType, float>()
			{
				[RaidGoalType.Conquer] = 1f,
				[RaidGoalType.Extortion] = 1f,
				[RaidGoalType.Enslave] = 1f,
				[RaidGoalType.Rescue] = 1f
			};

			float ConquerWeight = 0f;
			if (incident.parms.points < 1000f)
				ConquerWeight = 0f;
			else if (incident.parms.points < 3000f)
				ConquerWeight = 1f;
			else
				ConquerWeight = 2f;
			weightDic[RaidGoalType.Conquer] = ConquerWeight;

			float EnslaveWeight = 0f;
			if (map.mapPawns.FreeColonistsSpawnedCount <= 1)
				EnslaveWeight = 0f;
			else
				EnslaveWeight = UnityEngine.Mathf.Clamp01(map.mapPawns.FreeColonistsSpawnedCount / 10f) * 0.8f;
			weightDic[RaidGoalType.Enslave] = EnslaveWeight;

			float RescueWeight = 0f;
			if (map.mapPawns.PrisonersOfColonySpawnedCount < 1)
				RescueWeight = 0f;
			else
			{
				int factionPrisonerSum = map.mapPawns.PrisonersOfColonySpawned.Sum(p => !p.Dead && p.Faction == incident.SourceFaction ? 1 : 0);
				if (factionPrisonerSum == 0)
					RescueWeight = 0f;
				else if (factionPrisonerSum <= 3)
					RescueWeight = 1f;
				else
					RescueWeight = 2f;
			}
			weightDic[RaidGoalType.Rescue] = RescueWeight;

			RaidGoalType theChosenOne = weightDic.Keys.ToList().RandomElementByWeight(t => weightDic[t]);
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"Raid goal estimated [{theChosenOne}]");
			RaidingGoal goal = Activator.CreateInstance(RaidingGoal.GetRaidClassByEnum(theChosenOne)) as RaidingGoal;
			goal.ApplyToIncident(incident);
		}

		public static void RebuffDemandAndSmiteThePlayer(TravelingIncidentCaravan caravan, Pawn pawn)
		{
			if (!(caravan.incident is InterceptedIncident_HumanCrowd_RaidEnemy incident))
				return;
			caravan.Communicable = false;
			if (incident.raidGoalType == RaidGoalType.Rescue)
			{
				RaidingGoal_Rescue goal = incident.goal as RaidingGoal_Rescue;
				foreach (var p in goal.Prisoners)
				{
					//p.guilt.lastGuiltyTick = Find.TickManager.TicksGame;  //Disabled for V1.3
					ExecutionUtility.DoExecutionByCut(pawn, p);
				}
			}
			incident.goal = new RaidingGoal_Smite();
			if (pawn.skills.GetSkill(SkillDefOf.Social).Level < 5)
				incident.CombatMoral = -1;
			else if (pawn.skills.GetSkill(SkillDefOf.Social).Level < 10)
				incident.CombatMoral = 0;
			else
				incident.CombatMoral = 1;
		}

		public static void CombatMoralResolver(InterceptedIncident_HumanCrowd_RaidEnemy incident)
		{
			Map map = incident.parms.target as Map;
			var colonists = map.mapPawns.FreeColonists;
			HediffDef hediffPos = PESDefOf.PES_CombatFervor;
			HediffDef hediffNeg = PESDefOf.PES_CombatTiredness;
			if (incident.CombatMoral == 0)
				return;
			foreach (var pawn in colonists)
			{
				if (incident.CombatMoral == 1)
				{
					if (pawn.health.hediffSet.HasHediff(hediffNeg))
					{
						foreach (var hediff in pawn.health.hediffSet.hediffs)
						{
							if (hediff.def == hediffNeg)
							{
								pawn.health.RemoveHediff(hediff);
								break;
							}
						}
					}
					pawn.health.AddHediff(hediffPos, null, null, null);
				}
				else
				{
					if (!pawn.health.hediffSet.HasHediff(hediffPos))
						pawn.health.AddHediff(hediffNeg, null, null, null);
				}
			}
		}

		public static int SilverInMap(Map map)
		{
			var stacks = map.listerThings.ThingsOfDef(ThingDefOf.Silver);
			if (stacks == null || stacks.Count <= 0)
				return 0;
			else
				return stacks.Sum(stack => stack.stackCount);
		}

		public static int GoldInMap(Map map)
		{
			var stacks = map.listerThings.ThingsOfDef(ThingDefOf.Gold); ;
			if (stacks == null || stacks.Count <= 0)
				return 0;
			else
				return stacks.Sum(stack => stack.stackCount);
		}
	}
}
