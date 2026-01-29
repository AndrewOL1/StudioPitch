#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Controller")]
    public class SteamInputControllerData : MonoBehaviour, ISteamInputControllerData
    {
        public enum ManagedEvents
        {
            Change,
            Update,
        }

        [Tooltip("If set to true then we will attempt to force Steam to use input for this app on start.\nThis is generally only needed in editor testing.")]
        [SerializeField]
        private bool forceInput = true;
        [Tooltip("If true then the first controller connected will be set as the controller handle")]
        public bool getFirst = true;

        [SerializeField]
        private List<ManagedEvents> m_Delegates;

        public InputHandle_t? Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                onChanged?.Invoke();
            }
        }

        public UnityEvent onChanged;

        private InputHandle_t? m_Data = null;

        private void Awake()
        {
            if (!SteamTools.Interface.IsReady)
                SteamTools.Interface.OnReady += HandleLateInit;
            else
                HandleLateInit();
        }

        private void OnDestroy()
        {
            if (forceInput)
                Application.OpenURL("steam://forceinputappid/0");
        }

        [ContextMenu("Set First Controller")]
        public void SetFirstFound()
        {
            if (API.Input.Client.connectedControllers.Count > 0)
                Data = API.Input.Client.connectedControllers[0];
            else
                Data = null;
        }

        public InputMotionData_t GetMotion()
        {
            return API.Input.Client.GetMotionData(m_Data.Value);
        }

        private void HandleLateInit()
        {
            SteamTools.Interface.OnReady -= HandleLateInit;

            if (forceInput)
            {
                Application.OpenURL($"steam://forceinputappid/{API.App.Id}");
                if (getFirst)
                    Invoke(nameof(SetFirstFound), 1);
            }
            else if (getFirst)
                SetFirstFound();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamInputControllerData), true)]
    public class SteamInputControllerDataEditor : Editor
    {
        private SerializedProperty forceInputProp;
        private SerializedProperty getFirstProp;
        private SteamToolsSettings settings;

        private static readonly string[] _settingsOptions =
        {
            "Vibrate",
            "General Events"
        };

        private SettingsMask _settingsMask = SettingsMask.None;

        private void OnEnable()
        {
            settings = SteamToolsSettings.GetOrCreate();
            forceInputProp = serializedObject.FindProperty("forceInput");
            getFirstProp = serializedObject.FindProperty("getFirst");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // === Header links ===
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.Get(settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/friends");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(forceInputProp);
            EditorGUILayout.PropertyField(getFirstProp);
            EditorGUILayout.Space();

            DrawFunctionSettings();
            DrawFunctionEvents();
            serializedObject.ApplyModifiedProperties();
        }

        // --- SETTINGS (single-instance) ---
        private void DrawFunctionSettings()
        {
            var data = (SteamInputControllerData)target;
            var go = data.gameObject;

            // Refresh mask
            _settingsMask = SettingsMask.None;
            for (int i = 0; i < _settingsOptions.Length; i++)
            {
                var type = GetTypeForFeature(_settingsOptions[i]);
                if (type != null && go.GetComponent(type) != null)
                    _settingsMask |= (SettingsMask)(1 << i);
            }

            EditorGUI.BeginChangeCheck();
            _settingsMask = (SettingsMask)EditorGUILayout.EnumFlagsField("Configuration", _settingsMask);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < _settingsOptions.Length; i++)
                {
                    var type = GetTypeForFeature(_settingsOptions[i]);
                    if (type == null) continue;

                    bool hasComp = go.GetComponent(type) != null;
                    bool shouldHave = (_settingsMask & (SettingsMask)(1 << i)) != 0;

                    if (shouldHave && !hasComp)
                        go.AddComponent(type).hideFlags = HideFlags.HideInInspector;
                    else if (!shouldHave && hasComp)
                        DestroyImmediate(go.GetComponent(type));
                }
            }
            EditorGUI.indentLevel++;
            // Draw configuration for active settings
            foreach (var featureName in _settingsOptions)
            {
                var type = GetTypeForFeature(featureName);
                var comp = go.GetComponent(type);
                if (comp == null) continue;

                var so = new SerializedObject(comp);
                so.Update();

                EditorGUILayout.BeginVertical("box");
                foreach (var (prop, header) in GetPropertiesWithAttribute<SettingsFieldAttribute>(so))
                    EditorGUILayout.PropertyField(prop, new GUIContent(ObjectNames.NicifyVariableName(prop.name)));
                EditorGUILayout.EndVertical();

                so.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFunctionEvents()
        {
            var data = (SteamInputControllerData)target;
            var soData = serializedObject;
            var delegatesProp = soData.FindProperty("m_Delegates");

            if (delegatesProp == null)
                return;

            var dataEvents = data.GetComponent<SteamInputControllerDataEvents>();

            if (dataEvents == null)
                return;

            if (dataEvents != null)
                dataEvents.hideFlags = HideFlags.HideInInspector;

            SerializedObject soEvents = dataEvents ? new SerializedObject(dataEvents) : null;

            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            int removeIndex = -1;
            Vector2 removeButtonSize = GUIStyle.none.CalcSize(EditorGUIUtility.IconContent("Toolbar Minus"));

            for (int i = 0; i < delegatesProp.arraySize; i++)
            {
                var delegateProp = delegatesProp.GetArrayElementAtIndex(i);
                var evt = (SteamInputControllerData.ManagedEvents)delegateProp.enumValueIndex;

                GUIContent label = new GUIContent(evt.ToString());
                SerializedProperty propToDraw = null;

                // --- Match enum to actual event SerializedProperty ---
                switch (evt)
                {
                    case SteamInputControllerData.ManagedEvents.Change:
                        propToDraw = soEvents?.FindProperty(nameof(SteamInputControllerDataEvents.onChange)); break;
                    case SteamInputControllerData.ManagedEvents.Update:
                        propToDraw = soEvents?.FindProperty(nameof(SteamInputControllerDataEvents.onUpdate)); break;
                }

                if (propToDraw != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(propToDraw, label, true);

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25)))
                        removeIndex = i;

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                else
                {
                    EditorGUILayout.HelpBox($"Event {evt} is not available on current components.", MessageType.Info);
                }
            }

            if (removeIndex >= 0)
                delegatesProp.DeleteArrayElementAtIndex(removeIndex);

            // === Add New Event Menu ===
            if (GUILayout.Button("Add New Event Type"))
            {
                GenericMenu menu = new GenericMenu();

                foreach (SteamUserData.ManagedEvents evt in System.Enum.GetValues(typeof(SteamUserData.ManagedEvents)))
                {
                    bool alreadyAdded = false;
                    for (int i = 0; i < delegatesProp.arraySize; i++)
                    {
                        if ((SteamUserData.ManagedEvents)delegatesProp.GetArrayElementAtIndex(i).enumValueIndex == evt)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        menu.AddDisabledItem(new GUIContent(evt.ToString()));
                    else
                        menu.AddItem(new GUIContent(evt.ToString()), false, () =>
                        {
                            delegatesProp.arraySize++;
                            delegatesProp.GetArrayElementAtIndex(delegatesProp.arraySize - 1).enumValueIndex = (int)evt;
                            soData.ApplyModifiedProperties();
                        });
                }

                menu.ShowAsContext();
            }

            EditorGUI.indentLevel--;
            soData.ApplyModifiedProperties();
            soEvents?.ApplyModifiedProperties();
        }

        // --- Helpers ---
        private static System.Type GetTypeForFeature(string featureName)
        {
            return featureName switch
            {
                // Settings
                "Vibrate" => typeof(SteamInputControllerVibrate),
                "General Events" => typeof(SteamInputControllerDataEvents),
                _ => null,
            };
        }

        private IEnumerable<(SerializedProperty prop, string header)> GetPropertiesWithAttribute<T>(SerializedObject so)
            where T : PropertyAttribute
        {
            var prop = so.GetIterator();
            if (!prop.NextVisible(true)) yield break;
            while (prop.NextVisible(false))
            {
                var field = so.targetObject.GetType().GetField(prop.name,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (field == null) continue;
                var attr = System.Attribute.GetCustomAttribute(field, typeof(T)) as SettingsFieldAttribute;
                if (attr != null)
                    yield return (so.FindProperty(prop.name), attr.header);
            }
        }

        [System.Flags]
        private enum SettingsMask
        {
            None = 0,
            Vibrate = 1 << 0,
            GeneralEvents = 1 << 1
        }
    }
#endif
}
#endif