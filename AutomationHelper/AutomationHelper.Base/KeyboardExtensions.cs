using System;
using System.Windows.Forms;

namespace TCDesktopAutomation.Extensions
{
	public static class KeyboardExtensions
	{
		public static  void HomeEndDel()
		{
			SendKeys.Flush();
			SendKeys.SendWait("^{HOME}"); // Move to start of control
			SendKeys.SendWait("^+{END}"); // Select everything
			SendKeys.SendWait("{DEL}"); // Delete selection
		}
		public static void CtrlADel()
		{
			SendKeys.Flush();
			SendKeys.SendWait("^a"); // Select all text
			Wait.Sleep(TimeSpan.FromMilliseconds(100));
			SendKeys.SendWait("{DEL}"); // Delete selection
		}

		public static void PressEnter()
		{
			SendKeys.SendWait("{ENTER}");
		}

		public static void PressTab()
		{
			SendKeys.SendWait("{TAB}");
		}

		public static void PressBackspace()
		{
			SendKeys.SendWait("{BACKSPACE}");
		}


		public static void EnterText(string text)
		{
			SendKeys.Flush();
			foreach (var letter in text.ToCharArray())
			{
				SendKeys.SendWait(letter.ToString());
				Wait.Sleep(TimeSpan.FromMilliseconds(100));
			}
		}
	}
}
