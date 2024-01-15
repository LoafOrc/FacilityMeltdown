using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

namespace FacilityMeltdown.API {
    public abstract class MeltdownSequenceEffect {
        protected PlayerControllerB player => GameNetworkManager.Instance.localPlayerController;
        protected Vector3 mainEntrancePosition { get; private set; }

        public static List<MeltdownSequenceEffect> effects = new List<MeltdownSequenceEffect>();

        public bool Playing { get; protected set; }
        public bool IsOneShot { get; protected set; }
        public string Name { get; private set; }
        public string ModGUID { get; private set; }
        public string FullName { get; private set; }

        public GameObject gameObject { get; private set; }

        public MeltdownSequenceEffect(string modGUID, string name) {
            Name = name;
            ModGUID = modGUID;
            FullName = modGUID + "." + name;

            effects.Add(this);
        }

        public virtual void Setup() { 
            Playing = true;
            gameObject = new GameObject(Name + "Handler");
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
