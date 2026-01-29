#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Lobby")]
    [HelpURL("https://kb.heathen.group/steam/features/lobby")]
    public class SteamLobbyData : MonoBehaviour, ISteamLobbyData
    {
        public enum LoadOnStart
        {
            None,
            Any,
            Party,
            Session,
            General
        }

        public LoadOnStart load = LoadOnStart.None;

        public LobbyData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                onChanged?.Invoke(value);
            }
        }

        [HideInInspector]
        public LobbyDataEvent onChanged;

        private LobbyData m_Data;

        [SerializeField]
        private List<string> m_Delegates;

        private void Start()
        {
            switch (load)
            {
                case LoadOnStart.Any:
                    if (API.Matchmaking.Client.memberOfLobbies.Count > 0)
                        Data = API.Matchmaking.Client.memberOfLobbies[0];
                    break;
                case LoadOnStart.General:
                    if (API.Matchmaking.Client.memberOfLobbies.Count > 0)
                        foreach (var lobby in API.Matchmaking.Client.memberOfLobbies)
                            if (lobby.IsGeneral)
                            {
                                Data = lobby;
                                break;
                            }
                    break;
                case LoadOnStart.Session:
                    if (API.Matchmaking.Client.memberOfLobbies.Count > 0)
                        foreach (var lobby in API.Matchmaking.Client.memberOfLobbies)
                            if (lobby.IsSession)
                            {
                                Data = lobby;
                                break;
                            }
                    break;
                case LoadOnStart.Party:
                    if (API.Matchmaking.Client.memberOfLobbies.Count > 0)
                        foreach (var lobby in API.Matchmaking.Client.memberOfLobbies)
                            if (lobby.IsParty)
                            {
                                Data = lobby;
                                break;
                            }
                    break;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamLobbyData), true)]
    public class SteamLobbyDataEditor : ModularEditor
    {
        private SteamToolsSettings settings;
        private SerializedProperty loadProperty;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            // Fields
            typeof(SteamLobbyHexIdInputField),
            typeof(SteamLobbyHexIdLabel),
            typeof(SteamLobbyMaxSlots),
            typeof(SteamLobbyMemberCount),
            typeof(SteamLobbyMembers),
            typeof(SteamLobbyName),

            // Functions / single-instance components
            typeof(SteamLobbyCreate),
            typeof(SteamLobbyJoin),
            typeof(SteamLobbyJoinOnInvite),
            typeof(SteamLobbyJoinSessionLobby),
            typeof(SteamLobbyLeave),
            typeof(SteamLobbyInvite),
            typeof(SteamLobbyQuickMatch),
            typeof(SteamLobbyMetadata),
            typeof(SteamLobbyGameServer),
            typeof(SteamLobbyChatUI),
            typeof(SteamLobbyInputUI),
            typeof(SteamLobbyDataEvents),
            typeof(SteamLobbyInvokeCommandLine)
        };

        private void OnEnable()
        {
            settings = SteamToolsSettings.GetOrCreate();
            loadProperty = serializedObject.FindProperty("load");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{settings.Get(settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/lobby"
                , "https://discord.gg/heathen-group-463483739612381204"
                , new[]{ loadProperty });
            
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif