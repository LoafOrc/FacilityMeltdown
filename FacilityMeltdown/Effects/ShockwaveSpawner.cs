using System.Collections;
using FacilityMeltdown.API;
using FacilityMeltdown.Behaviours;
using UnityEngine;

namespace FacilityMeltdown.Effects {
    internal class ShockwaveSpawner : MeltdownSequenceEffect {
        public ShockwaveSpawner() : base(MeltdownPlugin.modGUID, "ShockwaveSpawner") {}

        GameObject shockwave;

        public override IEnumerator Play(float timeLeftUntilMeltdown) {
            if (shockwave != null) GameObject.Destroy(shockwave);

            shockwave = GameObject.Instantiate(Assets.shockwavePrefab);
            shockwave.transform.position = mainEntrancePosition;
            shockwave.AddComponent<Shockwave>();


            yield return new WaitForSeconds(Random.Range(20f, 30f));
            yield break;
        }

        public override void Cleanup() {
            base.Cleanup();
            GameObject.Destroy(shockwave);
        }
    }
}
