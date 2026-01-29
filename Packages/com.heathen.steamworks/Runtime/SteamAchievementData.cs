#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Steamworks/Achievement")]
    [HelpURL("https://kb.heathen.group/steam/features/achievements")]
    public class SteamAchievementData : MonoBehaviour
    {
        public string apiName;
        public AchievementData Data
        {
            get => apiName;
            set => apiName = value.ApiName;
        }

        [SerializeField]
        private List<string> m_Delegates;

        public void Unlock() => Data.Unlock();
        public void Clear() => Data.Clear();
        public void Store() => Data.Store();
        public void SetAchieved(bool value)
        {
            if (value)
                Data.Unlock();
            else
                Data.Clear(); 
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamAchievementData), true)]
    public class SteamAchievementDataEditor : ModularEditor
    {
        private string[] _options;
        private int _selectedIndex;
        private SerializedProperty apiNameProp;
        private SteamToolsSettings settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamAchievementName),
            typeof(SteamAchievementDescription),
            typeof(SteamAchievementIcon),
            typeof(SteamAchievementChanged),
        };

        private void OnEnable()
        {
            apiNameProp = serializedObject.FindProperty("apiName");
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            settings = SteamToolsSettings.GetOrCreate();
            var list = settings != null && settings.achievements != null
                ? settings.achievements
                : new System.Collections.Generic.List<string>();

            if (list.Count > 0)
            {
                _options = list.ToArray();
                var current = apiNameProp.stringValue;
                _selectedIndex = Mathf.Max(0, System.Array.IndexOf(_options, current));
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
            }
            else
            {
                _options = null;
            }
        }

        public override void OnInspectorGUI()
        {
            if(settings != null)
                settings = SteamToolsSettings.GetOrCreate();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.Get(settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/achievements");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // === Achievement dropdown ===
            if (_options == null || _options.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No achievements found!.\n\n" +
                    "Open Project Settings > Player > Steamworks to configure your achievements.",
                    MessageType.Warning
                );

                serializedObject.ApplyModifiedProperties();
                return;
            }

            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _options);
            if (_selectedIndex >= 0 && _selectedIndex < _options.Length)
                apiNameProp.stringValue = _options[_selectedIndex];

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
    }
#endif
}
#endif