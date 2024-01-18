using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace FacilityMeltdown.Networking {
    public abstract class SaveableNetworkBehaviour : NetworkBehaviour {
        public int uniqueId = 0;

        public abstract void SaveObjectData();

        public abstract void LoadObjectData();
    }
}
