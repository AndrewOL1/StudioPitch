#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
#endif
using UnityEngine;
using Heathen.SteamworksIntegration.API;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationData))]
    public class SteamAuthenticationGetTicket : MonoBehaviour
    {
        public AuthenticationTicket Data
        {
            get => m_Data;
            private set
            {
                m_Data = value;
            }
        }
        private AuthenticationTicket m_Data;

        public void GetTicketForLobbyServer(SteamLobbyData lobby)
        {
            if (!lobby.Data.IsValid)
                return;

            var serverData = lobby.Data.GameServer;
            if (serverData.id.IsValid())
            {
                Authentication.GetAuthSessionTicket(serverData.id, HandleTicketCallback);
            }
        }

        public void GetTicketForLobbyOwner(SteamLobbyData lobby)
        {
            if (lobby.Data.IsValid)
            {
                Authentication.GetAuthSessionTicket(lobby.Data.Owner.user, HandleTicketCallback);
            }
        }

        public void GetTicketForUser(SteamUserData user)
        {
            if (user.Data.IsValid)
            {
                Authentication.GetAuthSessionTicket(user.Data, HandleTicketCallback);
            }
        }

        public void GetTicketForGameServer(SteamGameServerData server)
        {
            if(server.Data.SteamId.IsValid())
            {
                Authentication.GetAuthSessionTicket(server.Data.SteamId, HandleTicketCallback);
            }
        }

        public void GetTicketForWebAPI(string identity)
        {
            Authentication.GetWebAuthSessionTicket(identity, HandleTicketCallback);
        }

        private void HandleTicketCallback(AuthenticationTicket ticket, bool ioError)
        {
            Data = ticket;
        }
    }
}
#endif