using PES.RW_JustUtils;
using PreemptiveStrike.Dialogue;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PreemptiveStrike.Interceptor
{
	internal class InterceptedIncident_MechCluster : InterceptedIncident_SkyFaller
	{
		public MechClusterSketch sketch;
		public IntVec3 center;

		public override string IncidentTitle_Confirmed => "PES_MechCluster".Translate();

		public override string IntentionStr => "PES_Intention_Raid".Translate();

		public override bool IsHostileToPlayer => true;

		public override SkyFallerType FallerType => SkyFallerType.Big;

		public override void ExecuteNow()
		{
			IncidentInterceptorUtility.tempMechClusterSketch = sketch;
			IncidentInterceptorUtility.tempCenter = center;
			IncidentInterceptorUtility.Interception_MechCluster = MechClusterWorkerType.Execute;
			try
			{
				if (incidentDef != null && parms != null)
					incidentDef.Worker.TryExecute(parms);
				else
					Logger.LogNL($"[InterceptedIncident_MechCluster.ExecuteNow] Unexpected null: incidentDef or parms");
			}
			finally
			{
				IncidentInterceptorUtility.Interception_MechCluster = MechClusterWorkerType.Ready;
				IncidentInterceptorUtility.tempMechClusterSketch = null;
				IncidentInterceptorUtility.tempCenter = IntVec3.Invalid;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref sketch, "sketch");
			Scribe_Values.Look(ref center, "center", IntVec3.Invalid);
		}

		public override void ConfirmMessage()
		{
			SparkUILetter.Make("PES_Warning_MechCluster".Translate(), "PES_Warning_MechCluster_Text".Translate(), LetterDefOf.ThreatBig, parentCaravan);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
		}

		public override bool PreCalculateDroppingSpot()
		{
			lookTargets = new TargetInfo(center, parms.target as Map);
			return true;
		}
	}
}
