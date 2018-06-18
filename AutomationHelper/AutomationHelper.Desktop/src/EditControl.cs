using System;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using AutomationHelper.Base;
using AutomationHelper.Desktop;

namespace TCDesktopAutomation.Controls
{
	public class EditControl : ControlBase
	{
		public EditControl(Locator locator):base(locator)
		{
		}

		public void EnterTextNoCheck(string value)
		{
			if (value == null)
				throw new ArgumentNullException(
					"String parameter must not be null.");

			if (Element == null)
				throw new ArgumentNullException(
					"AutomationElement parameter must not be null");
			if (!Element.Current.IsEnabled)
				throw new InvalidOperationException(
					$"The control with an AutomationID of {AutomationId} is not enabled.");

			Element.SetFocus();
			Wait.Sleep(TimeSpan.FromMilliseconds(100));
			KeyboardExtensions.CtrlADel();
		    KeyboardExtensions.EnterText(value);
		}

		public void EnterTextAndCheck(string value)
		{
			EnterTextNoCheck(value);
			CustomAssert.Equal(value, Text,"Entered text was changed");
		}

		public void EnterTextAndCheckStartWith(string value)
		{
			EnterTextNoCheck(value);
			CustomAssert.True(Text.StartsWith(value), $"Field value '{Text}' should start with entered value '{value}'");
		}

		public void EnterDate(DateTime date)
		{
			EnterTextNoCheck(Helpers.FormatDayMonthYear(date));
		}

		public void EnterTextUsingValuePattern(string text)
		{
			object valuePattern = null;

			if (Element.TryGetCurrentPattern(
				ValuePattern.Pattern, out valuePattern))
			{
				Element.SetFocus();
				((ValuePattern)valuePattern).SetValue(text);
			}
		}

		/*public void EnterText(string value)
		{
			try
			{
				if (value == null)
					throw new ArgumentNullException(
						"String parameter must not be null.");

				if (Element == null)
					throw new ArgumentNullException(
						"AutomationElement parameter must not be null");

				if (!Element.Current.IsEnabled)
				{
					throw new InvalidOperationException($"The control with an AutomationID of {AutomationId} is not enabled.");
				}
				if (!Element.Current.IsKeyboardFocusable)
				{
					//log
					//throw new InvalidOperationException(
					//	$"The control with an AutomationID of {AutomationId} is read-only.");
				}
				object valuePattern = null;
				if (!Element.TryGetCurrentPattern(
					ValuePattern.Pattern, out valuePattern))
				{
					//Log($"The control with an AutomationID of {AutomationId} does not support ValuePattern. Using keyboard input.\n");

					Element.SetFocus();
					Thread.Sleep(100);
					SendKeys.SendWait("^{HOME}");   // Move to start of control
					SendKeys.SendWait("^+{END}");   // Select everything
					SendKeys.SendWait("{DEL}");     // Delete selection
					SendKeys.SendWait(value);
				}
				
				else
				{
					//Log($"The control with an AutomationID of {AutomationId} does not support ValuePattern. Using keyboard input.\n");

					Element.SetFocus();
					((ValuePattern)valuePattern).SetValue(value);
				}
			}
			catch (Exception exc)
			{
				//log.
			}
			
		}*/

	}
}
