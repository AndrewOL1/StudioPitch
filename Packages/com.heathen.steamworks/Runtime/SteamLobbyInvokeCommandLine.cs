#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Command Line", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyDataEvents))]
    public class SteamLobbyInvokeCommandLine : MonoBehaviour
    {
        public enum Rule
        {
            Any,
            PartyOnly,
            SessionOnly,
            GeneralOnly,
            NotParty,
            NotSession,
            NotGeneral,
        }

        [SettingsField(header = "Launch Command")]
        public Rule joinRequestedWhen;

        private SteamLobbyDataEvents m_Events;
        private LobbyData pendingLobby = 0;

        private void Start()
        {
            m_Events = GetComponent<SteamLobbyDataEvents>();

            pendingLobby = API.Matchmaking.Client.GetCommandLineConnectLobby();
            if (pendingLobby.IsValid)
            {
                API.Matchmaking.Client.OnLobbyDataUpdate.AddListener(HandleLobbyDataUpdate);
                pendingLobby.RequestData();
            }
        }

        private void HandleLobbyDataUpdate(LobbyDataUpdateEventData arg0)
        {
            API.Matchmaking.Client.OnLobbyDataUpdate.RemoveListener(HandleLobbyDataUpdate);
            switch (joinRequestedWhen)
            {
                case Rule.Any:
                    m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
                case Rule.GeneralOnly:
                    if (pendingLobby.IsGeneral)
                        m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
                case Rule.NotGeneral:
                    if (!pendingLobby.IsGeneral)
                        m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
                case Rule.PartyOnly:
                    if (pendingLobby.IsParty)
                        m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
                case Rule.NotParty:
                    if (!pendingLobby.IsParty)
                        m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
                case Rule.SessionOnly:
                    if (pendingLobby.IsSession)
                        m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
                case Rule.NotSession:
                    if (!pendingLobby.IsSession)
                        m_Events.onLobbyJoinRequest?.Invoke(pendingLobby, UserData.Me);
                    break;
            }
        }
    }
}
#endif