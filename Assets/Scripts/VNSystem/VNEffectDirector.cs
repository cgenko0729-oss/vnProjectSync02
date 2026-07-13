using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace VNSystem
{
    [Serializable]
    public sealed class VNEffectBinding
    {
        public string id;
        public UnityEvent onStart = new UnityEvent();
        public UnityEvent onStop = new UnityEvent();
    }

    public sealed class VNEffectDirector : MonoBehaviour
    {
        [SerializeField] List<VNEffectBinding> bindings = new List<VNEffectBinding>();
        readonly HashSet<string> _active = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> ActiveEffects => _active;

        public Tween SetEffect(string id, bool enabled, float waitDuration, bool instant)
        {
            var binding = bindings.Find(item => item != null &&
                string.Equals(item.id, id, StringComparison.OrdinalIgnoreCase));
            if (binding == null)
            {
                Debug.LogWarning($"[VN] Unknown runtime effect '{id}'.", this);
                return null;
            }

            if (enabled)
            {
                binding.onStart.Invoke();
                _active.Add(binding.id);
            }
            else
            {
                binding.onStop.Invoke();
                _active.Remove(binding.id);
            }

            return instant || waitDuration <= 0f
                ? null
                : DOTween.Sequence().AppendInterval(waitDuration).SetTarget(this).SetLink(gameObject);
        }

        public void Restore(IEnumerable<string> activeEffectIds)
        {
            var wanted = new HashSet<string>(activeEffectIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var binding in bindings)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.id)) continue;
                bool shouldBeActive = wanted.Contains(binding.id);
                bool isActive = _active.Contains(binding.id);
                if (shouldBeActive != isActive) SetEffect(binding.id, shouldBeActive, 0f, true);
            }
        }

        public List<string> Capture()
        {
            return new List<string>(_active);
        }

        void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}
