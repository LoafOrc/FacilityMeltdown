using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FacilityMeltdown {
    internal class Assets {
        internal static AssetBundle assetBundle;

        internal static AudioClip[] warnings;
        internal static AudioClip defaultMusic, shockwave;
        internal static GameObject facilityExplosionPrefab, meltdownHandlerPrefab, shockwavePrefab, geigerCounterItem;
        internal static GameObject[] facilityEffects;
        internal static Item geigerCounterItemDef;
        internal static TerminalNode geigerCounterNode;

        public static void Init() {
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "facilitymeltdown"));

            warnings = [
                LoadAsset < AudioClip >("warning1.mp3"),
                LoadAsset < AudioClip >("warning2.mp3"),
                LoadAsset < AudioClip >("warning3.mp3"),
                LoadAsset < AudioClip >("warning4.mp3")
            ];

            defaultMusic = LoadAsset<AudioClip>("meltdownMusic.mp3");
            shockwave = LoadAsset<AudioClip>("shockwave.mp3");

            facilityExplosionPrefab = LoadAsset<GameObject>("FacilityExplosion.prefab");
            meltdownHandlerPrefab = LoadAsset<GameObject>("MeltdownHandler.prefab");
            shockwavePrefab = LoadAsset<GameObject>("Shockwave.prefab");

            geigerCounterItem = LoadAsset<GameObject>("GeigerCounterItem.prefab");
            geigerCounterItemDef = LoadAsset<Item>("GeigerCounterItemDef.asset");
            geigerCounterNode = LoadAsset<TerminalNode>("GeigerCounterNode.asset");

            facilityEffects = [
                LoadAsset<GameObject>("Dust.prefab"),
                LoadAsset<GameObject>("Waterstream.prefab")
            ];
        }

        static T LoadAsset<T>(string path) where T : UnityEngine.Object {
            T result = assetBundle.LoadAsset<T>(path);
            if(result == null) throw new ArgumentException(path + " is not valid in the assetbundle!");
            return result;
        }
    }
}
