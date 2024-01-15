using System.Collections;
using FacilityMeltdown.API;
using FacilityMeltdown.Util;
using UnityEngine;

namespace FacilityMeltdown.Effects {
    internal class EmergencyLightsEffect : MeltdownSequenceEffect {
        public EmergencyLightsEffect() : base(MeltdownPlugin.modGUID, "EmergencyLights") {}

        public override void Setup() {
            base.Setup();
            for (int i = 0; i < RoundManager.Instance.allPoweredLights.Count; i++) {
                RoundManager.Instance.allPoweredLights[i].color = Color.red;
            }
        }
        public override IEnumerator Play(float timeLeftUntilMeltdown) {
            for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++) {
                RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", true);
            }

            yield return new WaitForSeconds(2f);

            for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++) {
                RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", false);
            }

            yield return new WaitForSeconds(5f);
            yield break;
        }

        public override bool IsEnabledOnThisMoon(SelectableLevel level) {
            return MeltdownConfig.Instance.EMERGENCY_LIGHTS;
        }
    }
}
