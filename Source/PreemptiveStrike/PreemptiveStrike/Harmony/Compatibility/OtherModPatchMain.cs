using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PreemptiveStrike.Harmony.Compatibility
{
    class OtherModPatchMain
    {
        public static void ModCompatibilityPatches()
        {
			//Lt.Bob - RimQuest.DoPatch throwing errors for some but RimQuest not v1.1 updated; Disabling
			/*
            if(RimQuest.IsModLoaded())
            {
                Log.Message("PES: Try to patch RimQuest");
                RimQuest.DoPatch();
            }
			*/
        }
    }
}
