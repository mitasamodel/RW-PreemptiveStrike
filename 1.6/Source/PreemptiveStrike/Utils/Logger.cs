using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PES.RW_JustUtils
{
	public static class Logger
	{
		private static bool _init = false;
		static readonly string logFile = @Environment.CurrentDirectory + @"\Mods\PES.log";

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
			File.AppendAllText(logFile, msg + "\n");
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
	}
}
