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
	class InterceptedIncident_SkyFaller_RandomDrop : InterceptedIncident_SkyFaller_DropPodAssault
	{
		public override bool PreCalculateDroppingSpot()
		{
			lookTargets = null;
			return true;
		}
	}
}
