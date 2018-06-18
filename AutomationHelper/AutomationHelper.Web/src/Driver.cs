using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Keys = OpenQA.Selenium.Keys;
using AutomationHelper.Base;
using OpenQA.Selenium.Support.Extensions;
using SeleniumExtras.PageObjects;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;


namespace AutomationHelper.Web
{
	public class Driver 
	{
		private IWebDriver webDriver;
	    private TimeSpan implicitWait;
	    private TimeSpan scriptTimeout;
	    private TimeSpan pageLoadTimeout;
	    private bool isMaximized;
	    private Browsers browser;
	    private ILog logger;

        public Driver(Browsers browser = Browsers.Chrome, TimeSpan implicitWait = default(TimeSpan), TimeSpan scriptTimeout= default(TimeSpan), TimeSpan pageLoadTimeout= default(TimeSpan), bool? isMaximized=null, ILog logger = null)
		{
            if(implicitWait == default(TimeSpan))
                this.implicitWait = TimeSpan.FromSeconds(15);
		    if (scriptTimeout == default(TimeSpan))
		        this.scriptTimeout = TimeSpan.FromSeconds(15);
		    if (pageLoadTimeout == default(TimeSpan))
		        this.pageLoadTimeout = TimeSpan.FromSeconds(15);
		    if (isMaximized == null)
		        this.isMaximized = true;
		    this.logger = logger;
            InitDriver();
		}

	    public string ChromeInstanceUserProfilePath { get; set; }
        public string FirefoxProfileName { get; set; } = null;


	    private void InitDriver()
		{
		    switch (browser)
		    {
		        case Browsers.Chrome:
		            InitializeChrome();
		            break;
		        case Browsers.Firefox:
		            InitializeFirefox();
		            break;
		        case Browsers.IE:
		            InitializeIE();
		            break;
		        case Browsers.Edge:
		            InitializeEdge();
		            break;
		    }

			webDriver.Manage().Timeouts().ImplicitWait = implicitWait;
            webDriver.Manage().Timeouts().AsynchronousJavaScript = scriptTimeout;
			webDriver.Manage().Timeouts().PageLoad = pageLoadTimeout;
			if (isMaximized)
				if (browser != Browsers.Firefox)
					webDriver.Manage().Window.Maximize();
				else
				{
					try
					{
						var screenWidth = Screen.PrimaryScreen.Bounds.Width;
						var screenHeight = Screen.PrimaryScreen.Bounds.Height;
						webDriver.Manage().Window.Size = new Size(screenWidth, screenHeight);
					}
					catch(Exception ex)
					{
						logger?.Info("Can't change firefox browser resolution. "+ex.Message);
					}
				}
		}
        
	   #region Initialize Browsers

		private string chromeInstanceUserProfilePath;
		private string chromeUserProfilePath => $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Google\Chrome\User Data TruCode";
		private void InitializeChrome()
		{
			var options = new ChromeOptions();
			var proxy = new Proxy();
			proxy.Kind = ProxyKind.Manual;
			proxy.IsAutoDetect = false;
			proxy.HttpProxy = proxy.SslProxy = "127.0.0.1";
			options.Proxy = proxy;

			options.Proxy = proxy;
			options.AddArgument("no-sandbox");
			options.AddArgument("--ignore-certificate-errors");
            options.AddUserProfilePreference("safebrowsing.enabled", true);
            options.AddArgument("--disable-popup-blocking");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-plugins");
			options.AddArgument("--no-default-browser-check");
			options.AddArgument("--clear-token-service");
			options.AddArgument("--disable-default-apps");
			options.AddArgument("--no-displaying-insecure-content");
			options.AddArgument("--disable-bundled-ppapi-flash");
			options.AddArgument("test-type");
			options.AddArgument("--disable-infobars");
			options.AddArgument("--disable-session-crashed-bubble"); 
			options.AddArgument("test-type");

		    if (ChromeInstanceUserProfilePath != null)
		    {
		        options.AddArguments($"user-data-dir={ChromeInstanceUserProfilePath}");
                logger?.Info($"Setting chrome user data dir to '{ChromeInstanceUserProfilePath}'");
		    }
		    options.Proxy = null;
			webDriver = new ChromeDriver(options);
            WaitForLoad();
            logger?.Pass($"Chrome browser initialize completed.");
		}
		private void InitializeFirefox()
		{
		    FirefoxProfile profile = null;
            if (FirefoxProfileName != null)
		    {
		        var profileManager = new FirefoxProfileManager();
		        if (profileManager.ExistingProfiles.Any(p => p == FirefoxProfileName))
		        {
		            logger?.Info($"Initialize firefox with existing profile '{FirefoxProfileName}'");
		            profile = profileManager.GetProfile(FirefoxProfileName);
		        }
		    }
		    if (profile == null)
		    {
		        logger?.Info($"Initialize firefox with default profile");
		        profile = new FirefoxProfile();
		    }
		    var firefoxOptions = new FirefoxOptions();
			firefoxOptions.Profile = profile;
			webDriver = new FirefoxDriver(firefoxOptions);
		    logger?.Pass("Firefox browser initialize completed");
		}
		private void InitializeIE()
		{
			var options = new InternetExplorerOptions();
			options.BrowserCommandLineArguments = "--private";
			options.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
			options.UnhandledPromptBehavior = UnhandledPromptBehavior.Accept;
			webDriver = new InternetExplorerDriver(options);
		    logger?.Pass("IE browser initialize completed");
		}
		private void InitializeEdge(ILog logger = null)
		{
			webDriver = new EdgeDriver();
		    logger?.Pass("Edge browser initialize completed");
		}
		#endregion


		#region Find methods
		public IWebElement FindVisibleElement(By @by)
		{
			var element = GetVisibleElementOrNull(by, implicitWait);
			if(element == null)
				throw new Exception($"Can't find visible element '{by}'");
			return element;
		}
		public IWebElement FindElement(By @by)
		{
			var element = GetExistingElementOrNull(by, implicitWait);
			if (element == null)
				throw new Exception($"Can't find element '{by}'");
			return element;
		}
		public ReadOnlyCollection<IWebElement> FindElements(By @by)
		{
			return webDriver.FindElements(by);
		}
		public ReadOnlyCollection<IWebElement> FindElementsWithTimeout(By @by, TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.Zero : timeout;
            webDriver.Manage().Timeouts().ImplicitWait = timeout;
			try
			{
				return webDriver.FindElements(by);
			}
			finally
			{
				webDriver.Manage().Timeouts().ImplicitWait = implicitWait;
			}
		}
		public List<By> FindVisibleElementsLocators(By parentBy, string xpathSelector)
		{
			var result = new List<By>();
			var i = 1;
			var by =  new ByChained(parentBy, By.XPath($"{xpathSelector}[{i}]"));
			var element = GetExistingElementOrNull(by);

			while (element != null)
			{
				if(element.Displayed)
					result.Add(by);
				i++;
				by = new ByChained(parentBy, By.XPath($"{xpathSelector}[{i}]"));
				element = GetExistingElementOrNull(by);
			}
			return result;
		}
		public List<By> FindVisibleElementsLocators(string xpathSelector)
		{
			var result = new List<By>();
			var i = 1;
			var by =By.XPath($"{xpathSelector}[{i}]");
			var element = GetExistingElementOrNull(by);

			while (element != null)
			{
				if (element.Displayed)
					result.Add(by);
				i++;
				by =By.XPath($"{xpathSelector}[{i}]");
				element = GetExistingElementOrNull(by);
			}
			return result;
		}

		/// <summary>
		/// Getting the element if it present and visible on the DOM of a page, otherway return null
		/// </summary>
		/// <param name="by">WebElement locator</param>
		/// <returns></returns>
		public IWebElement GetVisibleElementOrNull(By @by, TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.Zero : timeout;
		    var wait = new WebDriverWait(webDriver, timeout);
		    webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
			try
			{
				var result = wait.Until<IWebElement>((webDriver) =>
				{
					try
					{
						return webDriver.FindElements(by).First(e => e.Displayed, "No one elements displayed");
					}
					catch
					{
					}
					return null;
				});
				return result;
			}
			catch (Exception ex)
			{
				logger?.Info($"Element '{by}' is not visible on a page. {ex.Message}");
				return null;
			}
			finally
			{
				webDriver.Manage().Timeouts().ImplicitWait = implicitWait;
			}
		}

		public IWebElement GetExistingElementOrNull(By @by, TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.Zero : timeout;
            var wait = new WebDriverWait(webDriver, timeout);
			webDriver.Manage().Timeouts().ImplicitWait = timeout;
			try
			{
				var result = wait.Until(ExpectedConditions.ElementExists(by));
				return result;
			}
			catch (Exception ex)
			{
				logger?.Info($"Element '{by}' is not present on the DOM of a page. {ex.Message}");
				return null;
			}
			finally
			{
				webDriver.Manage().Timeouts().ImplicitWait = implicitWait;
			}
		}
		#endregion

		public void WaitToBeClickable(By by, TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(5) : timeout;
            var wait = new WebDriverWait(webDriver, timeout);
			webDriver.Manage().Timeouts().ImplicitWait = timeout;
			try
			{
				wait.Until(ExpectedConditions.ElementToBeClickable(by));
			}
			catch (Exception ex)
			{
				logger?.Info($"Element '{by}' is not present on the DOM of a page. {ex.Message}");
			}
			finally
			{
				webDriver.Manage().Timeouts().ImplicitWait = implicitWait;
			}
		}
		#region Implementation of IWebDriver
		public void Dispose()
		{
			webDriver.Dispose();
		}

		public void Close()
		{
			webDriver.Close();
		}

		public void Quit()
		{
			webDriver.Quit();
		}

		public IOptions Manage()
		{
			return webDriver.Manage();
		}

		public INavigation Navigate()
		{
			return webDriver.Navigate();
		}

		public ITargetLocator SwitchTo()
		{
			return webDriver.SwitchTo();
		}

		public string Url {
			get => webDriver.Url;
			set => webDriver.Url = value;
		}
		public string Title => webDriver.Title;
		public string PageSource => webDriver.PageSource;
		public string CurrentWindowHandle => webDriver.CurrentWindowHandle;
		public ReadOnlyCollection<string> WindowHandles => webDriver.WindowHandles;
		#endregion

	    private void GoToUrl(string url)
	    {
	        webDriver.Navigate().GoToUrl(url);
            WaitForLoad();
		    logger?.Info($"Navigate to Url '{url}' successfully completed");
	    }
		
		public void WaitForLoad()
		{
			Wait.UntilTrue(() => bool.Parse(ExecuteScript("return document.readyState === 'complete'").ToString()),
				"Page is not loaded", webDriver.Manage().Timeouts().PageLoad);
			/*new WebDriverWait(webDriver, BrowserSettings.Default.PageLoadTimeout).Until(
				d => (IJavaScriptExecutor)d).ExecuteScript("return document.readyState === 'complete'");*/
		}
        
		public object ExecuteScript(string script, params object[] args)
		{
			return ((IJavaScriptExecutor) webDriver).ExecuteScript(script,args);
		}
		public void ExecuteAsyncScript(string script, params object[] args)
		{
			((IJavaScriptExecutor)webDriver).ExecuteAsyncScript(script, args);
		}
        
		public void DeleteAllCookies()
		{
			webDriver.Manage().Cookies.DeleteAllCookies();
		}
		public Screenshot TakeScreenshot()
	    {
	        return webDriver.TakeScreenshot();
	    }

		#region Keyboard
		public void PressKey(string key)
		{
			logger?.Info($"Pressing '{key}' key");
			Builder.SendKeys(key).Build().Perform();
		}

		public void PressCtrlAndKey(string key)
		{
			Builder.KeyDown(Keys.Control).SendKeys(key).KeyUp(Keys.Control).Build().Perform();
		}
		public void PressKeyUntil(string key, Func<bool> exitCondition, int maxIterations = 50)
		{
			for (int i = 0; i < maxIterations; i++)
			{
				if (exitCondition())
					return;
				PressKey(key);
			}
			throw new Exception($"ExitCondition have not been reached (Key {key} have been pressed for {maxIterations} times)");
		}

		#endregion

		public Actions Builder => new Actions(webDriver);
	}
}
