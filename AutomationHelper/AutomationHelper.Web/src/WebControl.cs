using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Drawing;
using AutomationHelper.Base;

namespace AutomationHelper.Web
{
    public class WebControl
	{
		protected By _by;
		protected Driver _driver;
	    protected ILog logger;
		
		public WebControl(Driver driver, By by, ILog logger = null)
		{
			_driver = driver;
			_by = by;
		    this.logger = logger;
		}

		protected IWebElement WebElement => _driver.FindVisibleElement(_by);

		public string Class => GetAttribute("class");
		public string Id => GetAttribute("id");

	    public virtual Color Background => ConvertToColor(GetCssValue("background-color"));
	    public Color Color => ConvertToColor(GetCssValue("color"));
        private Color ConvertToColor(string value)
	    {
	        if (value.Contains("rgba"))
	            return RGBAToColor(value);
	        return RGBToColor(value);
        }
	    private Color RGBAToColor(string value)
	    {
	        var hexValue = value.Replace("rgba(", "").Replace(")", "").Split(',');
	        var r = Int32.Parse(hexValue[0].Trim());
	        var g = Int32.Parse(hexValue[1].Trim());
	        var b = Int32.Parse(hexValue[2].Trim());
	        var a = (int)Math.Round(Double.Parse(hexValue[3].Trim()) * 255);

	        var resultColor = Color.FromArgb(a, r, g, b);
	        return resultColor;
	    }
	    private Color RGBToColor(string value)
	    {
	        var hexValue = value.Replace("rgb(", "").Replace(")", "").Split(',');
	        var r = Int32.Parse(hexValue[0].Trim());
	        var g = Int32.Parse(hexValue[1].Trim());
	        var b = Int32.Parse(hexValue[2].Trim());

	        var resultColor = Color.FromArgb(r, g, b);
	        return resultColor;
	    }

	    

		public FontWeights FontWeight
		{
			get
			{
				var fontWeight = GetCssValue("font-weight");
				if (fontWeight.ToLower() == "bold")
					return FontWeights.Bold;
				if(fontWeight == "700")
					return FontWeights.Bold;
				if (fontWeight.ToLower() == "normal")
					return FontWeights.Normal;
				if (fontWeight == "400")
					return FontWeights.Normal;
				throw new NotImplementedException($"Not recognized FontWeight '{fontWeight}'");
			}
		}

		public string GetAttribute(string attributeName)
		{
			return WebElement.GetAttribute(attributeName);
		}
		public string GetCssValue(string propertyName)
		{
			return WebElement.GetCssValue(propertyName);
		}
		public virtual void Click()
		{
			logger?.Info($"Click on '{_by}'");
			if (_driver.currentBrowser == Browsers.Firefox)
				_driver.ExecuteScript("arguments[0].click();", WebElement);
			else
				WebElement.Click();
		}
		/// <summary>
		/// Click to the link with text inside the control
		/// </summary>
		/// <param name="text"></param>
		public void ClickLinkWithText(string text)
		{
		    logger?.Info($"Click to link with text '{text}'");
			new WebControl(_driver, new ByChained(_by, By.LinkText(text))).Click();
		}
		public void WaitToBeClickable(TimeSpan timeout = default(TimeSpan))
		{
			_driver.WaitToBeClickable(_by, timeout);
		}
		public bool IsVisible(TimeSpan timeout = default(TimeSpan))
		{
			return _driver.GetVisibleElementOrNull(_by, timeout) != null;
		} 
		public bool Displayed => _driver.FindElement(_by).Displayed;

	    public virtual bool IsFocused()
	    {
	        return Class.Contains("k-state-focused");
	    }
        public bool Enabled => WebElement.Enabled;
		public virtual string Text => WebElement.Text.Replace(Environment.NewLine, " ").Trim();
		public string InnerText => GetAttribute("innerText");
		public string Title => GetAttribute("title");
		private Point Location => WebElement.Location;
	    public int Left => Location.X;
	    public int Top => Location.Y;
	    public int Right => Left + Size.Width;
	    public int Bottom => Top + Size.Height;
		public int Width => Size.Width;
		public int Height => Size.Height;
		public string HeightCssValue => GetCssValue("height");

		protected Size Size => WebElement.Size;
		public virtual bool IsSelected => Class.Contains("item-selected");
		public virtual void ScrollInto()
		{
			_driver.ExecuteScript("arguments[0].scrollIntoView();", WebElement);
		}
		public string BackgroundImageUrl => GetCssValue("background-image").ToLower();
		public void Scroll(int offset)
		{
			_driver.ExecuteScript($"arguments[0].scrollTop = arguments[0].scrollTop +{offset};", WebElement);
		}

		public void Focus()
		{
			_driver.ExecuteScript("arguments[0].focus();", WebElement);
		}
		#region Keyboard actions

	    public virtual void PressKey(string key)
	    {
	        logger?.Info($"Press {key} key for control '{_by}'");
            WebElement.SendKeys(key);
	    }
		#endregion

		#region Mouse action
		public void HoverOver()
		{
			_driver.Builder.MoveToElement(WebElement).Build().Perform();
		}
		public void ClickAndHold()
		{
			_driver.Builder.ClickAndHold(WebElement).Build().Perform();
		}
		public void MouseMoveByOffset(int x, int y)
		{
			_driver.Builder.MoveByOffset(x, y).Build().Perform();
		}
		public void MouseRelease()
		{
			_driver.Builder.Release().Build().Perform();
		}
		#endregion
        
	    public virtual double GetScrollTop()
	    {
	        var scrollTop = _driver.ExecuteScript("return arguments[0].scrollTop;", WebElement).ToString();
	        logger?.Info($"Scroll top is: {scrollTop}, for element '{_by}'");
            return Double.Parse(scrollTop);
        }
	}
}

