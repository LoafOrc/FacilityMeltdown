using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Util.Config;
[AttributeUsage(AttributeTargets.Property)]
internal class ConfigGroupAttribute(string Group) : Attribute {
    public string Group { get; private set; } = Group;
}
