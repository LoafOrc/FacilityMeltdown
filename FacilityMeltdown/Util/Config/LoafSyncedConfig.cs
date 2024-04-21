using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Util.Config;
internal abstract class LoafSyncedConfig<T> : LoafConfig<T> where T : LoafSyncedConfig<T>
{
    internal static T Default { get; private set; } = null;

    internal LoafSyncedConfig(ConfigFile file) : base(file)
    {
        if (Default == null)
            Default = (T)this;
    }


}
