using System.Collections;
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
            warningAudioSource.volume = MeltdownConfig.Default.CFG_MUSIC_VOLUME.Value;

            AudioClip sound = Assets.warnings[Random.Range(0, Assets.warnings.Length)];
            warningAudioSource.clip = sound;
            warningAudioSource.Play();

            yield return new WaitForSeconds(warningAudioSource.clip.length);

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }

        public override IEnumerator Stop() {
            warningAudioSource.Stop();

            yield return null;
            yield break;
        }
    }
}
