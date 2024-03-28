using FacilityMeltdown.API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FacilityMeltdown.Behaviours;
public class MeltdownMoonMapper : MonoBehaviour {
    public static MeltdownMoonMapper Instance { get; private set; }

    [field: SerializeField]
    [Tooltip("Lights to flash red during the meltdown sequence. Doesn't include inside lights.")]
    public List<Light> outsideEmergencyLights = new List<Light>();

    [field: SerializeField]
    [Tooltip("Colour to flash lights with, should probably just leave this at read.")]
    public Color emergencyLightColour { get; private set; } = Color.red;

    [field: SerializeField]
    public GameObject shockwavePrefab { get; private set; }
    [field: SerializeField]
    public GameObject explosionPrefab { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void OnDisable() {
        if(Instance != null)
            Instance = null;
    }

    private void Update() {
        if(Keyboard.current.rightBracketKey.wasPressedThisFrame) {
            MeltdownAPI.StartMeltdown("DEBUG KEY PRESSED");
        }
    }
}
