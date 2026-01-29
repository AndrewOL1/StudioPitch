#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamLobbyData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyDataEvents : MonoBehaviour
    {
        [EventField]
        public LobbyDataEvent onLobbyChange;
        [EventField]
        public UnityEvent<bool> onLobbySet;
        [EventField]
        public UnityEvent<bool> onLobbyRemoved;
        [EventField]
        public UnityEvent<string> onLobbyIdChanged;
        [EventField]
        public UnityEvent<bool> onLobbySetIsOwner;
        [EventField]
        public UnityEvent<bool> onLobbySetIsNotOwner;
        [EventField]
        public UnityEvent<bool> onLobbySetIsMember;
        [EventField]
        public UnityEvent<bool> onLobbySetIsNotMember;
        [EventField]
        public LobbyInviteEvent onLobbyInvite;
        [EventField]
        public GameLobbyJoinRequestedEvent onLobbyJoinRequest;
        [EventField]
        public LobbyChatMsgEvent onChatMessage;
        [EventField]
        public LobbyDataListEvent onSearchResult;
        /// <summary>
        /// Occurs when the local user enters a lobby as a response to a join
        /// </summary>
        [EventField]
        public LobbyDataEvent onEnterSuccess;
        /// <summary>
        /// Occurs when the local user tried but failed to enter a lobby
        /// </summary>
        [EventField]
        public LobbyResponseEvent onEnterFailure;
        /// <summary>
        /// Occurs when the local user creates a lobby
        /// </summary>
        [EventField]
        public LobbyDataEvent onCreate;
        /// <summary>
        /// Occurs when the local user tried but failed to create a lobby
        /// </summary>
        [EventField]
        public EResultEvent onCreationFailure;
        /// <summary>
        /// Occurs when the local user attempts to quick match but fails to find a match or resolve the quick match
        /// </summary>
        [EventField]
        public UnityEvent onQuickMatchFailure;
        /// <summary>
        /// Occurs when any data is updated on the lobby be that lobby metadata or a members metadata
        /// </summary>
        [EventField]
        public LobbyDataUpdateEvent onDataUpdate;
        /// <summary>
        /// Occurs when the local user leaves the managed lobby
        /// </summary>
        [EventField]
        public UnityEvent onUserLeft;
        /// <summary>
        /// Occurs when the local user is asked to leave the lobby via Heathen's Kick system
        /// </summary>
        [EventField]
        public UnityEvent onAskedToLeave;
        /// <summary>
        /// Occurs when the <see cref="GameServer"/> information is first set on the lobby
        /// </summary>
        [EventField]
        public GameServerSetEvent onGameCreate;
        /// <summary>
        /// Occurs when the local user is a member of a lobby and a new member joins that lobby
        /// </summary>
        [EventField]
        public UserDataEvent onOtherUserJoined;
        /// <summary>
        /// Occurs when the local user is a member of a lobby and another fellow member leaves the lobby
        /// </summary>
        [EventField]
        public UserLeaveEvent onOtherUserLeft;
        /// <summary>
        /// Occurs when the local user who is the owner of the lobby receives and starts an auth session request for a user
        /// </summary>
        [EventField]
        public LobbyAuthenticaitonSessionEvent onAuthenticationSessionResult;

        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChange);

            API.Matchmaking.Client.OnLobbyAskedToLeave.AddListener(HandleAskedToLeave);
            API.Matchmaking.Client.OnLobbyDataUpdate.AddListener(HandleLobbyDataUpdate);
            API.Matchmaking.Client.OnLobbyLeave.AddListener(HandleLobbyLeave);
            API.Matchmaking.Client.OnLobbyGameCreated.AddListener(HandleGameServerSet);
            API.Matchmaking.Client.OnLobbyChatUpdate.AddListener(HandleChatUpdate);
            API.Matchmaking.Client.OnLobbyAuthenticationRequest.AddListener(HandleAuthRequest);
            API.Matchmaking.Client.OnLobbyChatMsg.AddListener(HandleChatMessage);
            API.Matchmaking.Client.OnLobbyInvite.AddListener(onLobbyInvite.Invoke);
            API.Overlay.Client.OnGameLobbyJoinRequested.AddListener(onLobbyJoinRequest.Invoke);
        }

        private void OnDestroy()
        {
            API.Matchmaking.Client.OnLobbyGameCreated.RemoveListener(HandleGameServerSet);
            API.Matchmaking.Client.OnLobbyChatUpdate.RemoveListener(HandleChatUpdate);
            API.Matchmaking.Client.OnLobbyAuthenticationRequest.RemoveListener(HandleAuthRequest);
            API.Matchmaking.Client.OnLobbyChatMsg.RemoveListener(HandleChatMessage);
            API.Matchmaking.Client.OnLobbyInvite.RemoveListener(onLobbyInvite.Invoke);
            API.Overlay.Client.OnGameLobbyJoinRequested.RemoveListener(onLobbyJoinRequest.Invoke);
        }

        private void HandleOnChange(LobbyData arg0)
        {
            onLobbySet?.Invoke(arg0.IsValid);
            if (arg0.IsValid)
            {
                onLobbySetIsOwner?.Invoke(arg0.IsOwner);
                onLobbySetIsNotOwner?.Invoke(!arg0.IsOwner);
                onLobbySetIsMember?.Invoke(arg0.IsAMember(UserData.Me));
                onLobbySetIsNotMember?.Invoke(!arg0.IsAMember(UserData.Me));
            }
            onLobbyRemoved?.Invoke(!arg0.IsValid);
            onLobbyIdChanged?.Invoke(arg0.IsValid ? arg0.HexId : string.Empty);

            onLobbyChange.Invoke(arg0);
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == m_Inspector.Data)
            {
                onChatMessage.Invoke(message);
            }
        }

        private void HandleAuthRequest(LobbyData lobby, UserData sender, byte[] ticket, byte[] inventory)
        {
            if (lobby == m_Inspector.Data)
            {
                Authentication.BeginAuthSession(ticket, sender, (session) =>
                {
                    onAuthenticationSessionResult.Invoke(session, inventory);
                });
            }
        }

        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == m_Inspector.Data)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    onOtherUserJoined?.Invoke(arg0.m_ulSteamIDUserChanged);
                else
                    onOtherUserLeft?.Invoke(new UserLobbyLeaveData { user = arg0.m_ulSteamIDUserChanged, state = state });
            }
        }

        private void HandleGameServerSet(LobbyGameCreated_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == m_Inspector.Data)
                onGameCreate.Invoke(m_Inspector.Data.GameServer);
        }

        private void HandleLobbyLeave(LobbyData arg0)
        {
            if (arg0 == m_Inspector.Data)
            {
                m_Inspector.Data = default;
                onUserLeft.Invoke();
            }
        }

        private void HandleAskedToLeave(LobbyData arg0)
        {
            if (arg0 == m_Inspector.Data)
                onAskedToLeave.Invoke();
        }

        private void HandleLobbyDataUpdate(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == m_Inspector.Data)
                onDataUpdate.Invoke(arg0);
        }
    }
}
#endif