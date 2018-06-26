using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using AutomationHelper.Base;

namespace AutomationHelper.Desktop
{
	public static class AutomationElementExtensions
	{
		private static TimeSpan defaultTimeout= TimeSpan.FromSeconds(10);

		public static bool IsNotNull(this Rectangle rect)
		{
			if (rect.Right != 0 && rect.Bottom != 0)
				return true;
			return false;
		}

		public static void ClickCenter(this AutomationElement element)
		{
			var rect = element.Current.BoundingRectangle;
			MouseLeftClick(new Point((int) rect.X + (int) (rect.Width / 2),
				(int) rect.Y + (int) (rect.Height / 2)));
		}

		public static void ClickRight(this AutomationElement element)
		{
			var rect = element.Current.BoundingRectangle;
			MouseLeftClick(new Point((int) rect.X + (int) (rect.Width) - 5,
				(int) rect.Y + (int) (rect.Height / 2)));
		}

		public static void ClickOnPoint(Point point)
		{
			Point p = new Point(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
			MouseLeftClick(p);
		}

		public static void ClickOnClickablePoint(this AutomationElement element)
		{
			Point point;
			element.TryGetClickablePoint(out point);
			Point p = new Point(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
			MouseLeftClick(p);
		}

		public static void DoubleClick(this AutomationElement element)
		{
			var rect = element.Current.BoundingRectangle;
			MouseDoubleClick(new Point((int) rect.X + (int) (rect.Width / 2),
				(int) rect.Y + (int) (rect.Height / 2)));
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const int MOUSEEVENTF_RIGHTUP = 0x10;
		private const int MOUSE_HOVER = 0x02A1;

		private static void MouseLeftClick(System.Drawing.Point point)
		{
			Cursor.Position = point;
			mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
		}

		private static void MouseDoubleClick(System.Drawing.Point point)
		{
			Cursor.Position = point;
			mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
			mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
		}

		private static void MouseRightClick(System.Drawing.Point point)
		{
			Cursor.Position = point;
			mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
		}

		#region Find Methods

		/*
		/// <summary>
		/// Walks the UI Automation tree and adds the control type of each element it finds 
		/// in the control view to a TreeView.
		/// </summary>
		/// <param name="rootElement">The root of the search on this iteration.</param>
		/// <param name="treeNode">The node in the TreeView for this iteration.</param>
		/// <remarks>
		/// This is a recursive function that maps out the structure of the subtree beginning at the
		/// UI Automation element passed in as rootElement on the first call. This could be, for example,
		/// an application window.
		/// CAUTION: Do not pass in AutomationElement.RootElement. Attempting to map out the entire subtree of
		/// the desktop could take a very long time and even lead to a stack overflow.
		/// </remarks>
		private static void WalkControlElements(AutomationElement rootElement, TreeNode treeNode)
		{
			// Conditions for the basic views of the subtree (content, control, and raw) 
			// are available as fields of TreeWalker, and one of these is used in the 
			// following code.
			AutomationElement elementNode = TreeWalker.ControlViewWalker.GetFirstChild(rootElement);

			while (elementNode != null)
			{
				TreeNode childTreeNode = treeNode.Nodes.Add(elementNode.Current.ControlType.LocalizedControlType);
				WalkControlElements(elementNode, childTreeNode);
				elementNode = TreeWalker.ControlViewWalker.GetNextSibling(elementNode);
			}
		}
		public static List<AutomationElement> GetAllChildren(this AutomationElement element)
		{
			var allChildren = new List<AutomationElement>();
			AutomationElement sibling = TreeWalker.RawViewWalker.GetFirstChild(element);

			while (sibling != null)
			{
				allChildren.Add(sibling);
				sibling = TreeWalker.RawViewWalker.GetNextSibling(sibling);
			}
			return allChildren;
		}
		public static List<AutomationElement> GetAllDescendants(this AutomationElement element, int depth = 0, int maxDepth = 3)
		{
			var allChildren = new List<AutomationElement>();

			if (depth > maxDepth)
			{
				return allChildren;
			}

			AutomationElement sibling = TreeWalker.RawViewWalker.GetFirstChild(element);

			while (sibling != null)
			{
				allChildren.Add(sibling);
				allChildren.AddRange(sibling.GetAllDescendants(depth + 1, maxDepth));
				sibling = TreeWalker.RawViewWalker.GetNextSibling(sibling);
			}
			return allChildren;
		}
		*/


		public static AutomationElement FindParentOrNull(this AutomationElement element, Locator locator = null, ILog logger = null)
		{
			var maxNesting = 20;
			TreeWalker walker = TreeWalker.ControlViewWalker;
			AutomationElement node = walker.GetParent(element);
			if (locator == null) return node;
			for (int i = 0; i < maxNesting; i++)
			{
				if (node.IsMatch(locator.Properties) || node == null)
					return node;
				node = walker.GetParent(node);
			}
			logger?.Info(
				$"Can't find matching parent element '{locator.ToString()}' for element '{new ControlBase(element).Name}'");
			return null;
		}

		[Obsolete("Use FindChildOrNull().This one use TreeWalker for finding")]
		public static AutomationElement FindChildOrNull2(this AutomationElement element, Locator locator = null,
			ILog logger = null)
		{
			var maxChildrenForCheck = 100;
			TreeWalker walker = TreeWalker.ControlViewWalker;
			AutomationElement node = walker.GetFirstChild(element);
			if (locator == null) return node;
			for (int i = 0; i < maxChildrenForCheck; i++)
			{
				if (node.IsMatch(locator.Properties) || node == null)
					return node;
				node = walker.GetNextSibling(node);
			}
			logger?.Info($"Can't find child element '{locator.ToString()}' for element '{new ControlBase(element).Name}'");
			return null;
		}

		public static AutomationElement FindChildOrNull(this AutomationElement element, Locator locator = null,
			ILog logger = null)
		{
			try
			{
				var condition = locator == null ? Condition.TrueCondition : locator.Condition;
				return element.FindFirst(TreeScope.Children, condition);
			}
			catch (Exception ex)
			{
				logger?.Info("Exception was thrown while finding first child. " + ex.Message);
				Thread.Sleep(500);
				Thread.Yield();
				return null;
			}
		}

		public static AutomationElement FindDescendantOrNull(this AutomationElement element, Locator locator = null, ILog logger = null)
		{
			try
			{
				var condition = locator == null ? Condition.TrueCondition : locator.Condition;
				return element.FindFirst(TreeScope.Descendants, condition);
			}
			catch (Exception ex)
			{
				logger?.Info("Exception was thrown while finding first Descendant. " + ex.Message);
				Thread.Sleep(500);
				Thread.Yield();
				return null;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="locator"></param>
		/// <param name="timeout">Default is 10 seconds</param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public static AutomationElement FindElement(this AutomationElement element, Locator locator,
			TimeSpan timeout = default(TimeSpan), ILog logger = null)
		{
			timeout = timeout == default(TimeSpan) ? defaultTimeout : timeout;
			return Wait.UntilNoException(() =>
			{
				AutomationElement found = element.FindElementOrNull(locator, logger);
				if (found == null)
					throw new Exception($"Can't find element '{locator}' for '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		public static AutomationElement FindElementOrNull(this AutomationElement element, Locator locator, ILog logger = null)
		{
			AutomationElement found = null;
			if (locator.Where == Locator.WhereFind.Children)
				found = element.FindChildOrNull(locator, logger);
			else if (locator.Where == Locator.WhereFind.Descendants)
				found = element.FindDescendantOrNull(locator, logger);
			else if (locator.Where == Locator.WhereFind.Parent)
				found = element.FindParentOrNull(locator, logger);
			return found;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="locator"></param>
		/// <param name="timeout">Default is 10 seconds</param>
		/// <returns></returns>
		public static List<AutomationElement> FindElements(this AutomationElement element, Locator locator,
			TimeSpan timeout = default(TimeSpan), ILog logger = null)
		{
			timeout = timeout == default(TimeSpan) ? defaultTimeout : timeout;
			return Wait.UntilNoException(() =>
			{
				List<AutomationElement> found = null;
				if (locator.Where == Locator.WhereFind.Children)
					found = element.FindAllChilds(locator, logger);
				else if (locator.Where == Locator.WhereFind.Descendants)
					found = element.FindAllDescendants(locator, logger);
				else if (locator.Where == Locator.WhereFind.Parent)
					throw new Exception("Find elements is not implemented for parent");
				if (!found.Any())
					logger?.Info($"Can't find any element '{locator}' for '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		public static List<AutomationElement> FindAllChilds(this AutomationElement element, Locator locator = null,
			ILog logger = null)
		{
			locator = locator ?? new Locator(Locator.WhereFind.Children);
			List<AutomationElement> result = new List<AutomationElement>();
			var found = element.FindAll(TreeScope.Children, locator.Condition);
			foreach (AutomationElement el in found)
				result.Add(el);
			logger?.Info($"Found {result.Count} children elements");
			return result;
		}

		public static List<AutomationElement> FindAllDescendants(this AutomationElement element, Locator locator = null, ILog logger = null)
		{
			locator = locator ?? new Locator(Locator.WhereFind.Descendants);
			List<AutomationElement> result = new List<AutomationElement>();
			var found = element.FindAll(TreeScope.Descendants, locator.Condition);
			foreach (AutomationElement el in found)
				result.Add(el);
			logger?.Info($"Found {result.Count} descendants elements");
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="timeout">Default is 10 seconds</param>
		/// <returns></returns>
		public static AutomationElement FindPreviosSibling(this AutomationElement element, TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? defaultTimeout : timeout; 
			return Wait.UntilNoException(() =>
			{
				var found = TreeWalker.RawViewWalker.GetPreviousSibling(element);
				if (found == null)
					throw new Exception($"Can't find previous sibling for element '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		public static AutomationElement FindNextSibling(this AutomationElement element, TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? defaultTimeout : timeout;
			return Wait.UntilNoException(() =>
			{
				var found = element.FindNextSiblingOrNull();
				if (found == null)
					throw new Exception($"Can't find next sibling for element '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		public static AutomationElement FindNextSiblingOrNull(this AutomationElement element)
		{
			return TreeWalker.RawViewWalker.GetNextSibling(element);
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="locator"></param>
		/// <param name="timeout">Default is 10 seconds</param>
		/// <returns></returns>
		public static AutomationElement FindChild(this AutomationElement element, Locator locator,
			TimeSpan timeout = default(TimeSpan),ILog logger = null)
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(10) : timeout;
			return Wait.UntilNoException(() =>
			{
				var found = element.FindChildOrNull(locator, logger);
				if (found == null)
					throw new Exception($"Can't find child element '{locator}' for '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="locator"></param>
		/// <param name="timeout">Default is 20 seconds</param>
		/// <returns></returns>
		public static AutomationElement FindParent(this AutomationElement element, Locator locator,
			TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(20) : timeout;
			return Wait.UntilNoException(() =>
			{
				var found = element.FindParentOrNull(locator);
				if (found == null)
					throw new Exception($"Can't find parent '{locator}' for element '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="locator"></param>
		/// <param name="timeout">Default is 20 seconds</param>
		/// <returns></returns>
		public static AutomationElement FindDescendant(this AutomationElement element, Locator locator,
			TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(20) : timeout;
			return Wait.UntilNoException(() =>
			{
				var found = element.FindDescendantOrNull(locator);
				if (found == null)
					throw new Exception($"Can't find descendants element '{locator}' for '{new ControlBase(element).Name}'");
				return found;
			}, timeout);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="locator"></param>
		/// <param name="timeout">Default is 20 seconds</param>
		/// <returns></returns>
		public static AutomationElement LastDescendant(this AutomationElement element, Locator locator,
			TimeSpan timeout = default(TimeSpan))
		{
			timeout = timeout == default(TimeSpan) ? TimeSpan.FromSeconds(20) : timeout;
			return Wait.UntilNoException(() =>
			{
				var all = element.FindAll(TreeScope.Descendants, locator.Condition);
				if (all.Count == 0)
					throw new Exception($"Can't find any descendants '{locator}' for '{new ControlBase(element).Name}'");
				return all[all.Count - 1];
			}, timeout);
		}
		#endregion

		public static bool IsMatch(this AutomationElement element, List<LocatorProperty> properties)
		{
			var result = false;
			foreach (var property in properties)
			{
				result = result || element.isMatch(property);
			}
			return result;
		}
		private static bool isMatch(this AutomationElement element, LocatorProperty property)
		{
			bool result = true;
			if (property.Name != null)
				result = element.Current.Name == property.Name;
			if (result && property.AutomationId != null)
				result = element.Current.AutomationId == property.AutomationId;
			if (result && property.ClassName != null)
				result = element.Current.ClassName == property.ClassName;
			if (result && property.ControlType != default(ControlType))
				result = Equals(element.Current.ControlType, property.ControlType);
			return result;
		}

		private static void DropDown(this AutomationElement element)
		{
			element.Clear();
			Wait.Sleep(TimeSpan.FromMilliseconds(100));
			KeyboardExtensions.PressBackspace();
			Wait.Sleep(TimeSpan.FromMilliseconds(500));
		}

		public static void Clear(this AutomationElement element)
		{
			element.SetFocus();
			element.ClickCenter();
			Wait.Sleep(TimeSpan.FromMilliseconds(100));
			KeyboardExtensions.CtrlADel();
		}

		public static void SelectRowFromDropDownTable(this AutomationElement element, string startWithText = "")
		{
			var parentWindow = element.FindParent(new Locator(Locator.WhereFind.Parent,controlType: ControlType.Window));
			element.DropDown();
			var window = parentWindow.FindChild(new Locator(Locator.WhereFind.Children,controlType: ControlType.Window));

			var allRows = window.FindDescendant(Locators.TableDataPanelLocator)
				.FindAllChilds().Select(r => new ControlBase(r)).ToList();

			if (String.IsNullOrEmpty(startWithText))
				allRows.First("Can't find any rows in the drop down table").Click();
			else
				allRows.First(r => r.Value.StartsWith(startWithText),
					$"Can't find row which start with '{startWithText}'").Click();
			Wait.Sleep(TimeSpan.FromMilliseconds(500));
		}

		public static void SelectCellFromDropDownTable(this AutomationElement element, string cellHeader, string cellText)
		{
			var parentWindow = element.FindParent(new Locator(Locator.WhereFind.Parent,controlType: ControlType.Window));
			element.DropDown();
			var window = parentWindow.FindChild(new Locator(Locator.WhereFind.Children,controlType: ControlType.Window));


			var allCells = window.FindDescendant(Locators.TableDataPanelLocator)
				.FindAllDescendants(new Locator(Locator.WhereFind.Descendants, cellHeader)).Select(d => new ControlBase(d)).ToList();

			if (!allCells.Any())
				throw new Exception($"Can't find any cell with header {cellHeader} in the table");
			var foundCell = allCells.First(r => r.Value == cellText, $"Can't find cell with text {cellText}");
			foundCell.Click();
			Wait.Sleep(TimeSpan.FromMilliseconds(500));
		}

		public static void SelectItemFromDropDownList(this AutomationElement element, string text)
		{
			var parentWindow = element.FindParent(new Locator(Locator.WhereFind.Parent,controlType: ControlType.Window));
			element.DropDown();
			var window = parentWindow.FindChild(new Locator(Locator.WhereFind.Children,controlType: ControlType.Window));

			var list = window.FindDescendant(new Locator(Locator.WhereFind.Descendants,controlType: ControlType.List));
			var scroll = list.FindDescendantOrNull(new Locator(Locator.WhereFind.Descendants,"scroll bar", controlType: ControlType.ScrollBar));
			var listItem = list.FindDescendantOrNull(new Locator(Locator.WhereFind.Descendants,name: text, controlType: ControlType.ListItem));
			if (scroll == null)
			{
				if (listItem == null)
					throw new Exception($"Can't find listItem with text '{text}' in drop down list");
				new ControlBase(listItem).Click();
			}
			else
			{
				for (int i = 0; i < 10; i++)
				{
					listItem = list.FindDescendantOrNull(new Locator(Locator.WhereFind.Descendants,name: text, controlType: ControlType.ListItem));
					if (listItem != null)
					{
						new ControlBase(listItem).Click();
						break;
					}
					new ScrollBarControl(scroll).ScrollDown();

				}
			}
			Wait.Sleep(TimeSpan.FromMilliseconds(500));
		}

	}
}
