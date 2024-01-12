using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.API;
using FacilityMeltdown.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace FacilityMeltdown.Effects {
    internal class InsideFacilityParticleEffects : MeltdownSequenceEffect {
        public InsideFacilityParticleEffects() : base(MeltdownPlugin.modGUID, "InsideFacilityEffects") {}


        public override IEnumerator Play(float timeLeftUntilMeltdown) {
            for (int i = 0; i < Random.Range(5, 15); i++) { // todo: make this scale based on map size
                Vector3 position = GetRandomPositionInsideFacility() + Vector3.up;
                RaycastHit hit;
                if (Physics.Raycast(new Ray(position, Vector3.up), out hit, 20f, 256)) {
                    GameObject prefab = Assets.facilityEffects[Random.Range(0, Assets.facilityEffects.Length)];
                    GameObject created = GameObject.Instantiate(prefab);
                    prefab.transform.position = hit.point;
                    created.transform.parent = gameObject.transform;
                }
                MeltdownPlugin.logger.LogWarning("Failed to spawn effect, raycast failed.");
            }

            if (player.isInsideFactory)
                GameObject.Instantiate(StartOfRound.Instance.explosionPrefab, GetRandomPositionNearPlayer(), Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform);
            if (MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
                if (timeLeftUntilMeltdown > 60) {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                } else {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                }
            }

            yield return new WaitForSeconds(Random.Range(8f, 14f));
            yield break;
        }

        public override void Cleanup() {
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--) {
                GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
            }
            base.Cleanup();
        }
    }
}
