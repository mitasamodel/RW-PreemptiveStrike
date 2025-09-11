using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace PreemptiveStrike.Harmony
{
	[StaticConstructorOnStartup]
	class HarmonyMain
	{
		public static HarmonyLib.Harmony instance;  //Lt. Bob: 1.1

		static HarmonyMain()
		{
			instance = new HarmonyLib.Harmony("DrCarlLuo.Rimworld.PreemptiveStrike");   //Lt. Bob: 1.1
			instance.PatchAll(Assembly.GetExecutingAssembly());
			ManualPatchings();
		}

		static void ManualPatchings()
		{
			//This alphabeaver one is f**king special
			//Why it has to be an INTERNAL CLASS, WHYYYYYYYYYYYYYYYYYYYYYY?????
			MethodInfo prefix = typeof(Patch_IncidentWorker_Alphabeavers_TryExecuteWorker).GetMethod("Prefix");
			instance.Patch(AccessTools.Method(AccessTools.TypeByName("RimWorld.IncidentWorker_Alphabeavers"), "TryExecuteWorker"), new HarmonyMethod(prefix));

			//So as the F**king ShipPartCrash, f**k internal class, f**k this code, f**k everything
			//prefix = typeof(Patch_ShipPartCrash_TryExecuteWorker).GetMethod("PreFix",BindingFlags.Static);
			//instance.Patch(AccessTools.Method(AccessTools.TypeByName("RimWorld.IncidentWorker_ShipPartCrash"), "TryExecuteWorker"), new HarmonyMethod(prefix));

			Compatibility.OtherModPatchMain.ModCompatibilityPatches();
		}

		public static float GetNutrition(Thing foodSource, ThingDef foodDef)
		{
			if (foodSource == null || foodDef == null)
			{
				return 0f;
			}
			if (foodSource.def == foodDef)
			{
				return foodSource.GetStatValue(StatDefOf.Nutrition, true);
			}
			return foodDef.GetStatValueAbstract(StatDefOf.Nutrition, null);
		}
	}
}
