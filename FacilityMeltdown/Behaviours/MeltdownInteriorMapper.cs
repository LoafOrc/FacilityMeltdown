using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FacilityMeltdown.Behaviours;
public class MeltdownInteriorMapper : MonoBehaviour {
    public static MeltdownInteriorMapper Instance { get; private set; }

    [field: SerializeField]
    [Tooltip("Colour to flash lights with, should probably just leave this at red. Only applies to inside lights.")]
    public Color outsideEmergencyLightColour { get; private set; } = Color.red;

    private void Awake() {
        Instance = this;
    }

    private void OnDisable() {
        if(Instance != null)
            Instance = null;
    }

    internal static void EnsureMeltdownInteriorMapper() {
        if(Instance != null) return;
        if(GameObject.FindObjectOfType<MeltdownInteriorMapper>() != null) return; // skipping as the moon has its own override

        MeltdownPlugin.logger.LogInfo("Creating InteriorMapper!");
        Instance = new GameObject("DefaultMeltdownInteriorMappings").AddComponent<MeltdownInteriorMapper>();
    }
}
