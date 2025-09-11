using PES.RW_JustUtils;
using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreemptiveStrike
{
	public static class Helper
	{
		public static bool IsQuest(IncidentParms parms)
		{
			bool result = (!string.IsNullOrEmpty(parms?.questTag)) || (parms?.quest is Quest);
			if (PES_Settings.DebugModeOn)
				Logger.LogNL($"[IsQuest] Result[{result}] Tag[{parms?.questTag}] Quest[{parms?.quest}]");
			return result;
		}
	}
}
