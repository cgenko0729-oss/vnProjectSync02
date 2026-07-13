using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VNEffects;

namespace VNSystem
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class VNCharacterView : MonoBehaviour
    {
        [SerializeField] Image portrait;
        [SerializeField] VNEntranceAnimator entranceAnimator;

        CanvasGroup _group;
        RectTransform _rect;

        public string CharacterId { get; private set; }
        public string ExpressionId { get; private set; }
        public VNCharacterSlot Slot { get; private set; }
        public bool IsVisible => _group != null && _group.alpha > 0.001f && gameObject.activeSelf;

        public RectTransform Rect => _rect != null ? _rect : (_rect = (RectTransform)transform);

        void Awake()
        {
            EnsureComponents();
        }

        public void EnsureComponents()
        {
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            if (portrait == null) portrait = GetComponent<Image>();
            if (portrait == null) portrait = gameObject.AddComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            if (entranceAnimator == null) entranceAnimator = GetComponent<VNEntranceAnimator>();
        }

        public Tween Show(VNCharacterDefinition definition, string expressionId, VNCharacterSlot slot,
            VNEntrancePreset entrance, float duration, bool instant)
        {
            EnsureComponents();
            DOTween.Kill(this);
            CharacterId = definition.id;
            ExpressionId = string.IsNullOrWhiteSpace(expressionId) ? definition.defaultExpression : expressionId;
            Slot = slot;
            portrait.sprite = definition.FindExpression(ExpressionId);
            Rect.sizeDelta = definition.portraitSize;
            gameObject.SetActive(true);

            if (instant)
            {
                _group.alpha = 1f;
                Rect.localScale = Vector3.one;
                return null;
            }

            if (entranceAnimator != null)
            {
                entranceAnimator.PrepareHidden();
                return entranceAnimator.PlayEntrance(entrance, Mathf.Max(0.01f, duration));
            }

            _group.alpha = 0f;
            Vector2 target = Rect.anchoredPosition;
            Rect.anchoredPosition = target + new Vector2(0f, -40f);
            return DOTween.Sequence()
                .Append(_group.DOFade(1f, duration).SetEase(Ease.OutQuad))
                .Join(Rect.DOAnchorPos(target, duration).SetEase(Ease.OutCubic))
                .SetTarget(this).SetLink(gameObject);
        }

        public Tween SetExpression(VNCharacterDefinition definition, string expressionId, float duration, bool instant)
        {
            EnsureComponents();
            Sprite next = definition.FindExpression(expressionId);
            if (next == null)
            {
                Debug.LogWarning($"[VN] Character '{definition.id}' has no expression '{expressionId}'.", this);
                return null;
            }

            ExpressionId = expressionId;
            DOTween.Kill(this);
            if (instant || duration <= 0f)
            {
                portrait.sprite = next;
                _group.alpha = 1f;
                return null;
            }

            float half = duration * 0.5f;
            return DOTween.Sequence()
                .Append(_group.DOFade(0f, half))
                .AppendCallback(() => portrait.sprite = next)
                .Append(_group.DOFade(1f, half))
                .SetTarget(this).SetLink(gameObject);
        }

        public Tween MoveTo(VNCharacterSlot slot, Vector2 position, float duration, bool instant)
        {
            EnsureComponents();
            Slot = slot;
            if (instant || duration <= 0f)
            {
                Rect.anchoredPosition = position;
                return null;
            }
            return Rect.DOAnchorPos(position, duration).SetEase(Ease.InOutSine)
                .SetTarget(this).SetLink(gameObject);
        }

        public Tween Hide(float duration, bool instant)
        {
            EnsureComponents();
            DOTween.Kill(this);
            if (instant || duration <= 0f)
            {
                _group.alpha = 0f;
                gameObject.SetActive(false);
                return null;
            }

            if (entranceAnimator != null)
                return entranceAnimator.PlayExitFade(duration).OnComplete(() => gameObject.SetActive(false));

            return _group.DOFade(0f, duration).SetEase(Ease.InQuad)
                .OnComplete(() => gameObject.SetActive(false))
                .SetTarget(this).SetLink(gameObject);
        }

        public VNCharacterState CaptureState()
        {
            return new VNCharacterState
            {
                characterId = CharacterId,
                expressionId = ExpressionId,
                slot = Slot,
                visible = IsVisible
            };
        }

        void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}
