using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;
using AutomationHelper.Base;

namespace AutomationHelper.Desktop
{
	public class Locator
	{
		public enum WhereFind
		{
			Children,
			Descendants,
			Parent
		}
		public Locator ParentLocator{ get; set; }
		public List<LocatorProperty> Properties { get; set; }
		public WhereFind? Where { get; set; }
		private readonly Condition _condition;
		public Condition Condition
		{
			get
			{
				if (_condition == null)
				{
					if (Properties != null)
						return new OrCondition(Properties.Select(p => p.ToAndCondition()).ToArray());
					else
						throw new Exception("There is no Properties in the locator");
				}
				return _condition;
			}
		}

		public Locator(WhereFind where, string name = null, string automationId = null, string className = null,
			ControlType controlType = default(ControlType))
		{
			var prop = new LocatorProperty
			{
				Name = name,
				AutomationId = automationId,
				ClassName = className,
				ControlType = controlType
			};
			_condition = prop.ToAndCondition();
			Where = where;
			Properties = new List<LocatorProperty>(){prop};
		}
		public Locator(WhereFind where, LocatorProperty property)
		{
			var prop = property;
			_condition = prop.ToAndCondition();
			Where = where;
			Properties = new List<LocatorProperty>() { prop };
		}

		/// <summary>
		/// Implement or properties
		/// </summary>
		/// <param name="Properties"></param>
		public Locator(WhereFind where, params LocatorProperty[] orProperties)
		{
			Properties = orProperties.ToList();
			Where = where;
		}
		
		public static Locator ByChained(params Locator[] locators)
		{
			if (!locators.Any())
				throw new Exception("There are no any locator in the chain");
			if(locators.Any(l=>l == null))
				throw new Exception("There are null locator in the chain");
			Locator resultLocator = locators[0];
			for (int i = 1; i < locators.Length; i++)
			{
				locators[i].ParentLocator = locators[i - 1];
				resultLocator = locators[i];
			}
			return resultLocator;
		}
		/*
		public string Name { get; set; } 
		public string ClassName { get; set; }
		public string AutomationId { get; set; } 
		public ControlType ControlType { get; set; } = default(ControlType);*/
		
		public override string ToString()
		{
			return Properties.Select(p => p.ToString()).JoinByComma();
		}
		private AutomationElement findElement(Locator locator)
		{

			if (locator.ParentLocator == null)
			{
				return AutomationElement.RootElement.FindElement(locator);
			}
			return findElement(locator.ParentLocator).FindElement(locator);
		}
		private AutomationElement findElementOrNull(Locator locator)
		{
			if (locator.ParentLocator == null)
				return AutomationElement.RootElement.FindElementOrNull(locator);
			var parent = findElementOrNull(locator.ParentLocator);
			if (parent == null)
				return null;
			else return parent.FindElementOrNull(locator);
		}
		private List<AutomationElement> findElements(Locator locator)
		{
			if (locator.ParentLocator == null)
				return AutomationElement.RootElement.FindElements(locator);
			var parent = findElement(locator.ParentLocator);
			return parent.FindElements(locator);
		}

		public AutomationElement FindElement()
		{
			return findElement(this);
		}
		public List<AutomationElement> FindElements()
		{
			return findElements(this);
		}
		public AutomationElement FindElementOrNull()
		{
			return findElementOrNull(this);
		}
	}
}
