using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Util.Config;
[AttributeUsage(AttributeTargets.Property)]
internal class ConfigRangeAttribute : Attribute {
    internal float Min { get; private set; }
    internal float Max { get; private set; }

    internal ConfigRangeAttribute(float min, float max) {
        this.Min = min;
        this.Max = max;
    }
}
