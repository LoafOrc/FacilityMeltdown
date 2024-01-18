using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Networking;

namespace FacilityMeltdown.Behaviours {
    public abstract class SaveableObject : GrabbableObject {
        public int uniqueId = 0;

        public override void LoadItemSaveData(int saveData) {
            base.LoadItemSaveData(saveData);

            uniqueId = saveData;
        }

        public override int GetItemDataToSave() {
            return uniqueId;
        }

        public virtual void Awake() {
            if (IsHost) {
                uniqueId = UnityEngine.Random.Range(0, 1000000);

                var SaveableNetworkBehaviours = transform.GetComponentsInChildren<SaveableNetworkBehaviour>();

                foreach (var item in SaveableNetworkBehaviours) {
                    item.uniqueId = uniqueId;
                }
            }
        }


        public abstract void SaveObjectData();

        public abstract void LoadObjectData();
    }
}
