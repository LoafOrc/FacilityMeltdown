using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace FacilityMeltdown.Util {
    public class UnlitModeHandler : MonoBehaviour {
        private readonly List<LightData> _lights = new List<LightData>();
        private readonly List<CameraData> _postProcessingCameras = new List<CameraData>();

        private GameObject _unlitRig = null;
        private HDAdditionalLightData[] _unlitRigLights = null;

        private void Awake() {
            SpawnRig();
        }

        private void Update() {
            var keyboard = Keyboard.current;
            // numpad 0 to toggle
            // numpad 4 to lower intensity
            // numpad 5 to raise intensity
            // only affects things in the scene, so if you load new things, or a new camera is focused, you have to re-toggle
            if (keyboard.rightShiftKey.wasPressedThisFrame) {
                if (!_unlitRig.activeSelf) {
                    DisableVolumes();
                    _unlitRig.SetActive(true);
                } else {
                    _unlitRig.SetActive(false);
                    EnableVolumes();
                }
            } else if (keyboard.rightBracketKey.wasPressedThisFrame) {
                foreach (var light in _unlitRigLights) {
                    light.intensity = Mathf.Clamp(light.intensity - 1, 0, 1000);
                }
            } else if (keyboard.leftBracketKey.wasPressedThisFrame) {
                foreach (var light in _unlitRigLights) {
                    light.intensity = Mathf.Clamp(light.intensity + 1, 0, 1000);
                }
            }
        }

        private void SpawnRig() {
            var rig = new GameObject("UnlitRig");
            rig.transform.SetParent(transform);
            var directions = new[] {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
        };
            for (int i = 0; i < 6; i++) {
                var light = new GameObject($"Light{i}");
                light.transform.SetParent(rig.transform);
                light.transform.rotation = Quaternion.LookRotation(directions[i]);

                var lightComponent = light.AddComponent<Light>();
                lightComponent.type = LightType.Directional;

                var lightData = light.AddComponent<HDAdditionalLightData>();
                lightData.lightUnit = LightUnit.Lux;
                lightData.intensity = 2;
                lightData.affectsVolumetric = false;
                lightData.angularDiameter = 90;
            }

            _unlitRigLights = rig.GetComponentsInChildren<HDAdditionalLightData>();
            _unlitRig = rig;
            _unlitRig.SetActive(false);
        }

        private void EnableVolumes() {
            foreach (var light in _lights) {
                if (!light.light) continue;
                light.light.enabled = true;
                light.light.intensity = light.intensity;
                light.light.affectsVolumetric = light.affectsVolumetric;
            }

            _lights.Clear();

            foreach (var camera in _postProcessingCameras) {
                if (!camera.camera) continue;
                camera.camera.volumeLayerMask = camera.volumeLayerMask;
                if (!camera.hadHDData) {
                    Destroy(camera.camera);
                }
            }

            _postProcessingCameras.Clear();
        }

        private void DisableVolumes() {
            for (int i = 0; i < SceneManager.loadedSceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                foreach (var root in scene.GetRootGameObjects()) {
                    foreach (var light in root.GetComponentsInChildren<HDAdditionalLightData>()) {
                        if (!light.enabled) continue;
                        light.enabled = false;
                        _lights.Add(new LightData {
                            light = light,
                            intensity = light.intensity,
                            affectsVolumetric = light.affectsVolumetric
                        });
                        light.intensity = 0;
                        light.affectsVolumetric = false;
                    }

                    foreach (var camera in root.GetComponentsInChildren<Camera>()) {
                        disableCamera(camera);
                    }
                }
            }

            if (Camera.main) {
                disableCamera(Camera.main);
            }

            void disableCamera(Camera camera) {
                if (!camera) return;

                var hadHDData = true;
                if (!camera.TryGetComponent(out HDAdditionalCameraData data)) {
                    data = camera.gameObject.AddComponent<HDAdditionalCameraData>();
                    hadHDData = false;
                }

                if (!data.enabled) return;
                _postProcessingCameras.Add(new CameraData {
                    camera = data,
                    volumeLayerMask = data.volumeLayerMask,
                    hadHDData = hadHDData
                });
                data.volumeLayerMask = 0;
            }
        }

        private struct LightData {
            public HDAdditionalLightData light;
            public float intensity;
            public bool affectsVolumetric;
        }

        private struct CameraData {
            public HDAdditionalCameraData camera;
            public LayerMask volumeLayerMask;
            public bool hadHDData;
        }
    }
}
