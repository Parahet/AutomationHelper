using System;

namespace AutomationHelper.Base
{
	class TextAttribute : Attribute
	{
		public string Text;
		public TextAttribute(string text)
		{
			Text = text;
		}
	}

	public static partial class EnumExtensions
	{
		public static string ToText(this Enum enumeration)
		{
			var memberInfo = enumeration.GetType().GetMember(enumeration.ToString());
			if (memberInfo.Length > 0)
			{
				object[] attributes = memberInfo[0].GetCustomAttributes(typeof(TextAttribute), false);
				if (attributes.Length > 0)
				{
					return ((TextAttribute)attributes[0]).Text;
				}
			}
			return enumeration.ToString();
		}
	}
}
