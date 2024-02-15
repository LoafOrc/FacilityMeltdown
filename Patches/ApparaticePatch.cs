using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Util;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(LungProp))]
    internal class ApparaticePatch {
        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.EquipItem))]
        internal static void BeginMeltdownSequence(LungProp __instance, ref bool ___isLungDocked) {
            if (!__instance.IsHost) return;
            if (!___isLungDocked) return;
            if (MeltdownHandler.Instance != null) return;

            // We just took it out
            try { // make sure to surround in try catch because this is a prefix
                if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE.Value) {
                    Stopwatch spawnEnemiesTimer = Stopwatch.StartNew();
                    List<string> disallowed = MeltdownConfig.Instance.GetDisallowedEnemies();
                    List<SpawnableEnemyWithRarity> allowedEnemies = RoundManager.Instance.currentLevel.Enemies.Where(enemy => { return !disallowed.Contains(enemy.enemyType.enemyName) && !EnemyCannotBeSpawned(enemy.enemyType); }).ToList();
                    int meltdownEnemyPower = 0;
                    List<int> spawnProbibilities = allowedEnemies.Select(enemy => { return enemy.rarity; }).ToList();

                    List<EnemyVent> avaliableVents = RoundManager.Instance.allEnemyVents.Where(vent => { return !vent.occupied; }).ToList();
                    avaliableVents.Shuffle();

                    for (int i = 0; i < Mathf.Min(MeltdownConfig.Instance.MONSTER_SPAWN_AMOUNT.Value, avaliableVents.Count); i++) {
                        EnemyVent vent = avaliableVents[i];
                        int randomWeightedIndex = RoundManager.Instance.GetRandomWeightedIndex([.. spawnProbibilities], RoundManager.Instance.EnemySpawnRandom);
                        if (EnemyCannotBeSpawned(allowedEnemies[randomWeightedIndex].enemyType)) continue;
                        RoundManager.Instance.currentEnemyPower += allowedEnemies[randomWeightedIndex].enemyType.PowerLevel;
                        meltdownEnemyPower += allowedEnemies[randomWeightedIndex].enemyType.PowerLevel;

                        MeltdownPlugin.logger.LogInfo("Spawning a " + allowedEnemies[randomWeightedIndex].enemyType.enemyName + " during the meltdown sequence");
                        vent.SpawnEnemy(allowedEnemies[randomWeightedIndex]);
                    }
                    spawnEnemiesTimer.Stop();
                    MeltdownPlugin.logger.LogInfo($"[Measurements] Spawning enemies took: {spawnEnemiesTimer.ElapsedMilliseconds}ms.");

                    Stopwatch calculateApparatusValue = Stopwatch.StartNew();
                    int baseValue = MeltdownConfig.Instance.APPARATUS_VALUE.Value;
                    int quotaValue = Mathf.RoundToInt(TimeOfDay.Instance.profitQuota * MeltdownConfig.Instance.APPARATUS_VALUE_BY_QUOTA.Value);
                    int distanceValue = Mathf.RoundToInt(Vector3.Distance(RoundManager.FindMainEntrancePosition(false, false), __instance.transform.position) * MeltdownConfig.Instance.APPARATUS_VALUE_BY_DISTANCE.Value);
                    int enemyPowerValue = Mathf.RoundToInt(meltdownEnemyPower * MeltdownConfig.Instance.APPARATUS_VALUE_BY_ENEMY_POWER.Value);

                    MeltdownPlugin.logger.LogDebug($"Evaluating Apparatus Value...");
                    MeltdownPlugin.logger.LogDebug($"Base Value = {baseValue}");
                    MeltdownPlugin.logger.LogDebug($"Quota Value = {TimeOfDay.Instance.profitQuota} x {MeltdownConfig.Instance.APPARATUS_VALUE_BY_QUOTA.Value} = {quotaValue}");
                    MeltdownPlugin.logger.LogDebug($"Distance Value = {Vector3.Distance(RoundManager.FindMainEntrancePosition(false, false), __instance.transform.position)} x {MeltdownConfig.Instance.APPARATUS_VALUE_BY_DISTANCE.Value} = {distanceValue}");
                    MeltdownPlugin.logger.LogDebug($"Enemy Power Value = {meltdownEnemyPower} x {MeltdownConfig.Instance.APPARATUS_VALUE_BY_ENEMY_POWER.Value} = {enemyPowerValue}");
                    
                    __instance.scrapValue = baseValue + quotaValue + distanceValue + enemyPowerValue;

                    MeltdownPlugin.logger.LogDebug($"Therefore the total value is: {__instance.scrapValue}");
                    calculateApparatusValue.Stop();
                    MeltdownPlugin.logger.LogInfo($"[Measurements] Calculating apparatus value took: {calculateApparatusValue.ElapsedMilliseconds}ms.");
                }
                GameObject meltdown = GameObject.Instantiate(Assets.meltdownHandlerPrefab);
                meltdown.GetComponent<NetworkObject>().Spawn();
            } catch (Exception ex) {
                MeltdownPlugin.logger.LogError(ex);
            }
        }

        static internal bool EnemyCannotBeSpawned(EnemyType type) {
            return type.spawningDisabled || type.numberSpawned >= type.MaxCount;
        }

        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.Start))]
        internal static void AddRadiationSource(LungProp __instance) {
            try {
                RadiationSource source = __instance.gameObject.AddComponent<RadiationSource>();
                source.radiationAmount = 40;
                source.radiationDistance = 40;

                if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE.Value)
                    __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE.Value;
                //___isLungDocked = false; // fix joining late
            } catch (Exception ex) {
                MeltdownPlugin.logger.LogError(ex);
            }
        }
    }
}
