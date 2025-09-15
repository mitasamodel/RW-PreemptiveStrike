using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PreemptiveStrike.Dialogue;
using PreemptiveStrike.Mod;
using RimWorld;
using Verse;

namespace PreemptiveStrike.Interceptor
{
	class InterceptedIncident_SkyFaller_EdgeDropGroup : InterceptedIncident_SkyFaller_DropPodAssault
	{
		List<Pawn> pawnList;
		List<Pair<List<Pawn>, IntVec3>> GroupList;
		private GroupListStorage storage;

		public override bool PreCalculateDroppingSpot()
		{
			pawnList = IncidentInterceptorUtility.GenerateRaidPawns(parms);
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;
			GroupList = PawnsArrivalModeWorkerUtility.SplitIntoRandomGroupsNearMapEdge(pawnList, parms.target as Map, false);
			storage = new GroupListStorage(GroupList);
			PawnsArrivalModeWorkerUtility.SetPawnGroupsInfo(parms, GroupList);
			var list1 = new List<TargetInfo>();
			foreach (var pair in GroupList)
			{
				if (pair.First.Count > 0)
					list1.Add(new TargetInfo(pair.Second, parms.target as Map, false));
			}
			lookTargets = list1;
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawnList, "pawnList", LookMode.Deep);
			Scribe_Deep.Look(ref storage, "storage");
		}

		public override void ExecuteNow()
		{
			IncidentInterceptorUtility.tempGroupList = storage.RebuildList();
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.ReturnTempList;
			IncidentInterceptorUtility.IsIntercepting_PawnGeneration = GeneratorPatchFlag.ReturnTempList;
			IncidentInterceptorUtility.tmpPawnList = pawnList;
			try
			{
				base.ExecuteNow();
			}
			finally
			{
				IncidentInterceptorUtility.tmpPawnList = null;
				IncidentInterceptorUtility.IsIntercepting_PawnGeneration = GeneratorPatchFlag.Generate;
				IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;
				IncidentInterceptorUtility.tempGroupList = null;
			}
		}
	}
}
