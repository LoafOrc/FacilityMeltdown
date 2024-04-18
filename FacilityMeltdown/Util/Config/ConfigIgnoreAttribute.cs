using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Util.Config;
[AttributeUsage(AttributeTargets.Property)]
internal class ConfigIgnoreAttribute : Attribute {
}
