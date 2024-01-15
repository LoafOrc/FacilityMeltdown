using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using RuntimeNetcodeRPCValidator;
using Unity.Netcode;
using System.Runtime.Serialization;

namespace FacilityMeltdown.Networking {
    [Serializable]
    public class SyncedInstance<T> {
        public static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        public static bool IsClient => NetworkManager.Singleton.IsClient;
        public static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized] protected static int IntSize = 4;
        [NonSerialized] static readonly DataContractSerializer serializer = new DataContractSerializer(typeof(T));

        internal static T Default { get; private set; }
        internal static T Instance { get; private set; }

        internal static bool Synced;

        protected void InitInstance(T instance) {
            Default = instance;
            Instance = instance;

            // Ensures the size of an integer is correct for the current system.
            IntSize = sizeof(int);
        }

        internal static void SyncInstance(byte[] data) {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }

        internal static void RevertSync() {
            Instance = Default;
            Synced = false;
        }

        public static byte[] SerializeToBytes(T val) {
            MemoryStream stream = new MemoryStream();

            try {
                serializer.WriteObject(stream, val);
                return stream.ToArray();
            } catch (Exception e) {
                MeltdownPlugin.logger.LogError($"Error serializing instance: {e}");
                return null;
            }
        }

        public static T DeserializeFromBytes(byte[] data) {
            MemoryStream stream = new MemoryStream(data);

            try {
                return (T)serializer.ReadObject(stream);
            } catch (Exception e) {
                MeltdownPlugin.logger.LogError($"Error deserializing instance: {e}");
                return default;
            }
        }

        internal static void SendMessage(string label, ulong clientId, FastBufferWriter stream) {
            bool fragment = stream.Capacity >= stream.MaxCapacity;
            NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;

            if (fragment) {
                MeltdownPlugin.logger.LogDebug(
                    $"Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
                    "Config instance will be sent in fragments to avoid overflowing the buffer."
                );
            }

            MessageManager.SendNamedMessage(label, clientId, stream, delivery);
        }
    }
}
