using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace PreemptiveStrike.Interceptor
{
	class InterceptedIncident_HumanCrowd_RaidEnemy_Groups : InterceptedIncident_HumanCrowd_RaidEnemy
	{
		protected List<Pair<List<Pawn>, IntVec3>> _groupList;
		protected GroupListStorage _storage;

		public override string IntentionStr => "PES_Intention_RaidGroup".Translate();

		protected override void ResolveLookTargets()
		{
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;

			// Generate groups from pawn list.
			_groupList = PawnsArrivalModeWorkerUtility.SplitIntoRandomGroupsNearMapEdge(pawnList, parms.target as Map, false);

			// Convert the list of pairs into something, which can be stored in save file with Expose.
			_storage = new GroupListStorage(_groupList);

			// Create Dictionary and store in parms.
			PawnsArrivalModeWorkerUtility.SetPawnGroupsInfo(parms, _groupList);

			// List of targets for indication.
			SetLookTargets();
		}

		public override void ExecuteNow()
		{
			IncidentInterceptorUtility.tempGroupList = _storage.RebuildList();
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.ReturnTempList;
			try
			{
				base.ExecuteNow();
			}
			finally
			{
				IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref _storage, "storage");
		}

		protected virtual void SetLookTargets()
		{
			var lookList = new List<TargetInfo>();
			foreach (var pair in _groupList)
			{
				if (pair.First.Count > 0)
					lookList.Add(new TargetInfo(pair.Second, parms.target as Map, false));
			}
			lookTargets = lookList;
		}
	}
}
