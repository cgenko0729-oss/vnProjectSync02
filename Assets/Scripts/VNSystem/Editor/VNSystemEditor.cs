using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;
using VNEffects;

namespace VNSystem.Editor
{
    public static class VNSystemSetup
    {
        const string DataFolder = "Assets/VNSystem/Data";
        const string CatalogPath = DataFolder + "/VNContentCatalog.asset";

        [MenuItem("Tools/VN System/Create or Repair Runtime Rig")]
        public static void CreateRuntimeRig()
        {
            var catalog = FindOrCreateCatalog();
            var director = UnityEngine.Object.FindFirstObjectByType<VNDirector>();
            if (director != null)
            {
                Selection.activeObject = director;
                Debug.Log("[VN] Existing VNDirector selected. No duplicate rig was created.", director);
                return;
            }

            var root = new GameObject("VNRoot", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(root, "Create VN Runtime Rig");
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var cameraRig = CreateStretchRect("CameraRig", root.transform);
            var stageObject = CreateStretchRect("Stage", cameraRig);
            var stage = Undo.AddComponent<VNStageController>(stageObject.gameObject);
            stage.EnsureRuntimeObjects();

            var transitionObject = CreateStretchRect("Transition", root.transform);
            var transition = Undo.AddComponent<VNScreenTransition>(transitionObject.gameObject);
            var vnCamera = Undo.AddComponent<VNCamera>(root);
            vnCamera.target = cameraRig;
            var weather = Undo.AddComponent<VNWeatherController>(root);
            var effects = Undo.AddComponent<VNEffectDirector>(root);
            var vnDirector = Undo.AddComponent<VNDirector>(root);
            vnDirector.Configure(catalog, stage, vnCamera, weather, transition, effects);

            if (UnityEngine.Object.FindFirstObjectByType<DialogueSystemController>() == null)
            {
                const string managerPath = "Assets/Plugins/Pixel Crushers/Dialogue System/Prefabs/Dialogue Manager.prefab";
                var managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(managerPath);
                if (managerPrefab != null)
                {
                    var manager = (GameObject)PrefabUtility.InstantiatePrefab(managerPrefab, root.scene);
                    Undo.RegisterCreatedObjectUndo(manager, "Create Dialogue Manager");
                }
                else Debug.LogWarning("[VN] Dialogue Manager prefab was not found. Add one with the Pixel Crushers setup menu.");
            }

            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeObject = root;
            Debug.Log("[VN] Runtime rig created. Assign sprites in the VNContentCatalog, then add a Dialogue Manager.", root);
        }

        [MenuItem("Tools/VN System/Validate Content Catalog")]
        public static void ValidateCatalogMenu()
        {
            var catalog = FindCatalog();
            if (catalog == null)
            {
                Debug.LogError("[VN] No VNContentCatalog exists. Run Create or Repair Runtime Rig first.");
                return;
            }

            var messages = VNContentValidator.Validate(catalog);
            if (messages.Count == 0) Debug.Log("[VN] Catalog validation passed.", catalog);
            else foreach (string message in messages) Debug.LogError("[VN] " + message, catalog);
        }

        internal static VNContentCatalog FindCatalog()
        {
            string guid = AssetDatabase.FindAssets("t:VNContentCatalog").FirstOrDefault();
            return string.IsNullOrEmpty(guid)
                ? null
                : AssetDatabase.LoadAssetAtPath<VNContentCatalog>(AssetDatabase.GUIDToAssetPath(guid));
        }

        static VNContentCatalog FindOrCreateCatalog()
        {
            var existing = FindCatalog();
            if (existing != null) return existing;
            EnsureFolder("Assets", "VNSystem");
            EnsureFolder("Assets/VNSystem", "Data");
            var catalog = ScriptableObject.CreateInstance<VNContentCatalog>();
            AddDefaultPresets(catalog);
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        static void AddDefaultPresets(VNContentCatalog catalog)
        {
            catalog.cameraPresets.Add(new VNCameraPreset { id = "camera.reset", move = VNCameraMove.Reset, duration = 0.8f });
            catalog.cameraPresets.Add(new VNCameraPreset { id = "camera.push.soft", move = VNCameraMove.PushIn, zoom = 1.06f, duration = 3f });
            catalog.cameraPresets.Add(new VNCameraPreset { id = "camera.closeup", move = VNCameraMove.SnapZoom, zoom = 1.12f, duration = 0.2f });
            catalog.effectPresets.Add(new VNEffectPreset { id = "weather.none", weather = VNWeather.None });
            catalog.effectPresets.Add(new VNEffectPreset { id = "weather.rain.light", weather = VNWeather.Rain });
            catalog.effectPresets.Add(new VNEffectPreset { id = "weather.snow", weather = VNWeather.Snow });
            catalog.effectPresets.Add(new VNEffectPreset { id = "weather.petals", weather = VNWeather.Petals });
            catalog.effectPresets.Add(new VNEffectPreset { id = "weather.fireflies", weather = VNWeather.Fireflies });
        }

        static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }

        static RectTransform CreateStretchRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }
    }

    public static class VNContentValidator
    {
        public static List<string> Validate(VNContentCatalog catalog)
        {
            var messages = new List<string>();
            ValidateIds(catalog.characters, item => item?.id, "character", messages);
            ValidateIds(catalog.backgrounds, item => item?.id, "background", messages);
            ValidateIds(catalog.cameraPresets, item => item?.id, "camera preset", messages);
            ValidateIds(catalog.effectPresets, item => item?.id, "effect preset", messages);

            foreach (var character in catalog.characters.Where(item => item != null))
            {
                if (character.expressions.Count == 0)
                    messages.Add($"Character '{character.id}' has no expressions.");
                ValidateIds(character.expressions, item => item?.id, $"expression in '{character.id}'", messages);
                foreach (var expression in character.expressions.Where(item => item != null))
                    if (expression.sprite == null) messages.Add($"Expression '{character.id}/{expression.id}' has no sprite.");
                if (character.FindExpression(character.defaultExpression) == null)
                    messages.Add($"Character '{character.id}' has no valid default expression '{character.defaultExpression}'.");
            }

            foreach (var background in catalog.backgrounds.Where(item => item != null))
                if (background.sprite == null) messages.Add($"Background '{background.id}' has no sprite.");
            return messages;
        }

        static void ValidateIds<T>(IEnumerable<T> items, Func<T, string> getId, string kind, List<string> messages)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int index = 0;
            foreach (var item in items)
            {
                string id = item == null ? null : getId(item);
                if (string.IsNullOrWhiteSpace(id)) messages.Add($"{kind} at index {index} has no id.");
                else if (!seen.Add(id.Trim())) messages.Add($"Duplicate {kind} id '{id}'.");
                index++;
            }
        }
    }

    public sealed class VNCommandBuilderWindow : EditorWindow
    {
        enum CommandType { Background, Character, Expression, Move, Hide, Camera, Weather, Effect, Wait }

        CommandType _type;
        string _id = "";
        string _secondary = "";
        VNCharacterSlot _slot = VNCharacterSlot.Center;
        VNEntrancePreset _entrance = VNEntrancePreset.FadeSlideUp;
        float _duration = 0.5f;
        bool _instant;
        string _result = "";

        [MenuItem("Tools/VN System/Sequence Command Builder")]
        static void Open() => GetWindow<VNCommandBuilderWindow>("VN Command Builder");

        void OnGUI()
        {
            EditorGUILayout.LabelField("Dialogue System Sequence Command", EditorStyles.boldLabel);
            _type = (CommandType)EditorGUILayout.EnumPopup("Command", _type);
            _id = EditorGUILayout.TextField("Primary ID", _id);
            if (_type == CommandType.Character || _type == CommandType.Move) _slot = (VNCharacterSlot)EditorGUILayout.EnumPopup("Slot", _slot);
            if (_type == CommandType.Character || _type == CommandType.Expression) _secondary = EditorGUILayout.TextField("Expression ID", _secondary);
            if (_type == CommandType.Character) _entrance = (VNEntrancePreset)EditorGUILayout.EnumPopup("Entrance", _entrance);
            if (_type == CommandType.Effect) _secondary = EditorGUILayout.TextField("Action (start/stop)", _secondary);
            _duration = EditorGUILayout.FloatField("Duration", _duration);
            _instant = EditorGUILayout.Toggle("Instant", _instant);

            if (GUILayout.Button("Build Command")) _result = Build();
            EditorGUILayout.SelectableLabel(_result, EditorStyles.textField, GUILayout.Height(36f));
            if (GUILayout.Button("Copy to Clipboard")) EditorGUIUtility.systemCopyBuffer = _result;
        }

        string Build()
        {
            switch (_type)
            {
                case CommandType.Background: return $"VNBG({_id}, crossfade, {F(_duration)}, {B(_instant)})";
                case CommandType.Character: return $"VNChar({_id}, {_slot}, {_secondary}, {_entrance}, {F(_duration)}, {B(_instant)})";
                case CommandType.Expression: return $"VNFace({_id}, {_secondary}, {F(_duration)}, {B(_instant)})";
                case CommandType.Move: return $"VNMove({_id}, {_slot}, {F(_duration)}, {B(_instant)})";
                case CommandType.Hide: return $"VNHide({_id}, {F(_duration)}, {B(_instant)})";
                case CommandType.Camera: return $"VNCamera({_id}, {F(_duration)}, {B(_instant)})";
                case CommandType.Weather: return $"VNWeather({_id}, {F(_duration)}, {B(_instant)})";
                case CommandType.Effect: return $"VNEffect({_id}, {_secondary}, {F(_duration)}, {B(_instant)})";
                default: return $"VNWait({F(_duration)})";
            }
        }

        static string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
        static string B(bool value) => value ? "true" : "false";
    }
}
