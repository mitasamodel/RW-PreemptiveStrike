using PES.RW_JustUtils;
using PreemptiveStrike.Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PreemptiveStrike.Interceptor
{
	internal class InterceptedIncident_HumanCrowd_RaidEnemy_DistributedGroups : InterceptedIncident_HumanCrowd_RaidEnemy_Groups
	{
		protected override void ResolveLookTargets()
		{
			var worker = parms.raidArrivalMode.Worker as PawnsArrivalModeWorker_EdgeWalkInDistributedGroups;
			if (worker == null)
			{
				Logger.Log_Error($"[InterceptedIncident_HumanCrowd_RaidEnemy_DistributedGroups.ResolveLookTargets] Unexpected worker null.");
				Verse.Log.Warning("[Preemptive Strike] Please report it to mod's author.");
			}

			//private List<Pair<List<Pawn>, IntVec3>> SplitIntoRandomGroupsNearMapEdge(List<Pawn> pawns, Map map)
			IncidentInterceptorUtility.IsIntercepting_GroupSpliter = GeneratorPatchFlag.Generate;

			// Generate groups using original's private method.
			_groupList = Patch_PawnsArrivalModeWorker_EdgeWalkInDistributedGroups_SplitIntoRandomGroupsNearMapEdge.Call(
				worker, pawnList, parms.target as Map);

			// Convert the list of pairs into something, which can be stored in save file with Expose.
			_storage = new GroupListStorage(_groupList);

			// Create Dictionary and store in parms.
			PawnsArrivalModeWorkerUtility.SetPawnGroupsInfo(parms, _groupList);

			// List of targets for indication.
			SetLookTargets();
		}
	}
}
