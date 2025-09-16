using PreemptiveStrike.DetectionSystem;
using PreemptiveStrike.Interceptor;
using UnityEngine;
using Verse;

namespace PreemptiveStrike.IncidentCaravan
{
	class TravelingIncidentCaravan_Simple : TravelingIncidentCaravan
	{
		public int RemainingRevealTick;
		public override Vector3 DrawPos => Vector3.zero;

		protected override void Tick()
		{
			//fail-safe
			if (remainingTick <= -10)
			{
				Find.WorldObjects.Remove(this);
			}

			--RemainingRevealTick;
			--remainingTick;
			if (CheckInVisionRange())
			{
				if (!detected)
				{
					detected = true;
					if (incident is InterceptedIncident_SkyFaller skyfallincident)
						skyfallincident.DetectMessage();
					if (incident is InterceptedIncident_MechCluster mechclusterinc)
						mechclusterinc.DetectMessage();
					EventManger.NotifyCaravanListChange?.Invoke();
				}
			}
			if (detected && CheckInVisionRange() && RemainingRevealTick <= 0)
				incident.RevealAllInformation();
			if (remainingTick <= 0)
			{
				Arrive();
				return;
			}
		}

		public bool CheckInVisionRange()
		{
			if (incident.parms.target == null) return false;
			Map map = incident.parms.target as Map;
			if (map == null) return false;
			return DetectDangerUtilities.GetVisionRangeOfMap(map.Tile) >= 1;
		}

		public override string CaravanTitle
		{
			get
			{
				if (incident.IntelLevel == IncidentIntelLevel.Unknown)
					return incident.IncidentTitle_Unknown;
				else
					return incident.IncidentTitle_Confirmed;
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			Tile = 0;
			Communicable = false;
			if (incident is InterceptedIncident_Infestation)
				detected = true;
			if (incident is InterceptedIncident_SolarFlare)
				detected = true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref RemainingRevealTick, "RemainingRevealTick", 0);
		}

		public override void TryNotifyCaravanIntel()
		{
			if (incident.IntelLevel == IncidentIntelLevel.Unknown)
				return;
			if (relationInformed)
				return;
			if (incident is InterceptedIncident_SkyFaller skyfallincident)
				skyfallincident.ConfirmMessage();
			else if (incident is InterceptedIncident_MechCluster mechclusterinc)
				mechclusterinc.ConfirmMessage();
			relationInformed = true;
		}
	}
}
