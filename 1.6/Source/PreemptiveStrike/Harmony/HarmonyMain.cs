using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using PES.RW_JustUtils;
using PreemptiveStrike.Mod;

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

			// Generate a log with info about unpatched PawnsArrivalModeWorker children.
			if (PES_Settings.DebugModeOn)
				LogPatchedMethod("TryResolveRaidSpawnCenter", new[] { typeof(IncidentParms) });
		}

		private static void LogPatchedMethod(string methodName, Type[] parameterTypes)
		{
			var harmonyId = instance?.Id ?? "DrCarlLuo.Rimworld.PreemptiveStrike";
			var baseType = typeof(PawnsArrivalModeWorker);
			var missing = new List<Type>();
			var patched = new List<Type>();

			// Gather all types safely, even from assemblies that may throw ReflectionTypeLoadException.
			static IEnumerable<Type> AllTypes()
			{
				// Go through all assemblies in the game.
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					Type[] types;

					// Try to collect all types (class, struct, interface).
					try { types = asm.GetTypes(); }
					// Exception contains a property Type with array of all types including failed (they are null here).
					catch (ReflectionTypeLoadException ex) { types = ex.Types; }
					if (types == null) continue;
					foreach (var t in types)
						if (t != null) yield return t;  // Stream result one by one instead of building huge array.
				}
			}

			foreach (var t in AllTypes())
			{
				// Skip abstract and if it is not subclass of PawnsArrivalModeWorker.
				if (t.IsAbstract || !t.IsSubclassOf(baseType)) continue;

				// Only consider types that DECLARE an override (not just inherit the base implementation).
				var method = AccessTools.DeclaredMethod(t, methodName,
													   parameterTypes);
				if (method == null) continue;

				var info = HarmonyLib.Harmony.GetPatchInfo(method);
				var hasOurPatch = info?.Owners?.Contains(harmonyId) == true;
				if (!hasOurPatch) missing.Add(t);
				else patched.Add(t);
			}

			if (patched.Count > 0)
			{
				Logger.LogNL($"[Patched PawnsArrivalModeWorker]");
				Logger.LogNL(string.Join("\n", patched.Select(x => x.FullName)));
			}

			if (missing.Count > 0)
			{
				Logger.LogNL($"[Missing PawnsArrivalModeWorker]");
				Logger.LogNL(string.Join("\n", missing.Select(x => x.FullName)));
			}
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
