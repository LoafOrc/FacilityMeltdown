using FacilityMeltdown.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FacilityMeltdown.Effects {
    internal class IntroDialogueSequencer : MeltdownSequenceEffect {
        public IntroDialogueSequencer() : base(MeltdownPlugin.modGUID, "IntroDialogueSequencer") { IsOneShot = true; }

        public override IEnumerator Play(float timeLeftUntilMeltdown) {

            yield return new WaitForSeconds(5f);

            HUDManager.Instance.ReadDialogue(MeltdownHandler.GetDialogue("meltdown.dialogue.start"));

            yield break;
        }
    }
}
