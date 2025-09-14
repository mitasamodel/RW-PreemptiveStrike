using PES.RW_JustUtils;
using RimWorld;
using Verse;

namespace PreemptiveStrike.Interceptor
{
	enum IncidentIntelLevel
	{
		Danger = 1,
		Unknown = 2,
		Neutral = 3
	}

	abstract class InterceptedIncident : IExposable
	{
		public IncidentCaravan.TravelingIncidentCaravan parentCaravan;
		public IncidentDef incidentDef;
		public IncidentParms parms;

		public LookTargets lookTargets = null;

		protected string incidentTitle_Confirmed = null;
		public abstract string IncidentTitle_Confirmed { get; }

		protected string incidentTitle_Unknow = null;
		public abstract string IncidentTitle_Unknown { get; }

		public abstract string IntentionStr { get; }

		public abstract IncidentIntelLevel IntelLevel { get; }

		public abstract bool IsHostileToPlayer { get; }

		public abstract void ExecuteNow();

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref incidentDef, "incidentDef");
			Scribe_Deep.Look(ref parms, "parms");
			Scribe_References.Look(ref parentCaravan, "parentCaravan");
			Scribe_Deep.Look(ref lookTargets, "lookTargets");
		}

		public virtual bool SubstituionWorkerExecution()
		{
			Logger.Log_Error("Substitution Worker not implemented!!!");
			return false;
		}
		
		public virtual bool ManualDeterminParams()
		{
			Logger.Log_Error("Manual Params Determination not implemented!!!");
			return false;
		}

		public abstract void RevealRandomInformation();

		public abstract void RevealAllInformation();

		public abstract void RevealInformationWhenCommunicationEstablished();

	}
}
