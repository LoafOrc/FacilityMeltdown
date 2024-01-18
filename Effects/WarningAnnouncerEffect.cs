using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.API;
using FacilityMeltdown.Util;
using UnityEngine;

namespace FacilityMeltdown.Effects {
    internal class WarningAnnouncerEffect : MeltdownSequenceEffect {
        public WarningAnnouncerEffect() : base(MeltdownPlugin.modGUID, "WarningAnnouncer") {}

        AudioSource warningAudioSource;

        public override void Setup() {
            base.Setup();
            warningAudioSource = gameObject.AddComponent<AudioSource>();
            warningAudioSource.loop = false;
            warningAudioSource.spatialBlend = 0;
        }

        public override IEnumerator Play(float timeLeftUntilMeltdown) {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            warningAudioSource.volume = MeltdownConfig.Default.CFG_MUSIC_VOLUME.Value;

            AudioClip sound = Assets.warnings[Random.Range(0, Assets.warnings.Length)];
            warningAudioSource.clip = sound;
            warningAudioSource.Play();

            yield return new WaitForSeconds(warningAudioSource.clip.length);

            float progress = GetMeltdownProgress(timeLeftUntilMeltdown);
            if (progress > .75) {
                yield return new WaitForSeconds(Random.Range(5f, 10f));
            } else if (progress > .5) {
                yield return new WaitForSeconds(Random.Range(4f, 8f));
            } else if (progress > .25) {
                yield return new WaitForSeconds(Random.Range(3f, 6f));
            } else {
                yield return new WaitForSeconds(Random.Range(2f, 4f));
            }
            yield break;
        }

        public override IEnumerator Stop() {
            warningAudioSource.Stop();

            yield return null;
            yield break;
        }
    }
}
