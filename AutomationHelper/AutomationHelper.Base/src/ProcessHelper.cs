using System;
using System.Diagnostics;
using System.Linq;

namespace AutomationHelper.Base
{
	public class ProcessHelper
	{
		public static void KillProcess(string processName, TimeSpan waitTimeout = default(TimeSpan), ILog logger = null)
		{
			logger?.Info($"Killing existing processes: '{processName}'");

			var processes = Process.GetProcessesByName(processName);
			logger?.Info($"Found {processes.Count()} processes by name: {processName}");
			foreach (var proc in processes)
			{
				if (!proc.HasExited)
				{
					proc.Kill();
					proc.WaitForExit();
				}
			}
			WaitTillProcessNotExist(processName, waitTimeout, logger);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processName"></param>
		/// <param name="timeout">The default Timeout is 10 seconds</param>
		/// <param name="logger"></param>
		public static void WaitTillProcessNotExist(string processName, TimeSpan timeout = default(TimeSpan), ILog logger = null)
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(10) : timeout;
			logger?.Info($"Wait ({timeout.TotalSeconds} seconds) till process '{processName}' not exist");
			Wait.UntilTrue(() => !IsProcessExist(processName), $"{processName} process is still exist",
				timeout);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processName"></param>
		/// <param name="timeout">The default Timeout is 10 seconds</param>
		public static void WaitTillProcessAppear(string processName, TimeSpan timeout = default(TimeSpan), ILog logger = null)
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(10) : timeout;
			logger?.Info($"Wait ({timeout.TotalSeconds} seconds) while process '{processName}' appear");
			Wait.UntilTrue(() => IsProcessExist(processName), $"{processName} process is still not exist",
				timeout);
		}
		public static bool IsProcessExist(string processName)
		{
			return Process.GetProcessesByName(processName).Length > 0;
		}
	}
}
