using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FacilityMeltdown.Behaviours {
    public class RadiationSource : MonoBehaviour {
        public static float CollectRadiationFromPoint(Vector3 point) {
            float radiation = 0;
            foreach(RadiationSource source in radiators) {
                radiation += source.GetRadiationFromPoint(point);
            }
            return radiation;
        }

        [HideInInspector]
        public static List<RadiationSource> radiators = new List<RadiationSource>();

        [Range(0, 100)]
        public float radiationAmount = 70;

        public bool isGlobal = false;
        [Range(0, 100)]
        public float radiationDistance = 50;

        [Header("Decay Settings")]
        public bool doesDecay = false;
        [Range(0, 120)]
        public float decayTime = 120;
        public AnimationCurve radiationOutputVsDecay = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0f, 1f));

        float existingTime = 0f;

        public float GetRadiationAtDistance(float distance) {
            if(distance > radiationDistance) return 0;
            float distance01 = Mathf.Clamp01((radiationDistance - distance) / radiationDistance);
            if (isGlobal) distance01 = 1;

            return distance01 * radiationAmount * GetDecayReduction();
        }

        public float GetDecayProgress() {
            if(!doesDecay) return 0;
            return Mathf.Clamp01(existingTime / decayTime);
        }

        public float GetDecayReduction() {
            if(!doesDecay) return 1;
            return radiationOutputVsDecay.Evaluate(GetDecayProgress());
        }

        public float GetRadiationFromPoint(Vector3 point) {
            return GetRadiationAtDistance(Vector3.Distance(transform.position, point));
        }

        void OnEnable() {
            radiators.Add(this);
            MeltdownPlugin.logger.LogInfo($"A new RadiationSource was created, there is now: {radiators.Count} radiators.");
        }

        void OnDisable() {
            radiators.Remove(this);
        }

        void Update() {
            existingTime += Time.deltaTime;
        }
    }
}
