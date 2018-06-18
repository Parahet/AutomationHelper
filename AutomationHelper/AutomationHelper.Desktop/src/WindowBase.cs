using System;
using AutomationHelper.Base;

namespace AutomationHelper.Desktop
{
	public abstract class WindowBase : ControlBase
	{
		protected WindowBase(Locator locator, bool waitForExist, ILog logger = null) : base(locator, logger)
		{
		    if (waitForExist)
		    {
		        SetFocus();
                WaitForExist();
			    logger?.Info($"Window '{GetType().Name}' is exist");
		    }
		}

		public override void WaitForExist(TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(10) : timeout;
			Wait.UntilTrue(IsExist, $"{GetType().Name} window didn't appear", timeout);
		}

		public void WaitForClose(TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(10) : timeout;
			Wait.UntilTrue(() => !IsExist(), $"{GetType().Name} window didn't close",
				timeout);
		}
	}
}
