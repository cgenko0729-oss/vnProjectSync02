using System;
using DG.Tweening;
using UnityEngine;
using VNEffects;

namespace VNSystem
{
    [DefaultExecutionOrder(-100)]
    public sealed class VNDirector : MonoBehaviour
    {
        public static VNDirector Instance { get; private set; }

        [Header("Data")]
        [SerializeField] VNContentCatalog catalog;

        [Header("Runtime")]
        [SerializeField] VNStageController stage;
        [SerializeField] VNCamera cameraDirector;
        [SerializeField] VNWeatherController weatherDirector;
        [SerializeField] VNScreenTransition screenTransition;
        [SerializeField] VNEffectDirector effectDirector;

        string _currentCameraPresetId;
        string _currentWeatherEffectId;

        public VNContentCatalog Catalog => catalog;
        public VNStageController Stage => stage;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[VN] More than one VNDirector exists in the scene.", this);
                return;
            }
            Instance = this;
            ResolveReferences();
        }

        public void Configure(VNContentCatalog contentCatalog, VNStageController stageController,
            VNCamera vnCamera = null, VNWeatherController weather = null,
            VNScreenTransition transition = null, VNEffectDirector effects = null)
        {
            catalog = contentCatalog;
            stage = stageController;
            cameraDirector = vnCamera;
            weatherDirector = weather;
            screenTransition = transition;
            effectDirector = effects;
            ResolveReferences();
        }

        public Tween SetBackground(string backgroundId, float duration = 0.8f, bool instant = false)
        {
            if (!Ready()) return null;
            var definition = catalog.FindBackground(backgroundId);
            if (definition == null)
            {
                Missing("background", backgroundId);
                return null;
            }
            return stage.SetBackground(definition, duration, instant);
        }

        public Tween ShowCharacter(string characterId, VNCharacterSlot slot, string expressionId = null,
            VNEntrancePreset entrance = VNEntrancePreset.FadeSlideUp, float duration = 0.5f, bool instant = false)
        {
            if (!Ready()) return null;
            var definition = catalog.FindCharacter(characterId);
            if (definition == null)
            {
                Missing("character", characterId);
                return null;
            }
            return stage.ShowCharacter(definition, slot, expressionId, entrance, duration, instant);
        }

        public Tween SetExpression(string characterId, string expressionId, float duration = 0.2f, bool instant = false)
        {
            if (!Ready()) return null;
            var definition = catalog.FindCharacter(characterId);
            if (definition == null)
            {
                Missing("character", characterId);
                return null;
            }
            return stage.SetExpression(definition, expressionId, duration, instant);
        }

        public Tween MoveCharacter(string characterId, VNCharacterSlot slot, float duration = 0.5f, bool instant = false)
        {
            return Ready() ? stage.MoveCharacter(characterId, slot, duration, instant) : null;
        }

        public Tween HideCharacter(string characterId, float duration = 0.4f, bool instant = false)
        {
            return Ready() ? stage.HideCharacter(characterId, duration, instant) : null;
        }

        public Tween SetCamera(string presetId, float durationOverride = -1f, bool instant = false)
        {
            if (!Ready()) return null;
            var preset = catalog.FindCamera(presetId);
            if (preset == null)
            {
                Missing("camera preset", presetId);
                return null;
            }
            if (cameraDirector == null)
            {
                Debug.LogWarning("[VN] No VNCamera is assigned.", this);
                return null;
            }

            float duration = durationOverride >= 0f ? durationOverride : preset.duration;
            if (instant) duration = 0f;
            Tween tween;
            switch (preset.move)
            {
                case VNCameraMove.Reset:
                    tween = cameraDirector.ResetCamera(duration);
                    break;
                case VNCameraMove.SnapZoom:
                    tween = cameraDirector.SnapZoom(preset.zoom, duration, preset.canvasTarget);
                    break;
                case VNCameraMove.Pan:
                    tween = cameraDirector.Pan(preset.canvasTarget, preset.centering, duration);
                    break;
                case VNCameraMove.DollyZoom:
                    tween = cameraDirector.DollyZoom(preset.zoom, duration);
                    break;
                default:
                    tween = cameraDirector.PushIn(preset.zoom, duration, preset.canvasTarget);
                    break;
            }
            _currentCameraPresetId = preset.id;
            if (instant && tween != null && tween.IsActive()) tween.Complete(true);
            return instant ? null : tween;
        }

        public Tween SetWeather(string effectId, float durationOverride = -1f, bool instant = false)
        {
            if (!Ready()) return null;
            var preset = catalog.FindEffect(effectId);
            if (preset == null)
            {
                Missing("weather effect", effectId);
                return null;
            }
            if (weatherDirector == null)
            {
                Debug.LogWarning("[VN] No VNWeatherController is assigned.", this);
                return null;
            }

            float duration = durationOverride >= 0f ? durationOverride : preset.transitionDuration;
            if (instant) duration = 0f;
            weatherDirector.SetWeather(preset.weather, duration);
            _currentWeatherEffectId = preset.id;
            return instant || duration <= 0f
                ? null
                : DOTween.Sequence().AppendInterval(duration).SetTarget(this).SetLink(gameObject);
        }

        public Tween SetEffect(string effectId, bool enabled, float waitDuration = 0f, bool instant = false)
        {
            if (effectDirector == null)
            {
                Debug.LogWarning("[VN] No VNEffectDirector is assigned.", this);
                return null;
            }
            return effectDirector.SetEffect(effectId, enabled, waitDuration, instant);
        }

        public Tween Wait(float duration)
        {
            return duration <= 0f ? null : DOTween.Sequence().AppendInterval(duration).SetTarget(this).SetLink(gameObject);
        }

        public VNStateSnapshot CaptureSnapshot()
        {
            ResolveReferences();
            return new VNStateSnapshot
            {
                backgroundId = stage != null ? stage.BackgroundId : null,
                cameraPresetId = _currentCameraPresetId,
                weatherEffectId = _currentWeatherEffectId,
                characters = stage != null ? stage.CaptureCharacters() : new System.Collections.Generic.List<VNCharacterState>(),
                activeEffects = effectDirector != null ? effectDirector.Capture() : new System.Collections.Generic.List<string>()
            };
        }

        public void RestoreSnapshot(VNStateSnapshot snapshot)
        {
            if (snapshot == null || !Ready()) return;
            stage.HideAllImmediate();
            if (!string.IsNullOrWhiteSpace(snapshot.backgroundId)) SetBackground(snapshot.backgroundId, 0f, true);
            foreach (var state in snapshot.characters)
            {
                if (state != null && state.visible)
                    ShowCharacter(state.characterId, state.slot, state.expressionId, VNEntrancePreset.FadeSlideUp, 0f, true);
            }
            if (!string.IsNullOrWhiteSpace(snapshot.cameraPresetId)) SetCamera(snapshot.cameraPresetId, 0f, true);
            if (!string.IsNullOrWhiteSpace(snapshot.weatherEffectId)) SetWeather(snapshot.weatherEffectId, 0f, true);
            if (effectDirector != null) effectDirector.Restore(snapshot.activeEffects);
        }

        public static bool TryParseEnum<T>(string value, out T result) where T : struct
        {
            return Enum.TryParse(value, true, out result);
        }

        void ResolveReferences()
        {
            if (stage == null) stage = GetComponentInChildren<VNStageController>(true);
            if (cameraDirector == null) cameraDirector = GetComponentInChildren<VNCamera>(true);
            if (weatherDirector == null) weatherDirector = GetComponentInChildren<VNWeatherController>(true);
            if (screenTransition == null) screenTransition = GetComponentInChildren<VNScreenTransition>(true);
            if (effectDirector == null) effectDirector = GetComponentInChildren<VNEffectDirector>(true);
            if (stage != null) stage.EnsureRuntimeObjects();
        }

        bool Ready()
        {
            ResolveReferences();
            if (catalog != null && stage != null) return true;
            Debug.LogError("[VN] VNDirector requires a Content Catalog and Stage Controller.", this);
            return false;
        }

        void Missing(string kind, string id)
        {
            Debug.LogError($"[VN] Unknown {kind} id '{id}'.", this);
        }

        void OnDestroy()
        {
            DOTween.Kill(this);
            if (Instance == this) Instance = null;
        }
    }
}
