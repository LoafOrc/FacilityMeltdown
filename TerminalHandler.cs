using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Util;
using HarmonyLib;
using JetBrains.Annotations;
using TerminalApi;
using TerminalApi.Classes;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace FacilityMeltdown {
    internal class TerminalHandler {
        internal class ReactorHealthReport {
            public float reactorInstability, timeRemaining;
            public float generatedAt = Time.time;

            public string GetFlavourText() {
                if (timeRemaining/MeltdownConfig.Instance.MELTDOWN_TIME > .75f) {
                    return "reactorscan.result.flavour.start";
                } else if (timeRemaining / MeltdownConfig.Instance.MELTDOWN_TIME > .5f) {
                    return "reactorscan.result.flavour.low";
                } else if (timeRemaining / MeltdownConfig.Instance.MELTDOWN_TIME > .33f) {
                    return "reactorscan.result.flavour.medium";
                } else if (timeRemaining / MeltdownConfig.Instance.MELTDOWN_TIME > .15f) {
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

        internal static AudioSource source;

        internal static string SubstituteVariables(string text) {
            StringBuilder builder = new StringBuilder(text);

            builder.Replace("<cooldown>", MeltdownConfig.Instance.SHIP_SCANNER_COOLDOWN.ToString());
            builder.Replace("<instability>", lastReport.reactorInstability.ToString());
            builder.Replace("<time_left>", lastReport.timeRemaining.ToString());
            
            return builder.ToString();
        }

        internal static bool ReactorHealthCheckReady() {
            return Time.time >= lastHealthCheck + MeltdownConfig.Instance.SHIP_SCANNER_COOLDOWN;
        }

        internal static ReactorHealthReport GetNewReactorHealthReport() {
            float reactorInstability = ((MeltdownConfig.Instance.MELTDOWN_TIME - MeltdownHandler.Instance.meltdownTimer) / MeltdownConfig.Instance.MELTDOWN_TIME) * 100; // this is at perfect accuracy
            reactorInstability = Mathf.Round(reactorInstability / MeltdownConfig.Instance.SHIP_SCANNER_ACCURACY) * MeltdownConfig.Instance.SHIP_SCANNER_ACCURACY; // now the ship is not 100% perfect but still consistent (unlike a random value)

            float timeRemaining = (1 - (reactorInstability / 100)) * MeltdownConfig.Instance.MELTDOWN_TIME; // not perfectly accurate either

            ReactorHealthReport report = new ReactorHealthReport {
                reactorInstability = reactorInstability,
                timeRemaining = timeRemaining
            };

            return report;
        }

        internal static void Init() {
            TerminalNode triggerNode = CreateTerminalNode($"use >reactor health to check the current health of the reactor\n", true);
            
            TerminalKeyword verbKeyword = CreateTerminalKeyword("health", true);
            TerminalKeyword nounKeyword = CreateTerminalKeyword("reactor");

            verbKeyword = verbKeyword.AddCompatibleNoun(nounKeyword, triggerNode);
            nounKeyword.defaultVerb = verbKeyword;

            AddTerminalKeyword(verbKeyword);

            // The second parameter passed in is a CommandInfo, if you want to have a callback.
            AddTerminalKeyword(nounKeyword, new CommandInfo() {
                TriggerNode = triggerNode,
                DisplayTextSupplier = () => {
                    
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
                },
                Category = "Other",
                Description = "do actions with the reactor"
                // The above would look like '>FRANK\nThis is just a test command.' in Other
            });
        }
    }
}
