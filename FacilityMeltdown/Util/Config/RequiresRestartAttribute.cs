using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Util.Config;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
internal class RequiresRestartAttribute(bool requiresRestart = true) : Attribute {
    public bool RequiresRestart { get; private set; } = requiresRestart;
}
