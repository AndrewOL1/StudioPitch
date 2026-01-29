#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Input Action")]
    [HelpURL("https://kb.heathen.group/steam/features/input")]
    public class SteamInputActionData : MonoBehaviour, ISteamInputActionData
    {
        [SerializeField]
        private string setName;
        [SerializeField]
        private string layerName;
        [SerializeField]
        private string actionName;

        public InputActionSetData Set
        {
            get => m_Set;
            set
            {
                m_Set = value;
            }
        }
        public InputActionSetLayerData Layer
        {
            get => m_Layer;
            set
            {
                m_Layer = value;
            }
        }
        public InputActionData Action
        {
            get => m_Action;
            set
            {
                m_Action = value;
            }
        }
        private InputActionSetData m_Set;
        private InputActionSetLayerData m_Layer;
        private InputActionData m_Action;
        [SerializeField]
        private List<string> m_Delegates;

        private void Start()
        {
            if (SteamTools.Interface.IsReady)
                Interface_OnReady();
            else
                SteamTools.Interface.OnReady += Interface_OnReady;
        }

        private void Interface_OnReady()
        {
            SteamTools.Interface.OnReady -= Interface_OnReady;

            m_Set = SteamTools.Interface.GetSet(layerName);
            m_Layer = new() { layerName = layerName };
            m_Action = SteamTools.Interface.GetAction(actionName);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamInputActionData))]
    public class SteamInputActionDataEditor : ModularEditor
    {
        private SerializedProperty setNameProp;
        private SerializedProperty layerNameProp;
        private SerializedProperty actionNameProp;

        private SteamToolsSettings settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamInputActionName),
            typeof(SteamInputActionGlyph),
            typeof(SteamInputActionEvent),
        };

        private string[] setOptions = new string[0];
        private string[] layerOptions = new string[0];
        private string[] actionOptions = new string[0];

        private void OnEnable()
        {
            setNameProp = serializedObject.FindProperty("setName");
            layerNameProp = serializedObject.FindProperty("layerName");
            actionNameProp = serializedObject.FindProperty("actionName");

            settings = SteamToolsSettings.GetOrCreate();

            if (settings != null)
            {
                setOptions = settings.inputSets?.ToArray() ?? new string[0];
                layerOptions = settings.inputLayers?.ToArray() ?? new string[0];
                actionOptions = settings.inputActions?.ToArray() ?? new string[0];
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.Get(settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/input");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            if (actionOptions == null || actionOptions.Length == 0)
            {
                EditorGUILayout.HelpBox("No Actions Founds! Configure Steamworks in Project Settings > Player > Steamworks.", MessageType.Warning);

                serializedObject.ApplyModifiedProperties();
                return;
            }

            // ---- Dropdowns for Set / Layer / Action ----
            DrawPopup("Set", setNameProp, setOptions);
            DrawPopup("Layer", layerNameProp, layerOptions);
            DrawPopup("Action", actionNameProp, actionOptions);

            EditorGUILayout.Space();

            // --- Features Dropdown ---
            HideAllAllowedComponents();
            DrawAddFieldDropdown();

            // --- Draw existing components via attributes ---
            EditorGUI.indentLevel++;
            DrawModularComponents();
            EditorGUI.indentLevel--;

            // --- Draw Functions as Flags (single-instance components) ---
            DrawFunctionFlags();

            // --- Draw Settings / Elements / Templates / Events ---
            DrawFields<SettingsFieldAttribute>("Settings");
            DrawFields<ElementFieldAttribute>("Elements");
            DrawFields<TemplateFieldAttribute>("Templates");
            DrawEventFields();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPopup(string label, SerializedProperty prop, string[] options)
        {
            int index = Mathf.Max(0, System.Array.IndexOf(options, prop.stringValue));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(60f));
            index = EditorGUILayout.Popup(index, options);
            EditorGUILayout.EndHorizontal();

            if (index >= 0 && index < options.Length)
                prop.stringValue = options[index];
        }
    }
#endif
}
#endif