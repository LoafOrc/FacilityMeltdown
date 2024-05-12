using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace FacilityMeltdown.Patches;

[HarmonyPatch(typeof(GrabbableObject))]
public class GrabbableObjectPatch {
	[HarmonyPrefix, HarmonyPatch(nameof(GrabbableObject.Update))]
	internal static void ApplyMinPeople(GrabbableObject __instance) {
		if(__instance is not LungProp apparatus) return;
		if (!apparatus.isLungDocked) {
			if (apparatus.grabbable) return;
			
			apparatus.customGrabTooltip = "";
			apparatus.grabbable = true;
			return;
		}

		int closePlayers = StartOfRound.Instance.GetConnectedPlayers().Count(player => Vector3.Distance(player.transform.position, apparatus.transform.position) < 15f);

		int actualNeededPlayers = Mathf.Min(MeltdownPlugin.config.MinPeopleToPullApparatus,
			StartOfRound.Instance.livingPlayers);
		
		if(closePlayers < actualNeededPlayers && TimeOfDay.Instance.GetCurrentTime().hours < 21) {
			apparatus.customGrabTooltip = $"[ NEEDS {actualNeededPlayers} PEOPLE ]";
			apparatus.grabbable = false;
		} else {
			apparatus.customGrabTooltip = "";
			apparatus.grabbable = true;
		}

            
	}
}