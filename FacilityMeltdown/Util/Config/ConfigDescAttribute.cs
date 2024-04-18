using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Util.Config;
[AttributeUsage(AttributeTargets.Property)]
internal class ConfigDescAttribute(string desc) : Attribute {
    public string Description { get; private set; } = desc;
}
