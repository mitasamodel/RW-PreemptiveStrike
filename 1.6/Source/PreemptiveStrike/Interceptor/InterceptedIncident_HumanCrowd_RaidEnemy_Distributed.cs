using PES.RW_JustUtils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace PreemptiveStrike.Interceptor
{
	/// <summary>
	/// Each pawn has its own random location.
	/// </summary>
	internal class InterceptedIncident_HumanCrowd_RaidEnemy_Distributed : InterceptedIncident_HumanCrowd_RaidEnemy
	{
		private List<IntVec3> _entryCells = new List<IntVec3>();

		protected override void ResolveLookTargets()
		{
			var lookList = new List<TargetInfo>();

			int i;
			for (i = 0; i < pawnList.Count; i++)
			{
				const int tries = 100;
				bool flag = false;
				Map map = parms.target as Map;
				for (int j = 0; j < tries; j++)
				{
					if (RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Ignore))
					{
						_entryCells.Add(result);
						lookList.Add(new TargetInfo(result, map));
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Logger.Log_Error($"Cannot generate any more pawns. Need [{pawnList.Count}], generated [{i}]. Tried [{tries}] times. Stop trying.");
					break;
				}
			}

			// Strip end of the list if we exited earlier.
			pawnList.RemoveRange(i, pawnList.Count - i);
			// Targets.
			lookTargets = lookList;
		}

		public override void ExecuteNow()
		{
			IncidentInterceptorUtility.TryFindRandomPawnEntryCell = GeneratorPatchFlag.ReturnTempList;
			IncidentInterceptorUtility.tempEntryCells = _entryCells;
			try
			{
				base.ExecuteNow();
			}
			finally
			{
				IncidentInterceptorUtility.tempEntryCells = null;
				IncidentInterceptorUtility.TryFindRandomPawnEntryCell = GeneratorPatchFlag.Generate;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref _entryCells, "distributedEntryCells", LookMode.Value);
		}
	}
}
