using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace AutomationHelper.Desktop
{
    public class LocatorProperty
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string AutomationId { get; set; }
        public ControlType ControlType { get; set; } = default(ControlType);
        private List<PropertyCondition> GetConditionsCollection()
        {
            var conditionsCollection = new List<PropertyCondition>();
            if (Name != null)
                conditionsCollection.Add(new PropertyCondition(AutomationElement.NameProperty, Name));
            if (ClassName != null)
                conditionsCollection.Add(new PropertyCondition(AutomationElement.ClassNameProperty, ClassName));
            if (AutomationId != null)
                conditionsCollection.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, AutomationId));
            if (ControlType != default(ControlType))
                conditionsCollection.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType));
            return conditionsCollection;
        }
        public AndCondition ToAndCondition()
        {
            var conditionsCollection = GetConditionsCollection();
            if (!conditionsCollection.Any())
                return new AndCondition(Condition.TrueCondition);
            return new AndCondition(conditionsCollection.ToArray());
        }

        public override bool Equals(object obj)
        {
            var other = obj as LocatorProperty;
            if (other == null) { return false; }
            bool result = true;
            if (result && other.Name != null)
                result = other.Name == Name;
            if (result && other.AutomationId != null)
                result = other.AutomationId == AutomationId;
            if (result && other.ClassName != null)
                result = other.ClassName == ClassName;
            if (result && other.ControlType != default(ControlType))
                result = Equals(other.ControlType, ControlType);
            return result;
        }

	    public override string ToString()
	    {
		    var conditionsCollection = GetConditionsCollection();
		    if (!conditionsCollection.Any())
			    return "Condition.TrueCondition";
		    return GetConditionsCollection().Select(c => $"{c.Property.ProgrammaticName}={c.Value.ToString()}").JoinByComma();
		}
    }
}
