using DG.Tweening;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using VNEffects;
using VNSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    public abstract class VNSequencerCommandBase : SequencerCommand
    {
        Tween _tween;

        protected VNDirector Director
        {
            get
            {
                var director = VNDirector.Instance;
                if (director == null) director = FindFirstObjectByType<VNDirector>();
                return director;
            }
        }

        protected void Track(Tween tween)
        {
            _tween = tween;
            if (_tween == null) Stop();
        }

        protected void Fail(string message)
        {
            Debug.LogError($"[VN Sequencer] {message}", this);
            Stop();
        }

        public void Update()
        {
            if (_tween == null) return;
            if (!_tween.IsActive() || _tween.IsComplete())
            {
                _tween = null;
                Stop();
            }
        }

        public void OnDestroy()
        {
            if (_tween != null && _tween.IsActive() && !_tween.IsComplete()) _tween.Kill(false);
            _tween = null;
        }
    }

    /// <summary>VNBG(backgroundId, crossfade|cut, duration, instant)</summary>
    public sealed class SequencerCommandVNBG : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            string id = GetParameter(0);
            string transition = GetParameter(1, "crossfade");
            float duration = GetParameterAsFloat(2, 0.8f);
            bool instant = GetParameterAsBool(3, false) || transition.Equals("cut", System.StringComparison.OrdinalIgnoreCase);
            Track(Director.SetBackground(id, duration, instant));
        }
    }

    /// <summary>VNChar(characterId, slot, expression, entrance, duration, instant)</summary>
    public sealed class SequencerCommandVNChar : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            string characterId = GetParameter(0);
            if (!VNDirector.TryParseEnum(GetParameter(1, "Center"), out VNCharacterSlot slot)) slot = VNCharacterSlot.Center;
            string expression = GetParameter(2, "neutral");
            if (!VNDirector.TryParseEnum(GetParameter(3, "FadeSlideUp"), out VNEntrancePreset entrance))
                entrance = VNEntrancePreset.FadeSlideUp;
            float duration = GetParameterAsFloat(4, 0.5f);
            bool instant = GetParameterAsBool(5, false);
            Track(Director.ShowCharacter(characterId, slot, expression, entrance, duration, instant));
        }
    }

    /// <summary>VNFace(characterId, expression, duration, instant)</summary>
    public sealed class SequencerCommandVNFace : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            Track(Director.SetExpression(GetParameter(0), GetParameter(1, "neutral"),
                GetParameterAsFloat(2, 0.2f), GetParameterAsBool(3, false)));
        }
    }

    /// <summary>VNMove(characterId, slot, duration, instant)</summary>
    public sealed class SequencerCommandVNMove : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            if (!VNDirector.TryParseEnum(GetParameter(1, "Center"), out VNCharacterSlot slot)) slot = VNCharacterSlot.Center;
            Track(Director.MoveCharacter(GetParameter(0), slot,
                GetParameterAsFloat(2, 0.5f), GetParameterAsBool(3, false)));
        }
    }

    /// <summary>VNHide(characterId, duration, instant)</summary>
    public sealed class SequencerCommandVNHide : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            Track(Director.HideCharacter(GetParameter(0),
                GetParameterAsFloat(1, 0.4f), GetParameterAsBool(2, false)));
        }
    }

    /// <summary>VNCamera(presetId, durationOverride, instant)</summary>
    public sealed class SequencerCommandVNCamera : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            Track(Director.SetCamera(GetParameter(0),
                GetParameterAsFloat(1, -1f), GetParameterAsBool(2, false)));
        }
    }

    /// <summary>VNWeather(effectPresetId, durationOverride, instant)</summary>
    public sealed class SequencerCommandVNWeather : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            Track(Director.SetWeather(GetParameter(0),
                GetParameterAsFloat(1, -1f), GetParameterAsBool(2, false)));
        }
    }

    /// <summary>VNEffect(effectId, start|stop, waitDuration, instant)</summary>
    public sealed class SequencerCommandVNEffect : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            string action = GetParameter(1, "start");
            bool enabled = !action.Equals("stop", System.StringComparison.OrdinalIgnoreCase) &&
                           !action.Equals("off", System.StringComparison.OrdinalIgnoreCase);
            Track(Director.SetEffect(GetParameter(0), enabled,
                GetParameterAsFloat(2, 0f), GetParameterAsBool(3, false)));
        }
    }

    /// <summary>VNWait(duration)</summary>
    public sealed class SequencerCommandVNWait : VNSequencerCommandBase
    {
        public void Awake()
        {
            if (Director == null) { Fail("No VNDirector exists in the scene."); return; }
            Track(Director.Wait(GetParameterAsFloat(0, 0f)));
        }
    }
}
