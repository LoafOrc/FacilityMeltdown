using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using UnityEngine;

namespace FacilityMeltdown.API {
    public abstract class MeltdownSequenceEffect {
        protected PlayerControllerB player => GameNetworkManager.Instance.localPlayerController;
        protected Vector3 mainEntrancePosition { get; private set; }


        public bool Playing { get; protected set; }
        public bool IsOneShot { get; protected set; }

        public GameObject gameObject => MeltdownHandler.Instance.gameObject;

        public virtual void Setup() { 
            Playing = true;
            mainEntrancePosition = RoundManager.FindMainEntrancePosition(false, true);
        }
        public virtual IEnumerator Play(float timeLeftUntilMeltdown) { yield return null; yield break; }
        public virtual IEnumerator Stop() { yield return null; yield break; }
        public virtual void Cleanup() {
            GameObject.Destroy(gameObject);
        }

        public virtual bool IsEnabledOnThisMoon(SelectableLevel level) { 
            return true; 
        }

        protected float GetMeltdownProgress(float time) {
            return 1-(time / MeltdownConfig.Instance.MELTDOWN_TIME.Value);
        }
        protected Vector3 PlacePositionInsideFacility(Vector3 position, float radius = 10f) {
            return RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, layerMask: -1, randomSeed: new System.Random());
        }

        protected Vector3 GetRandomPositionNearPlayer(float radius = 15f) {
            return PlacePositionInsideFacility(player.transform.position + (Random.insideUnitSphere * radius));
        }

        protected Vector3 GetRandomPositionInsideFacility() {
            return PlacePositionInsideFacility(RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position);
        }
    }
}
