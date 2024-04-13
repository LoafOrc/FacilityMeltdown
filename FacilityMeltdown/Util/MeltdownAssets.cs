using Biodiversity.Util.Assetloading;
using FacilityMeltdown.Patches;
using FacilityMeltdown.Util;
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
    internal class MeltdownAssets() : AssetBundleLoader<MeltdownAssets>("facilitymeltdown") {


        [LoadFromBundle("meltdownmusic.mp3")]
        public AudioClip defaultMusic { get; private set; }

        [LoadFromBundle("shockwave.mp3")]
        public AudioClip shockwave { get; private set; }

        [LoadFromBundle("FacilityExplosion.prefab")]
        public GameObject facilityExplosionPrefab { get; private set; }

        [LoadFromBundle("MeltdownHandler.prefab")]
        public GameObject meltdownHandlerPrefab { get; private set; }

        [LoadFromBundle("Shockwave.prefab")]
        public GameObject shockwavePrefab { get; private set; }

        [LoadFromBundle("GeigerCounterItemDef.asset")]
        public Item geigerCounterItemDef { get; private set; }

        [LoadFromBundle("GeigerCounterNode.asset")]
        public TerminalNode geigerCounterNode { get; private set; }

        [LoadFromBundle("HealthKeyword.asset")]
        public TerminalKeyword healthKeyword { get; private set; }

        [LoadFromBundle("ReactorKeyword.asset")]
        public TerminalKeyword reactorKeyword { get; private set; }

        [LoadFromBundle("ReactorHealthReport.asset")]
        public TerminalNode reactorHealthNode { get; private set; }

        public GameObject[] facilityEffects { get; private set; }
        public AudioClip[] warnings { get; private set; }

        // hacky solution cause i couldnt care less

        protected override void FinishLoadingAssets(AssetBundle bundle) {

            warnings = [
                LoadAsset < AudioClip >(bundle, "warning1.mp3"),
                LoadAsset < AudioClip >(bundle, "warning2.mp3"),
                LoadAsset < AudioClip >(bundle, "warning3.mp3"),
                LoadAsset < AudioClip >(bundle, "warning4.mp3")
            ];

            facilityEffects = [
                LoadAsset<GameObject>(bundle, "Dust.prefab"),
                LoadAsset<GameObject>(bundle, "Waterstream.prefab")
            ];

            TerminalPatch.terminalKeywordsToRegister.Add(healthKeyword);
            TerminalPatch.terminalKeywordsToRegister.Add(reactorKeyword);
            TerminalPatch.terminalNodesToRegister.Add(reactorHealthNode);
        }
    }
}
