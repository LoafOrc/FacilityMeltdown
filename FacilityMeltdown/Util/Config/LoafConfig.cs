using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace FacilityMeltdown.Util.Config;
[Serializable]
internal abstract class LoafConfig<T> where T : LoafConfig<T>
{
    internal List<(PropertyInfo, object)> configEntries = [];
    internal ConfigFile configFile { get; private set; }

    public LoafConfig(ConfigFile configFile)
    {
        if(configFile == null) return;
        this.configFile = configFile;
        string CurrentHeader = "Misc";
        Type type = typeof(T);
        foreach (PropertyInfo property in type.GetProperties())
        {
            if (property.GetCustomAttribute(typeof(ConfigIgnoreAttribute)) != null) continue;

            ConfigGroupAttribute headerAttribute = (ConfigGroupAttribute)property.GetCustomAttribute(typeof(ConfigGroupAttribute));
            if (headerAttribute != null)
            {
                CurrentHeader = headerAttribute.Group.Replace(" ", "");
            }
            string description = "no description here :pensive:";
            ConfigDescAttribute tooltipAttribute = (ConfigDescAttribute)property.GetCustomAttribute(typeof(ConfigDescAttribute));
            if (tooltipAttribute != null)
            {
                description = tooltipAttribute.Description;
            }


            ConfigDescription configDescription = new ConfigDescription(description);
            ConfigRangeAttribute rangeAttribute = (ConfigRangeAttribute)property.GetCustomAttribute(typeof(ConfigRangeAttribute));
            if (rangeAttribute != null)
            {
                if (property.PropertyType == typeof(int))
                {
                    configDescription = new ConfigDescription(description, new AcceptableValueRange<int>((int)rangeAttribute.Min, (int)rangeAttribute.Max));
                }
                else
                {
                    configDescription = new ConfigDescription(description, new AcceptableValueRange<float>(rangeAttribute.Min, rangeAttribute.Max));
                }
            }
            else
            {
                configDescription = new ConfigDescription(description);
            }

            // this is icky
            if(property.PropertyType == typeof(float)) {
                BindProperty<float>(property, CurrentHeader, configDescription);
            }
            if(property.PropertyType == typeof(int)) {
                BindProperty<int>(property, CurrentHeader, configDescription);
            }
            if(property.PropertyType == typeof(string)) {
                BindProperty<string>(property, CurrentHeader, configDescription);
            }
            if(property.PropertyType == typeof(bool)) {
                BindProperty<bool>(property, CurrentHeader, configDescription);
            }
        }

        // kill all the orphans :3
    }

    protected void BindProperty<V>(PropertyInfo property, string header, ConfigDescription description) {
        ConfigEntry<V> entry = configFile.Bind(header, property.Name, (V)property.GetValue(this), description);
        configEntries.Add((property, entry));
        property.SetValue(this, entry.Value);
    }
}