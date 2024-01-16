using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using TerminalApi.Classes;
using UnityEngine;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace FacilityMeltdown {
    internal class TerminalHandler {
        internal struct ReactorHealthReport {
            public float reactorInstability, timeRemaining;

            public string GetFlavourText() {
                if (timeRemaining > 90) {
                    return "The reactor is beginning to fail. It is recommened to finish up and get ready to leave.";
                } else if (timeRemaining > 60) {
                    return "The reactor is becoming more unstable, it is about to fail. Do not leave the ship or you risk company property. Any crew memebers still outside the ship are advised to return before the reactor completly fails.";
                } else if (timeRemaining > 40) {
                    return "The reactor is failing. You have very little time remaining before a catastrophic nuclear event happens.";
                } else if (timeRemaining > 20) {
                    return "The reactor is about to cause a catastrophic nuclear event. This is your last chance. Any remaining crew members have very little time before a massive reactor fail. If there are still in the facility they most likely are not making it out, on their own.";
                }
                return "";
            }

            public string GetTeminalOutput() {
                return $"Reactor instability at {reactorInstability}%.\nApproximately {timeRemaining} seconds left until catastrophic nuclear event.\n\n{GetFlavourText()}\n\n";
            }
        }

        internal const float reactorHealthCooldown = 15;
        internal const float reactorAccuracy = 10;

        internal static float lastHealthCheck = 0;
        internal static ReactorHealthReport lastReport;

        internal static bool ReactorHealthCheckReady() {
            return Time.time >= lastHealthCheck + reactorHealthCooldown;
        }

        internal static ReactorHealthReport GetNewReactorHealthReport() {
            float reactorInstability = (((2 * 60) - MeltdownHandler.Instance.meltdownTimer) / (2 * 60)) * 100; // this is at perfect accuracy
            reactorInstability = Mathf.Round(reactorInstability / reactorAccuracy) * reactorAccuracy; // now the ship is not 100% perfect but still consistent (unlike a random value)

            float timeRemaining = (1 - (reactorInstability / 100)) * (2 * 60); // not perfectly accurate either

            ReactorHealthReport report = new ReactorHealthReport();
            report.reactorInstability = reactorInstability;
            report.timeRemaining = timeRemaining;

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
                        string prefix = "USING LAST CACHED REPORT!\nThe ship's scanner needs to cooldown before you can scan the reactors health again!\n\n";
                        if(ReactorHealthCheckReady()) {
                            prefix = $"Generated a new reactor report, saving to cache. {reactorHealthCooldown} seconds until scanners are ready to generate another one.\n\n";
                            lastHealthCheck = Time.time;
                            lastReport = GetNewReactorHealthReport();
                        }

                        return prefix + lastReport.GetTeminalOutput();

                    } else {
                        return "Reactor instability at 0%.\nThe reactor is in good health. No caution is necessary.\n\n";
                    }
                },
                Category = "Other",
                Description = "do actions with the reactor"
                // The above would look like '>FRANK\nThis is just a test command.' in Other
            });
        }
    }
}
