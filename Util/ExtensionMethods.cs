using FacilityMeltdown.Lang;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FacilityMeltdown
{
    internal static class ExtensionMethods {
        private static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static List<PlayerControllerB> GetConnectedPlayers(this StartOfRound startOfRound) {
            return startOfRound.allPlayerScripts.Where((player) => player.isPlayerControlled).ToList();
        }
        public static void SpawnEnemy(this EnemyVent vent, SpawnableEnemyWithRarity enemy) {
            Vector3 position = vent.floorNode.position;
            float y = vent.floorNode.eulerAngles.y;
            RoundManager.Instance.SpawnEnemyOnServer(position, y, RoundManager.Instance.currentLevel.Enemies.IndexOf(enemy));
            MeltdownPlugin.logger.LogInfo("Verifing... " + RoundManager.Instance.currentLevel.Enemies[RoundManager.Instance.currentLevel.Enemies.IndexOf(enemy)].enemyType.enemyName);
            vent.OpenVentClientRpc();
            vent.occupied = false;
        }

        internal static String Translate(this String text) {
            return LangParser.GetTranslation(text);
        }

        // fixme: yeah lol shouldn't need to do this but the publiczer isnt work for csync so meh
        internal static T GetFieldValue<T>(this object obj, string name) {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}
