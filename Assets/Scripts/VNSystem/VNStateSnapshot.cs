using System;
using System.Collections.Generic;

namespace VNSystem
{
    [Serializable]
    public sealed class VNCharacterState
    {
        public string characterId;
        public string expressionId;
        public VNCharacterSlot slot;
        public bool visible;
    }

    [Serializable]
    public sealed class VNStateSnapshot
    {
        public string backgroundId;
        public string cameraPresetId;
        public string weatherEffectId;
        public List<VNCharacterState> characters = new List<VNCharacterState>();
        public List<string> activeEffects = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            return UnityEngine.JsonUtility.ToJson(this, prettyPrint);
        }

        public static VNStateSnapshot FromJson(string json)
        {
            return string.IsNullOrWhiteSpace(json)
                ? new VNStateSnapshot()
                : UnityEngine.JsonUtility.FromJson<VNStateSnapshot>(json);
        }
    }
}
