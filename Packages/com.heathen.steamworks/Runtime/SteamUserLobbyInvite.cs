#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Invite", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserLobbyInvite : MonoBehaviour
    {
        [SettingsField(header = "Invite")]
        public bool CreateIfMissing = true;
        private SteamUserData m_SteamUserData;

        private void Awake()
        {
            m_SteamUserData = GetComponent<SteamUserData>();
        }

        public void Invite(SteamLobbyData lobby)
        {
            if (lobby == null)
                return;

            if (lobby.Data.IsValid)
            {
                lobby.Data.InviteUserToLobby(m_SteamUserData.Data);
            }
            else if (CreateIfMissing)
            {
                var creator = lobby.GetComponent<SteamLobbyCreate>();
                var events = lobby.GetComponent<SteamLobbyDataEvents>();
                events.onCreate.AddListener(HandleLobbyCreated);
                creator.Create();
            }

            void HandleLobbyCreated(LobbyData arg0)
            {
                var events = lobby.GetComponent<SteamLobbyDataEvents>();
                events.onCreate.RemoveListener(HandleLobbyCreated);
                lobby.Data.InviteUserToLobby(m_SteamUserData.Data);
            }
        }

        public void Invite(LobbyData lobby)
        {
            if (m_SteamUserData.Data.IsValid
                && lobby.IsValid)
            {
                lobby.InviteUserToLobby(m_SteamUserData.Data);
            }
        }
    }
}
#endif