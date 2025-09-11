using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private static int _tabLevel = 0;

		public static void Init()
		{
#if DEBUG
			if (!_init)
			{
				_init = true;
				File.WriteAllText(logFile, "[PES] Debug start\n");    //force in debug
			}
#endif
		}

		public static void LogNL(string msg)
		{
#if DEBUG
			if (!_init) Init();
			File.AppendAllText(logFile, GetTabs() + msg + "\n");
#endif
		}

		public static void Log(string msg)
		{
#if DEBUG
			if (!_init) Init();
			File.AppendAllText(logFile, msg);
#endif
		}

		public static void Log_Warning(string str)
		{
			Verse.Log.Warning($"[Preemptive Strike] " + str);
#if DEBUG
			LogNL(str);
#endif
		}

		public static void Log_Error(string str)
		{
			Verse.Log.Error($"[Preemptive Strike] " + str);
#if DEBUG
			LogNL(str);
#endif
		}

		private static string GetTabs()
		{
			string str = "";
			for (int i = 0; i < _tabLevel; i++)
				str += "\t";
			return str;
		}

		public static void IncreaseTab()
		{
			_tabLevel++;
		}

		public static void DecreaseTab()
		{
			_tabLevel--;
		}

		public static void ResetTab()
		{
			_tabLevel = 0;
		}
	}
}
