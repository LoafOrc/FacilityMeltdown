using FacilityMeltdown.Behaviours;
using UnityEngine;

namespace FacilityMeltdown.Equipment
{
    public class GeigerCounterItem : GrabbableObject
    {
        [Space(15)]

        [Header("Audio")]
        public AudioSource generalAudioSource;
        public AudioSource lowRadiation, mediumRadiation, highRadiation;

        public AudioClip toggle, outOfBattery;

        public GameObject needle;
        public float maxRotation = 25;
        public float maxDetection = 50;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            MeltdownPlugin.logger.LogInfo("ACTIVATED GEIGER COUNTER");
            SwitchPoweredState(used);

            generalAudioSource.clip = toggle;
            generalAudioSource.Play();
        }

        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            SwitchPoweredState(false);

            generalAudioSource.clip = outOfBattery;
            generalAudioSource.Play();
        }

        public void SwitchPoweredState(bool on)
        {
            MeltdownPlugin.logger.LogInfo($"me when the on value is {on}. :rofl::rofl::rofl:");

            if (isBeingUsed)
            {
                lowRadiation.Play();
                mediumRadiation.Play();
                highRadiation.Play();
            }
            else
            {
                lowRadiation.Stop();
                mediumRadiation.Stop();
                highRadiation.Stop();
            }
        }

        public override void Update()
        {
            base.Update();

            if (isBeingUsed)
            {
                float radiation = RadiationSource.CollectRadiationFromPoint(transform.position);

                lowRadiation.volume = 0; mediumRadiation.volume = 0; highRadiation.volume = 0;

                if (radiation > maxDetection)
                {
                    highRadiation.volume = 1;
                }
                else if (radiation > maxDetection / 2)
                {
                    mediumRadiation.volume = 1;
                }
                else
                {
                    lowRadiation.volume = 1;
                }

                float needleTurnAmount = Mathf.Clamp01(radiation / maxDetection) * (maxRotation * 2);
                needleTurnAmount -= maxRotation;

                needle.transform.localEulerAngles = new Vector3(-90, needleTurnAmount, 0);
            }
        }
    }
}
