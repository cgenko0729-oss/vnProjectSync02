using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VNEffects;

namespace VNSystem
{
    [Serializable]
    public sealed class VNSlotPosition
    {
        public VNCharacterSlot slot;
        public Vector2 anchoredPosition;
    }

    public sealed class VNStageController : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] RectTransform backgroundRoot;
        [SerializeField] Image backgroundA;
        [SerializeField] Image backgroundB;

        [Header("Characters")]
        [SerializeField] RectTransform characterRoot;
        [SerializeField, Min(1)] int characterPoolSize = 5;
        [SerializeField] List<VNSlotPosition> slotPositions = new List<VNSlotPosition>();

        readonly List<VNCharacterView> _characters = new List<VNCharacterView>();
        int _activeBackground;

        public string BackgroundId { get; private set; }
        public IReadOnlyList<VNCharacterView> Characters => _characters;

        void Awake()
        {
            EnsureRuntimeObjects();
        }

        public void EnsureRuntimeObjects()
        {
            EnsureDefaultSlots();
            if (backgroundRoot == null) backgroundRoot = CreateRect("BackgroundRoot", transform);
            if (backgroundA == null) backgroundA = CreateImage("BackgroundA", backgroundRoot, true);
            if (backgroundB == null) backgroundB = CreateImage("BackgroundB", backgroundRoot, true);
            if (characterRoot == null) characterRoot = CreateRect("CharacterRoot", transform);

            _characters.Clear();
            characterRoot.GetComponentsInChildren(true, _characters);
            while (_characters.Count < characterPoolSize)
            {
                var rect = CreateRect($"Character{_characters.Count + 1}", characterRoot);
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                var view = rect.gameObject.AddComponent<VNCharacterView>();
                view.EnsureComponents();
                rect.gameObject.SetActive(false);
                _characters.Add(view);
            }
        }

        public Tween SetBackground(VNBackgroundDefinition definition, float duration, bool instant)
        {
            EnsureRuntimeObjects();
            if (definition == null || definition.sprite == null) return null;
            DOTween.Kill(this);
            Image current = _activeBackground == 0 ? backgroundA : backgroundB;
            Image next = _activeBackground == 0 ? backgroundB : backgroundA;
            next.sprite = definition.sprite;
            next.color = new Color(definition.tint.r, definition.tint.g, definition.tint.b, instant ? definition.tint.a : 0f);
            next.enabled = true;
            next.transform.SetAsLastSibling();
            BackgroundId = definition.id;

            if (instant || duration <= 0f)
            {
                current.enabled = false;
                next.color = definition.tint;
                _activeBackground = 1 - _activeBackground;
                return null;
            }

            var sequence = DOTween.Sequence()
                .Append(next.DOFade(definition.tint.a, duration))
                .Join(current.DOFade(0f, duration))
                .OnComplete(() => current.enabled = false)
                .SetTarget(this).SetLink(gameObject);
            _activeBackground = 1 - _activeBackground;
            return sequence;
        }

        public Tween ShowCharacter(VNCharacterDefinition definition, VNCharacterSlot slot, string expressionId,
            VNEntrancePreset entrance, float duration, bool instant)
        {
            EnsureRuntimeObjects();
            var view = FindCharacter(definition.id) ?? FindFreeCharacter();
            if (view == null)
            {
                Debug.LogError("[VN] Character pool is full.", this);
                return null;
            }
            view.Rect.anchoredPosition = PositionFor(slot);
            return view.Show(definition, expressionId, slot, entrance, duration, instant);
        }

        public Tween SetExpression(VNCharacterDefinition definition, string expressionId, float duration, bool instant)
        {
            var view = FindCharacter(definition.id);
            if (view == null)
            {
                Debug.LogWarning($"[VN] Cannot change expression: character '{definition.id}' is not visible.", this);
                return null;
            }
            return view.SetExpression(definition, expressionId, duration, instant);
        }

        public Tween MoveCharacter(string characterId, VNCharacterSlot slot, float duration, bool instant)
        {
            var view = FindCharacter(characterId);
            return view != null ? view.MoveTo(slot, PositionFor(slot), duration, instant) : null;
        }

        public Tween HideCharacter(string characterId, float duration, bool instant)
        {
            var view = FindCharacter(characterId);
            return view != null ? view.Hide(duration, instant) : null;
        }

        public VNCharacterView FindCharacter(string characterId)
        {
            return _characters.Find(view => view != null && view.gameObject.activeSelf &&
                string.Equals(view.CharacterId, characterId, StringComparison.OrdinalIgnoreCase));
        }

        public List<VNCharacterState> CaptureCharacters()
        {
            var result = new List<VNCharacterState>();
            foreach (var view in _characters)
                if (view != null && view.IsVisible) result.Add(view.CaptureState());
            return result;
        }

        public void HideAllImmediate()
        {
            foreach (var view in _characters)
                if (view != null && view.IsVisible) view.Hide(0f, true);
        }

        VNCharacterView FindFreeCharacter()
        {
            return _characters.Find(view => view != null && !view.IsVisible);
        }

        Vector2 PositionFor(VNCharacterSlot slot)
        {
            var value = slotPositions.Find(item => item != null && item.slot == slot);
            return value != null ? value.anchoredPosition : Vector2.zero;
        }

        void EnsureDefaultSlots()
        {
            if (slotPositions.Count > 0) return;
            slotPositions.Add(new VNSlotPosition { slot = VNCharacterSlot.FarLeft, anchoredPosition = new Vector2(-720f, 0f) });
            slotPositions.Add(new VNSlotPosition { slot = VNCharacterSlot.Left, anchoredPosition = new Vector2(-360f, 0f) });
            slotPositions.Add(new VNSlotPosition { slot = VNCharacterSlot.Center, anchoredPosition = Vector2.zero });
            slotPositions.Add(new VNSlotPosition { slot = VNCharacterSlot.Right, anchoredPosition = new Vector2(360f, 0f) });
            slotPositions.Add(new VNSlotPosition { slot = VNCharacterSlot.FarRight, anchoredPosition = new Vector2(720f, 0f) });
        }

        static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        static Image CreateImage(string name, Transform parent, bool stretch)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.enabled = false;
            if (!stretch) rect.sizeDelta = new Vector2(720f, 1080f);
            return image;
        }

        void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}
