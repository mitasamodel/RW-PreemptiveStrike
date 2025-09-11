using PreemptiveStrike.Mod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

//method = MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name;

namespace PES.RW_JustUtils
{
	public static class Logger
	{
		private static bool _init = false;
		static readonly string logFile = @Environment.CurrentDirectory + @"\Mods\PES.log";

		[ThreadStatic]
		private static int _tabLevel = 0;

		public static void Init()
		{
			if (!_init)
			{
				_init = true;
				File.WriteAllText(logFile, "[PES] Debug start\n");
			}
		}

		public static void LogNL(string msg)
		{
			if (!_init) Init();
			File.AppendAllText(logFile, GetTabs() + msg + "\n");
		}

		// Log from the beginning no matter tabs.
		public static void LogNL(int tab, string msg)
		{
			if (!_init) Init();
			File.AppendAllText(logFile, msg + "\n");
		}

		public static void Log(string msg)
		{
			if (!_init) Init();
			File.AppendAllText(logFile, msg);
		}

		public static void Log_Warning(string str)
		{
			Verse.Log.Warning($"[Preemptive Strike] " + str);
			if (PES_Settings.DebugModeOn)
				LogNL(str);
		}

		public static void Log_Error(string str)
		{
			Verse.Log.Error($"[Preemptive Strike] " + str);
			if (PES_Settings.DebugModeOn)
				LogNL(str);
		}

		private static string GetTabs()
		{
			if (_tabLevel <= 0) return string.Empty;
			return new string('\t', _tabLevel);
		}

		public static void IncreaseTab() => _tabLevel++;

		public static void DecreaseTab() { if (_tabLevel > 0) _tabLevel--; }

		public static void ResetTab() => _tabLevel = 0;

		/// <summary>
		/// Increase the tab at the call.
		/// Automatically decrease the tab at local variable's termination (end of block/method).
		/// </summary>
		/// <returns></returns>
		public static IDisposable Scope()
		{
			if (PES_Settings.DebugModeOn)
			{
				IncreaseTab();
				return new IndentPopper();
			}
			else
				return default;
		}

		/// <summary>
		/// Simple object, which implements IDisposable.
		/// </summary>
		private readonly struct IndentPopper : IDisposable
		{
			public void Dispose()
			{
				DecreaseTab();
			}
		}
	}
}
