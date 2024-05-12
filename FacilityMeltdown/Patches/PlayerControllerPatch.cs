using FacilityMeltdown.Config;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Serialization;
using Unity.Collections;
using Unity.Netcode;

namespace FacilityMeltdown.Patches;
[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerPatch {
    const string MESSAGE_REQUEST = "meltdown_OnRequestConfigSync";
    const string MESSAGE_RECIEVE = "meltdown_OnRecieveConfigSync";

    const int IntSize = sizeof(int);

    [HarmonyPostfix, HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    static void SyncConfig() {
        if(NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MESSAGE_REQUEST, OnRequestSync);

            return;
        }

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MESSAGE_RECIEVE, OnReceiveSync);
        RequestSync();
    }

    public static void RequestSync() {
        if(NetworkManager.Singleton.IsHost) return;

        using FastBufferWriter stream = new(IntSize, Allocator.Temp);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MESSAGE_REQUEST, 0uL, stream);
    }

    public static void OnRequestSync(ulong clientId, FastBufferReader _) {
        if(!NetworkManager.Singleton.IsHost) return;

        MeltdownPlugin.logger.LogInfo($"Config sync request received from client: {clientId}");

        byte[] array = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MeltdownPlugin.config));
        int value = array.Length;

        using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

        try {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MESSAGE_RECIEVE, clientId, stream);
        } catch(Exception e) {
            MeltdownPlugin.logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }

    static void OnReceiveSync(ulong _, FastBufferReader reader) {
        if(!reader.TryBeginRead(IntSize)) {
            MeltdownPlugin.logger.LogError("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if(!reader.TryBeginRead(val)) {
            MeltdownPlugin.logger.LogError("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        MeltdownPlugin.logger.LogDebug(Encoding.UTF8.GetString(data));

        MeltdownPlugin.config = JsonConvert.DeserializeObject<MeltdownConfig>(Encoding.UTF8.GetString(data), new JsonSerializerSettings {
            ContractResolver = new IncludePrivateSetterContractResolver()
        });
        MeltdownPlugin.logger.LogDebug("Deserialized values are: "); 

        foreach (PropertyInfo property in MeltdownPlugin.config.GetType().GetProperties()) {
            MeltdownPlugin.logger.LogDebug($"{property.Name} => {property.GetValue(MeltdownPlugin.config)}");
        }
        
        MeltdownPlugin.logger.LogInfo("Successfully synced config with host.");
    }
}

internal class IncludePrivateSetterContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        // If the property is not writable and it's a property with a private setter, set it to writable
        if (!property.Writable && member is PropertyInfo propInfo)
        {
            property.Writable = propInfo.GetSetMethod(true) != null;
        }

        return property;
    }
}