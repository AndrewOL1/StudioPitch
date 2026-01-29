#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Lobby Member")]
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-member-data")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamLobbyMemberData : MonoBehaviour
    {
        public LobbyMemberData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if(m_UserData == null)
                    m_UserData = GetComponent<SteamUserData>();
                m_UserData.Data = value.user;
                if (m_Events != null)
                    m_Events.onMetadataChanged?.Invoke(new() { lobby = m_Data.lobby, member = m_Data.user.IsValid ? m_Data : null });
            }
        }
        
        

        public LobbyData Lobby
        {
            get => Data.lobby;
        }

        private LobbyMemberData m_Data;
        private SteamUserData m_UserData;
        private SteamLobbyMemberDataEvents m_Events;
        [SerializeField]
        private List<string> m_Delegates;

        private void Awake()
        {
            m_UserData = GetComponent<SteamUserData>();
            m_Events = GetComponent<SteamLobbyMemberDataEvents>();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMemberData), true)]
    public class SteamLobbyMemberDataEditor : ModularEditor
    {
        private SteamToolsSettings settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamLobbyMemberChatMessage),
            typeof(SteamLobbyMemberDataEvents),
        };

        private void OnEnable()
        {
            settings = SteamToolsSettings.GetOrCreate();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{settings.Get(settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/lobby"
                , "https://discord.gg/heathen-group-463483739612381204"
                , null);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif