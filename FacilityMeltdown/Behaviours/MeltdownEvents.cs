using FacilityMeltdown.API;
using FacilityMeltdown.MeltdownSequence.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace FacilityMeltdown.Behaviours;
public class MeltdownEvents : MonoBehaviour {
    [SerializeField]
    UnityEvent OnMeltdownStart;

    [System.Serializable]
    public class Marker {
        [field: SerializeField]
        public float Progress { get; private set; } = .5f;
        [field: SerializeField]
        public UnityEvent OnMarker { get; private set; }

        internal bool Triggered = false;    
    }

    [SerializeField]
    List<Marker> markers = new List<Marker>();
    
    void OnEnable() {
        MeltdownAPI.OnMeltdownStart += OnMeltdownStart.Invoke;
    }
    void OnDisable() {
        MeltdownAPI.OnMeltdownStart -= OnMeltdownStart.Invoke;
    }

    void Update() {
        if(!MeltdownAPI.MeltdownStarted) return;

        foreach(Marker marker in markers) {
            if(marker.Triggered) continue;
            if(marker.Progress > MeltdownHandler.Instance.Progress) continue;

            marker.OnMarker.Invoke();
            marker.Triggered = true;
        }
    }
}
