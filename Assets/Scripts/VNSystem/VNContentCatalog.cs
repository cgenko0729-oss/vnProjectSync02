using System;
using System.Collections.Generic;
using UnityEngine;

namespace VNSystem
{
    public enum VNCharacterSlot
    {
        FarLeft,
        Left,
        Center,
        Right,
        FarRight
    }

    public enum VNCameraMove
    {
        Reset,
        PushIn,
        SnapZoom,
        Pan,
        DollyZoom
    }

    [Serializable]
    public sealed class VNExpressionDefinition
    {
        public string id = "neutral";
        public Sprite sprite;
    }

    [Serializable]
    public sealed class VNCharacterDefinition
    {
        public string id;
        public string displayName;
        public VNCharacterSlot defaultSlot = VNCharacterSlot.Center;
        public Vector2 portraitSize = new Vector2(720f, 1080f);
        public string defaultExpression = "neutral";
        public List<VNExpressionDefinition> expressions = new List<VNExpressionDefinition>();

        public Sprite FindExpression(string expressionId)
        {
            string wanted = string.IsNullOrWhiteSpace(expressionId) ? defaultExpression : expressionId;
            foreach (var expression in expressions)
            {
                if (expression != null && string.Equals(expression.id, wanted, StringComparison.OrdinalIgnoreCase))
                    return expression.sprite;
            }

            foreach (var expression in expressions)
            {
                if (expression != null && string.Equals(expression.id, defaultExpression, StringComparison.OrdinalIgnoreCase))
                    return expression.sprite;
            }

            return expressions.Count > 0 && expressions[0] != null ? expressions[0].sprite : null;
        }
    }

    [Serializable]
    public sealed class VNBackgroundDefinition
    {
        public string id;
        public Sprite sprite;
        public Color tint = Color.white;
    }

    [Serializable]
    public sealed class VNCameraPreset
    {
        public string id;
        public VNCameraMove move = VNCameraMove.PushIn;
        public float duration = 1f;
        public float zoom = 1.06f;
        public Vector2 canvasTarget;
        [Range(0f, 1f)] public float centering = 0.6f;
    }

    [Serializable]
    public sealed class VNEffectPreset
    {
        public string id;
        public VNEffects.VNWeather weather = VNEffects.VNWeather.None;
        public float transitionDuration = 1f;
    }

    [CreateAssetMenu(fileName = "VNContentCatalog", menuName = "VN System/Content Catalog")]
    public sealed class VNContentCatalog : ScriptableObject
    {
        public List<VNCharacterDefinition> characters = new List<VNCharacterDefinition>();
        public List<VNBackgroundDefinition> backgrounds = new List<VNBackgroundDefinition>();
        public List<VNCameraPreset> cameraPresets = new List<VNCameraPreset>();
        public List<VNEffectPreset> effectPresets = new List<VNEffectPreset>();

        Dictionary<string, VNCharacterDefinition> _characterById;
        Dictionary<string, VNBackgroundDefinition> _backgroundById;
        Dictionary<string, VNCameraPreset> _cameraById;
        Dictionary<string, VNEffectPreset> _effectById;

        public VNCharacterDefinition FindCharacter(string id)
        {
            EnsureCache();
            return Find(_characterById, id);
        }

        public VNBackgroundDefinition FindBackground(string id)
        {
            EnsureCache();
            return Find(_backgroundById, id);
        }

        public VNCameraPreset FindCamera(string id)
        {
            EnsureCache();
            return Find(_cameraById, id);
        }

        public VNEffectPreset FindEffect(string id)
        {
            EnsureCache();
            return Find(_effectById, id);
        }

        public void RebuildCache()
        {
            _characterById = Build(characters, item => item != null ? item.id : null);
            _backgroundById = Build(backgrounds, item => item != null ? item.id : null);
            _cameraById = Build(cameraPresets, item => item != null ? item.id : null);
            _effectById = Build(effectPresets, item => item != null ? item.id : null);
        }

        void OnValidate()
        {
            RebuildCache();
        }

        void EnsureCache()
        {
            if (_characterById == null) RebuildCache();
        }

        static T Find<T>(Dictionary<string, T> dictionary, string id) where T : class
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            dictionary.TryGetValue(id.Trim(), out var value);
            return value;
        }

        static Dictionary<string, T> Build<T>(IEnumerable<T> items, Func<T, string> getId) where T : class
        {
            var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                string id = getId(item);
                if (item != null && !string.IsNullOrWhiteSpace(id)) result[id.Trim()] = item;
            }
            return result;
        }
    }
}
