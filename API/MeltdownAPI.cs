using FacilityMeltdown.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.API {
    public static class MeltdownAPI {
        internal static Action OnMeltdownStart = delegate { };

        private static List<MeltdownSequenceEffect> _registedEffects = new List<MeltdownSequenceEffect>();
        private static List<ApparatusEvaluator> _evaluators = new List<ApparatusEvaluator>();

        public static IReadOnlyCollection<MeltdownSequenceEffect> RegisteredEffects { get { return _registedEffects.AsReadOnly(); } }
        public static IReadOnlyCollection<ApparatusEvaluator> RegisteredEvaluators { get { return _evaluators.AsReadOnly(); } }

        public static void RegisterEffect(MeltdownSequenceEffect effect) {
            _registedEffects.Add(effect);
            MeltdownPlugin.logger.LogInfo($"[API] Succesfully registered {effect.GetType().Name}!");
        }

        public static void RegisterEffect(MeltdownSequenceEffect[] effects) {
            foreach (MeltdownSequenceEffect effect in effects) { 
                RegisterEffect(effect);
            }
        }

        public static void RegisterEvaluator(ApparatusEvaluator effect) {
            _evaluators.Add(effect);
            MeltdownPlugin.logger.LogInfo($"[API] Succesfully registered {effect.GetType().Name}!");
        }

        public static void RegisterEvaluator(ApparatusEvaluator[] effects) {
            foreach (ApparatusEvaluator effect in effects) {
                RegisterEvaluator(effect);
            }
        }

        public static void RegisterMeltdownStartListener(Action listener) {
            OnMeltdownStart += listener;
            MeltdownPlugin.logger.LogInfo($"[API] {(new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name} registered a listener for the meltdown start!");
        }

        public static void TriggerMeltdown() {
            if (MeltdownHandler.Instance != null) return;
            MeltdownPlugin.logger.LogInfo($"[API] Another mod ({(new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name}), manually triggered a meltdown!");

            // We just took it out
            try { // make sure to surround in try catch because this is a prefix
                GameObject.Instantiate(Assets.meltdownHandlerPrefab).GetComponent<NetworkObject>().Spawn();
            } catch (Exception ex) {
                MeltdownPlugin.logger.LogError(ex);
            }
        }
    }
}
