#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Authentication")]
    [HelpURL("https://kb.heathen.group/steam/features/authentication")]
    public class SteamAuthenticationData : MonoBehaviour
    {
        public enum ManagedEvents
        {
            Changed,
            TicketRequestErred,
            RPCInvoked,
            InvalidTicketReceived,
            InvalidSessionRequested,
            SessionStarted,
        }

        public AuthenticationTicket Data
        {
            get => m_Data; 
            set
            {
                m_Data = value;
                if(m_Events != null) 
                    m_Events.onChange?.Invoke(m_Data);
            }
        }

        private AuthenticationTicket m_Data;
        private SteamAuthenticationEvents m_Events;
        [SerializeField]
        private System.Collections.Generic.List<ManagedEvents> m_Delegates;

        private void Awake()
        {
            m_Events = GetComponent<SteamAuthenticationEvents>();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamAuthenticationData), true)]
    public class SteamAuthenticationDataEditor : Editor
    {
        private SteamToolsSettings settings;
        private static readonly string[] _settingsOptions =
        {
            "Sessions",
            "Get Ticket",
            "Send RPC",
            "General Events"
        };

        [System.Flags]
        private enum SettingsMask
        {
            None = 0,
            Sessions = 1 << 0,
            GetTicket = 1 << 1,
            SendRpc = 1 << 2,
            GeneralEvents = 1 << 3
        }

        private SettingsMask _settingsMask = SettingsMask.None;

        private void OnEnable()
        {
            settings = SteamToolsSettings.GetOrCreate();
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

            DrawFunctionSettings();
            DrawFunctionEvents();
            serializedObject.ApplyModifiedProperties();
        }

        private static System.Type GetTypeForFeature(string featureName)
        {
            return featureName switch
            {
                // Settings
                "Sessions" => typeof(SteamAuthenticationSessions),
                "Get Ticket" => typeof(SteamAuthenticationGetTicket),
                "Send RPC" => typeof(SteamAuthenticationRpcInvoke),
                "General Events" => typeof(SteamAuthenticationEvents),
                _ => null,
            };
        }

        private System.Collections.Generic.IEnumerable<(SerializedProperty prop, string header)> GetPropertiesWithAttribute<T>(SerializedObject so)
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

        private void DrawFunctionSettings()
        {
            var data = (SteamAuthenticationData)target;
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
            _settingsMask = (SettingsMask)EditorGUILayout.EnumFlagsField("Settings", _settingsMask);
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

                var targetProperties = GetPropertiesWithAttribute<SettingsFieldAttribute>(so).ToList();
                if (targetProperties.Count > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    foreach (var (prop, header) in GetPropertiesWithAttribute<SettingsFieldAttribute>(so))
                        EditorGUILayout.PropertyField(prop, new GUIContent(ObjectNames.NicifyVariableName(prop.name)));
                    EditorGUILayout.EndVertical();
                }

                so.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawFunctionEvents()
        {
            var data = (SteamAuthenticationData)target;
            var soData = serializedObject;
            var delegatesProp = soData.FindProperty("m_Delegates");

            if (delegatesProp == null)
                return;

            var dataEvents = data.GetComponent<SteamAuthenticationEvents>();

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
                var evt = (SteamAuthenticationData.ManagedEvents)delegateProp.enumValueIndex;

                GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(evt.ToString()));
                SerializedProperty propToDraw = null;

                // --- Match enum to actual event SerializedProperty ---
                switch (evt)
                {
                    // === SteamLobbyDataEvents ===
                    case SteamAuthenticationData.ManagedEvents.Changed:
                        propToDraw = soEvents?.FindProperty(nameof(SteamAuthenticationEvents.onChange)); break;
                    case SteamAuthenticationData.ManagedEvents.RPCInvoked:
                        propToDraw = soEvents?.FindProperty(nameof(SteamAuthenticationEvents.onRpcInvoke)); break;
                    case SteamAuthenticationData.ManagedEvents.TicketRequestErred:
                        propToDraw = soEvents?.FindProperty(nameof(SteamAuthenticationEvents.onError)); break;
                    case SteamAuthenticationData.ManagedEvents.InvalidTicketReceived:
                        propToDraw = soEvents?.FindProperty(nameof(SteamAuthenticationEvents.onInvalidTicket)); break;
                    case SteamAuthenticationData.ManagedEvents.InvalidSessionRequested:
                        propToDraw = soEvents?.FindProperty(nameof(SteamAuthenticationEvents.onInvalidSession)); break;
                    case SteamAuthenticationData.ManagedEvents.SessionStarted:
                        propToDraw = soEvents?.FindProperty(nameof(SteamAuthenticationEvents.onSessionStart)); break;
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

                foreach (SteamAuthenticationData.ManagedEvents evt in System.Enum.GetValues(typeof(SteamAuthenticationData.ManagedEvents)))
                {
                    bool alreadyAdded = false;
                    for (int i = 0; i < delegatesProp.arraySize; i++)
                    {
                        if ((SteamAuthenticationData.ManagedEvents)delegatesProp.GetArrayElementAtIndex(i).enumValueIndex == evt)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded)
                        menu.AddDisabledItem(new GUIContent(evt.ToString()));
                    else
                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(evt.ToString())), false, () =>
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
    }
#endif
}
#endif