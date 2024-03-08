using FacilityMeltdown.Util;
using loaforcsSoundAPI.API;
using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacilityMeltdown.Integrations;
internal class MeltdownSoundAPI {
    internal class MeltdownStartedCondition : ConditionProvider {
        public override bool Evaluate(SoundReplaceGroup pack, JObject conditionDef) {
            return MeltdownHandler.Instance != null;
        }
    }

    internal class MeltdownTimeLeft : VariableProvider {
        public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
            return MeltdownHandler.Instance.meltdownTimer;
        }
    }

    internal class MeltdownProgress : VariableProvider {
        public override object Evaluate(SoundReplaceGroup group, JObject varDef) {
            return ((MeltdownConfig.Instance.MELTDOWN_TIME.Value - MeltdownHandler.Instance.meltdownTimer) / MeltdownConfig.Instance.MELTDOWN_TIME.Value) * 100;
        }
    }

    internal static void Integrate() {
        MeltdownPlugin.logger.LogInfo("Integrating into loaforcsSoundAPI.");
        SoundReplacementAPI.RegisterVariableProvider("meltdown:timeleft", new MeltdownTimeLeft());
        SoundReplacementAPI.RegisterVariableProvider("meltdown:progress", new MeltdownTimeLeft());
        SoundReplacementAPI.RegisterConditionProvider("meltdown:has_started", new MeltdownStartedCondition());
    }
}
