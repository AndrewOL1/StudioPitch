#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration.UI;
using Steamworks;
using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Leaderboard")]
    public class SteamLeaderboardData : MonoBehaviour
    {
        public enum ManagedLeaderboardEvents
        {
            LeaderboardChanged,
            FoundOrCreated,
            FailedToFindOrCreate,
            RankChange,
            ScoreUpload,
        }

        // the sort order of a leaderboard
        public enum LeaderboardSortMethod : int
        {
            TopIsLowestScore = 1,  // top-score is lowest number
            TopIsHighestScore = 2, // top-score is highest number
        }

        // the display type (used by the Steam Community web site) for a leaderboard
        public enum LeaderboardDisplayType : int
        {
            Numeric = 1,           // simple numerical score
            TimeSeconds = 2,       // the score represents a time, in seconds
            TimeMilliSeconds = 3,  // the score represents a time, in milliseconds
        }

        public string apiName;
        public bool createIfMissing = false;
        public LeaderboardDisplayType createAsDisplay = LeaderboardDisplayType.Numeric;
        public LeaderboardSortMethod createWithSort = LeaderboardSortMethod.TopIsLowestScore;

        public LeaderboardData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if (m_Events != null)
                    m_Events.onChange?.Invoke();
            }
        }

        [SerializeField]
        private List<string> m_Delegates;
        private LeaderboardData m_Data;
        private SteamLeaderboardDataEvents m_Events;

        private void Awake()
        {
            m_Events = GetComponent<SteamLeaderboardDataEvents>();
        }

        private void Interface_OnReady()
        {
            SteamTools.Interface.OnReady -= Interface_OnReady;

            if (!m_Data.IsValid)
            {
                if(!string.IsNullOrEmpty(apiName))
                {
                    m_Data = SteamTools.Interface.GetBoard(apiName);
                    if (!m_Data.IsValid)
                    {
                        if (createIfMissing)
                            API.Leaderboards.Client.FindOrCreate(Data.apiName, (ELeaderboardSortMethod)createWithSort, (ELeaderboardDisplayType)createAsDisplay, (data, ioError) =>
                            {
                                if (!ioError)
                                {
                                    m_Data = data;
                                    if (m_Events != null)
                                        m_Events.onFindOrCreate?.Invoke();
                                }
                                else if (m_Events != null)
                                    m_Events.onFindOrCreateFailure?.Invoke();
                            });
                    }
                }
            }
        }

        private void Start()
        {
            m_Events = GetComponent<SteamLeaderboardDataEvents>();
            if (SteamTools.Interface.IsReady)
                Interface_OnReady();
            else
                SteamTools.Interface.OnReady += Interface_OnReady;
        }

        private void HandleInitialization()
        {
            API.App.onSteamInitialized.RemoveListener(HandleInitialization);
            Interface_OnReady();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamLeaderboardData), true)]
    public class SteamLeaderboardDataEditor : ModularEditor
    {
        private SteamToolsSettings settings;

        private string[] _options;
        private int _selectedIndex;
        private SerializedProperty apiNameProp;
        private SerializedProperty createIfMissingProp;
        private SerializedProperty createAsDisplayProp;
        private SerializedProperty createWithSortProp;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamLeaderboardDataEvents),
            typeof(SteamLeaderboardDisplay),
            typeof(SteamLeaderboardName),
            typeof(SteamLeaderboardRank),
            typeof(SteamLeaderboardUpload),
            typeof(SteamLeaderboardUserEntry),
        };

        // --- Single-instance functions (Flags enum) ---
        [Flags]
        private enum FunctionsMask
        {
            None = 0,
            Display = 1 << 0,
            Upload = 1 << 1,
            GeneralEvents = 1 << 2,
        }

        private void OnEnable()
        {
            apiNameProp = serializedObject.FindProperty("apiName");
            createIfMissingProp = serializedObject.FindProperty("createIfMissing");
            createAsDisplayProp = serializedObject.FindProperty("createAsDisplay");
            createWithSortProp = serializedObject.FindProperty("createWithSort");

            settings = SteamToolsSettings.GetOrCreate();
            
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            var settings = SteamToolsSettings.GetOrCreate();
            var list = settings != null && settings.leaderboards != null
                ? settings.leaderboards
                : new System.Collections.Generic.List<string>();

            var temp = new string[list.Count + 1];
            for (int i = 0; i < list.Count; i++)
                temp[i] = list[i];
            temp[list.Count] = "<new>";

            _options = temp;

            var current = apiNameProp.stringValue;
            _selectedIndex = Mathf.Max(0, System.Array.IndexOf(_options, current));
            if (_selectedIndex < 0 || _selectedIndex >= _options.Length - 1)
                _selectedIndex = _options.Length - 1; // default to <new>
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var go = ((SteamLeaderboardData)target).gameObject;

            // --- Header links ---
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.Get(settings.ActiveApp.Value).applicationId);
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/leaderboards");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_options == null)
                RefreshOptions();

            _selectedIndex = EditorGUILayout.Popup("Leaderboard", _selectedIndex, _options);

            if (_selectedIndex >= 0 && _selectedIndex < _options.Length - 1)
            {
                // Existing leaderboard selected
                apiNameProp.stringValue = _options[_selectedIndex];
            }
            else
            {
                // <new> selected — show editable fields
                EditorGUILayout.PropertyField(apiNameProp, new GUIContent("API Name"));
                EditorGUILayout.PropertyField(createIfMissingProp);
                EditorGUILayout.PropertyField(createAsDisplayProp);
                EditorGUILayout.PropertyField(createWithSortProp);
            }

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