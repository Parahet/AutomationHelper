using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Accessibility;
using AutomationHelper.Base;


namespace AutomationHelper.Desktop
{
	public class ControlBase
	{
		protected Func<AutomationElement> findFunc;
		protected Locator locator;
		private AutomationElement _element;
		protected ILog logger;
		public ControlBase(Locator locator, ILog logger = null)
		{
			this.locator = locator;
			this.logger = logger ;
		}

		/// <summary>
		/// Use only for next/previous sibling
		/// </summary>
		/// <param name="element"></param>
		public ControlBase(AutomationElement element, ILog logger = null)
		{
			_element = element;
			this.logger = logger;
		}

		protected AutomationElement Element
		{
			get
			{
				if (_element != null)
					return _element;
				return locator.FindElement();
			}
		}

		public bool IsExist()
		{
			if (locator == null)
			{
				logger?.Info("Locator for element is null, check existance by IsOffScreen property");
				return !IsOffscreen;
			}
			return locator.FindElementOrNull() != null;
		}

		public bool IsOffscreen
		{
			get
			{
				try
				{
					return Element.Current.IsOffscreen;
				}
				catch (Exception ex)
				{
					logger?.Info($"Exception was throw while getting IsOffScreen property. {ex.Message}");
					return true;
				}
			}
		}

		public bool IsEnabled => Element.Current.IsEnabled;
		public string ClassName => Element.Current.ClassName;
		public ControlType ControlType => Element.Current.ControlType;
		public string AutomationId => Element.Current.AutomationId;
		public bool IsPassword => Element.Current.IsPassword;

		public Point? TryGetClickablePoint()
		{
			try
			{
				Point p;
				if (Element.TryGetClickablePoint(out p))
					return p;
				return null;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public Rectangle GetBoundingRectangle()
		{
			Rectangle result;
			try
			{
				result = Element.Current.BoundingRectangle;
			}
			catch
			{
				result = new Rectangle();
			}
			return result;
		}

		public void SetFocus()
		{
			try
			{
				Element.SetFocus();
			}
			catch (Exception e)
			{
				logger?.Info("Exception occurred on SetFocus() method: " + e.Message);
			}

		}

		public virtual void Click()
		{
			SetFocus();
			/*if(TryGetClickablePoint() != null)
				Element.ClickOnClickablePoint();
			else */
			Element.ClickCenter();
		}

		public void DoubleClick()
		{
			Element.DoubleClick();
		}

		public virtual void ClickRight()
		{
			SetFocus();
			Element.ClickRight();
		}

		public Bitmap GetScreenShot()
		{
			var rect = GetBoundingRectangle();
			if (rect == new Rectangle(0, 0, 0, 0))
			{
				logger?.Info($"Element 'Name: {Name}, ControlType: {ControlType}' is not visible on the screen");
				return null;
			}

			int width = Convert.ToInt32(rect.Width);
			int height = Convert.ToInt32(rect.Height);

			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				//SetFocus();
				graphics.CopyFromScreen(Convert.ToInt32(rect.X), Convert.ToInt32(rect.Y), System.Drawing.Point.Empty.X,
					System.Drawing.Point.Empty.Y, bitmap.Size);
			}
			return bitmap;
		}

		public Color GetBackgroundColor()
		{
			var bitmap = GetScreenShot();
			if (bitmap == null)
				return Color.FromArgb(255, 255, 255, 255);
			Dictionary<Color, int> colors = new Dictionary<Color, int>();
			for (int i = 0; i < bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height; j++)
				{
					var pixel = bitmap.GetPixel(i, j);
					if (colors.ContainsKey(pixel))
						colors[pixel]++;
					else
						colors.Add(pixel, 0);
				}
			}
			return colors.FindMax(c => c.Value).First().Key;
		}

		public string Name => Element.Current.Name ?? String.Empty;

		public string Value
		{
			get
			{
				object patternObj;
				if (Element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
				{
					var valuePattern = (ValuePattern) patternObj;
					return valuePattern.Current.Value;
				}
				if (Element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
				{
					var textPattern = (TextPattern) patternObj;
					return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
				}
				if (Element.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out patternObj))
				{
					var legacyPattern = (LegacyIAccessiblePattern) patternObj;
					return legacyPattern.Current.Value;
				}
				return string.Empty;
			}
		}

		public virtual string Text
		{
			get
			{
				var result = Value;
				if (String.IsNullOrEmpty(result) || result.StartsWith("about:blank") || result.StartsWith("javascript:"))
					result = Name;
				return result.Trim();
			}
		}

		public ControlBase NextSibling()
		{
			return new ControlBase(Element.FindNextSibling());
		}

		public ControlBase PreviosSibling()
		{
			return new ControlBase(Element.FindPreviosSibling());
		}

		public void SelectRowFromDropDownTable(string startWithText = "")
		{
			logger?.Info($"Selecting Row that start with '{startWithText}' from drop downm table, for element {Name}");
			Element.SelectRowFromDropDownTable(startWithText);
		}

		public void SelectCellFromDropDownTable(string cellHeader, string cellText)
		{
			logger?.Info($"Selecting '{cellHeader}' = '{cellText}' cell from drop downm table, for element {Name}");
			Element.SelectCellFromDropDownTable(cellHeader, cellText);
		}

		public void SelectItemFromDropDownList(string text)
		{
			logger?.Info($"Selecting item '{text}' from drop downm List, for element {Name}");
			Element.SelectItemFromDropDownList(text);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeout">Default is 10 seconds</param>
		public virtual void WaitForExist(TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(10) : timeout; 
			Wait.UntilTrue(IsExist, $"{GetType().Name} control didn't appear", timeout);
		}


		#region MSAA

		[DllImport("oleacc.dll")]
		public static extern int AccessibleChildren(IAccessible paccContainer,
			int iChildStart, int cChildren,
			[Out()] [MarshalAs(UnmanagedType.LPArray,
				SizeParamIndex = 4)] object[] rgvarChildren,
			ref int pcObtained);
		

		public List<string> GetChildrensNameMSAA()
		{
			var result = new List<string>();
			if ((bool)Element.GetCurrentPropertyValue(AutomationElementIdentifiers.IsLegacyIAccessiblePatternAvailableProperty))
			{
				var pattern = ((LegacyIAccessiblePattern)Element.GetCurrentPattern(LegacyIAccessiblePattern.Pattern));
				var obj = pattern.GetIAccessible();
				for (int i = 1; i <= obj.accChildCount; i++)
					result.Add((obj.accChild[i] as IAccessible).accName);
			}
			return result;
		}
		public List<string> GetChildrenValueMSAA()
		{
			var result = new List<string>();
			if ((bool)Element.GetCurrentPropertyValue(AutomationElementIdentifiers.IsLegacyIAccessiblePatternAvailableProperty))
			{
				var pattern = ((LegacyIAccessiblePattern)Element.GetCurrentPattern(LegacyIAccessiblePattern.Pattern));
				var obj = pattern.GetIAccessible();
				for (int i = 1; i <= obj.accChildCount; i++)
					result.Add((obj.accChild[i] as IAccessible).accValue);
			}
			return result;
		}
		#endregion
	}
}
