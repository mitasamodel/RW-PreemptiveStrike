using PES.RW_JustUtils;
using PreemptiveStrike.Dialogue;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PreemptiveStrike.Interceptor
{
	internal class InterceptedIncident_SkyFaller_DropPodAssault : InterceptedIncident_SkyFaller
	{
		public override string IncidentTitle_Confirmed => "PES_Skyfaller_raid".Translate();

		public override bool IsHostileToPlayer => true;

		public override SkyFallerType FallerType => SkyFallerType.Small;

		public override void ConfirmMessage()
		{
			SparkUILetter.Make("PES_Warning_DropPodAssault".Translate(), "PES_Warning_DropPodAssault_Text".Translate(), LetterDefOf.ThreatBig, parentCaravan);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
		}

		public override bool PreCalculateDroppingSpot()
		{
			lookTargets = new TargetInfo(parms.spawnCenter, parms.target as Map);
			return true;
		}

		public override void ExecuteNow()
		{
			IncidentInterceptorUtility.isIntercepting_DropPodAssault = false;
			if (incidentDef != null && parms != null)
				incidentDef.Worker.TryExecute(parms);
			else
				Logger.Log_Error("No IncidentDef or parms in InterceptedIncident!");
			IncidentInterceptorUtility.isIntercepting_DropPodAssault = true;
		}
	}
}
