using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.API;
using FacilityMeltdown.Behaviours;
using UnityEngine;

namespace FacilityMeltdown.Effects {
    internal class RadiationIncreasingEffect : MeltdownSequenceEffect {
        public RadiationIncreasingEffect() : base(MeltdownPlugin.modGUID, "RadiationIncreasing") { }

        bool radiationHudWarning = false;
        RadiationSource source;
        public override void Setup() {
            base.Setup();

            radiationHudWarning = false;

            source = gameObject.AddComponent<RadiationSource>();
            source.isGlobal = true;
            source.radiationAmount = 0;
        }

        public override IEnumerator Play(float timeLeftUntilMeltdown) {
            if(GetMeltdownProgress(timeLeftUntilMeltdown) <= .5 && !radiationHudWarning) {
                HUDManager.Instance.RadiationWarningHUD();
                radiationHudWarning = true;
            }

            source.radiationAmount = GetMeltdownProgress(timeLeftUntilMeltdown) * 100;

            yield return new WaitForSeconds(1);
            yield break;
        }
    }
}
