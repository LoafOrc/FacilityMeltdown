using FacilityMeltdown.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.API {
    public static class MeltdownAPI {
        private static List<MeltdownSequenceEffect> _registedEffects = new List<MeltdownSequenceEffect>();

        public static IReadOnlyCollection<MeltdownSequenceEffect> RegisteredEffects { get { return _registedEffects.AsReadOnly(); } }

        public static void RegisterEffect(MeltdownSequenceEffect effect) {
            _registedEffects.Add(effect);
            MeltdownPlugin.logger.LogInfo($"[API] Succesfully registered {effect.Name}!");
        }

        public static void RegisterEffect(MeltdownSequenceEffect[] effects) {
            foreach (MeltdownSequenceEffect effect in effects) { 
                RegisterEffect(effect);
            }
        }

        public static void TriggerMeltdown(string modGUID) {
            if (MeltdownHandler.Instance != null) return;
            MeltdownPlugin.logger.LogInfo($"[API] Another mod ({modGUID}), manually triggered a meltdown!");

            // We just took it out
            try { // make sure to surround in try catch because this is a prefix
                GameObject.Instantiate(Assets.meltdownHandlerPrefab).GetComponent<NetworkObject>().Spawn();
            } catch (Exception ex) {
                MeltdownPlugin.logger.LogError(ex);
            }
        }
    }
}
