using FacilityMeltdown.MeltdownSequence.Behaviours;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FacilityMeltdown.Patches;
[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch {
    internal class ReactorHealthReport {
        public float reactorInstability, timeRemaining;
        public float generatedAt = Time.time;

        public string GetFlavourText() {
            if(timeRemaining / MeltdownPlugin.config.MELTDOWN_TIME.Value > .75f) {
                return "reactorscan.result.flavour.start";
            } else if(timeRemaining / MeltdownPlugin.config.MELTDOWN_TIME.Value > .5f) {
                return "reactorscan.result.flavour.low";
            } else if(timeRemaining / MeltdownPlugin.config.MELTDOWN_TIME.Value > .33f) {
                return "reactorscan.result.flavour.medium";
            } else if(timeRemaining / MeltdownPlugin.config.MELTDOWN_TIME.Value > .15f) {
                return "reactorscan.result.flavour.high";
            }
            return "";
        }

        public string GetTeminalOutput() {
            String output = "reactorscan.result.unstable".Translate();
            output = SubstituteVariables(output);
            output += "\n\n" + SubstituteVariables(GetFlavourText().Translate()) + "\n\n";
            return output;
        }
    }

    internal static float lastHealthCheck = 0;
    internal static ReactorHealthReport lastReport = null;


    internal static string SubstituteVariables(string text) {

        StringBuilder builder = new StringBuilder(text);

        builder.Replace("<cooldown>", MeltdownPlugin.config.SCAN_COOLDOWN.Value.ToString());
        builder.Replace("<instability>", lastReport.reactorInstability.ToString());
        builder.Replace("<time_left>", lastReport.timeRemaining.ToString());

        return builder.ToString();
    }

    internal static string GetTextForNode() {
        if(MeltdownHandler.Instance) {
            string prefix = "reactorscan.error.overheat";
            if(ReactorHealthCheckReady()) {
                lastHealthCheck = Time.time;
                lastReport = GetNewReactorHealthReport();
                prefix = "reactorscan.success";
            }

            return SubstituteVariables(prefix.Translate()) + lastReport.GetTeminalOutput();

        } else {
            return "reactorscan.result.stable".Translate();
        }
    }

    internal static bool ReactorHealthCheckReady() {
        return Time.time >= lastHealthCheck + MeltdownPlugin.config.SCAN_COOLDOWN.Value;
    }

    internal static ReactorHealthReport GetNewReactorHealthReport() {
        float reactorInstability = ((MeltdownPlugin.config.MELTDOWN_TIME.Value - MeltdownHandler.Instance.meltdownTimer) / MeltdownPlugin.config.MELTDOWN_TIME.Value) * 100; // this is at perfect accuracy
        reactorInstability = Mathf.Round(reactorInstability / MeltdownPlugin.config.SCAN_ACCURACY.Value) * MeltdownPlugin.config.SCAN_ACCURACY.Value; // now the ship is not 100% perfect but still consistent (unlike a random value)

        float timeRemaining = (1 - (reactorInstability / 100)) * MeltdownPlugin.config.MELTDOWN_TIME.Value; // not perfectly accurate either

        ReactorHealthReport report = new ReactorHealthReport {
            reactorInstability = reactorInstability,
            timeRemaining = timeRemaining
        };

        return report;
    }

    internal static List<TerminalKeyword> terminalKeywordsToRegister = [];
    internal static List<TerminalNode> terminalNodesToRegister = [];

    [HarmonyPostfix, HarmonyPatch(nameof(Terminal.Awake))]
    internal static void RegisterNodes(Terminal __instance) {
        __instance.terminalNodes.allKeywords = __instance.terminalNodes.allKeywords.AddRangeToArray([.. terminalKeywordsToRegister]);
        __instance.terminalNodes.terminalNodes.AddRange(terminalNodesToRegister);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(Terminal.TextPostProcess))]
    internal static void ProcessReactorText(TerminalNode node, ref string __result) {
        if(node == MeltdownPlugin.assets.reactorHealthNode) {
            __result = "\n\n\n" + GetTextForNode();
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StormyWeather), nameof(StormyWeather.GetMetalObjectsAfterDelay))]
    internal static void CancelCoroutine(StormyWeather __instance, ref IEnumerator __result) {
        __instance.StopCoroutine(__result);
    }
}
