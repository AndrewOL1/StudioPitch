#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Join on Invite", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyJoin))]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyJoinOnInvite : MonoBehaviour
    {
        public enum JoinOnMode
        {
            WithInitialInvite,
            AfterAcceptInFriendChat
        }

        public enum FilterMode
        {
            None,
            IgnoreIfInParty,
            IgnoreIfInSession,
            IgnoreIfInAny,
        }

        public enum PreprocessOptions
        {
            None,
            LeaveAllFirst,
            LeavePartyFirst,
            LeaveSessionFirst,
        }

        [SettingsField(header = "Join")]
        public JoinOnMode mode = JoinOnMode.WithInitialInvite;
        [SettingsField(header = "Join")]
        public FilterMode filter = FilterMode.None;
        [SettingsField(header = "Join")]
        public PreprocessOptions preprocess = PreprocessOptions.None;

        private SteamLobbyData m_Inspector;
        private SteamLobbyJoin m_Join;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Join = GetComponent<SteamLobbyJoin>();
            API.Matchmaking.Client.OnLobbyInvite.AddListener(HandleInviteReceived);
            API.Overlay.Client.OnGameLobbyJoinRequested.AddListener(HandleInviteAccepted);
        }

        private void OnDestroy()
        {
            API.Matchmaking.Client.OnLobbyInvite.RemoveListener(HandleInviteReceived);
            API.Overlay.Client.OnGameLobbyJoinRequested.RemoveListener(HandleInviteAccepted);
        }

        private bool CanProcess()
        {
            switch(filter)
            {
                case FilterMode.None:
                    return true;
                case FilterMode.IgnoreIfInParty:
                    return !LobbyData.PartyLobby(out var _);
                case FilterMode.IgnoreIfInSession:
                    return !LobbyData.SessionLobby(out var _);
                case FilterMode.IgnoreIfInAny:
                    return API.Matchmaking.Client.memberOfLobbies.Count == 0;
                default:  return true;
            }
        }

        private void Preprocess()
        {
            if (preprocess == PreprocessOptions.None)
                return;

            var lobbies = API.Matchmaking.Client.memberOfLobbies.ToArray();
            for (int i = 0; i < lobbies.Length; i++)
            {
                switch(preprocess)
                {
                    case PreprocessOptions.LeaveAllFirst:
                        lobbies[i].Leave();
                        break;
                    case PreprocessOptions.LeaveSessionFirst:
                        if (lobbies[i].IsSession)
                            lobbies[i].Leave();
                        break;
                    case PreprocessOptions.LeavePartyFirst:
                        if (lobbies[i].IsParty)
                            lobbies[i].Leave();
                        break;
                }
            }
        }

        private void HandleInviteReceived(LobbyInvite arg0)
        {
            if (mode != JoinOnMode.WithInitialInvite
                || arg0.ToLobby.IsAMember(UserData.Me))
                return;

            Preprocess();
            if (!CanProcess())
                return;

            if (m_Join != null)
                m_Join.Join(arg0.ToLobby);
            else
                Debug.LogWarning("To join a lobby you are invited to the GameObject must have a Steam Lobby Join component");
        }

        private void HandleInviteAccepted(LobbyData arg0, UserData arg1)
        {
            if (mode != JoinOnMode.AfterAcceptInFriendChat
                || arg0.IsAMember(UserData.Me))
                return;

            Preprocess();
            if (!CanProcess())
                return;

            if (m_Join != null)
                m_Join.Join(arg0);
            else
                Debug.LogWarning("To join a lobby you are invited to the GameObject must have a Steam Lobby Join component");
        }

        public void OpenOverlay()
        {
            if (m_Inspector.Data.IsValid)
                API.Overlay.Client.ActivateInviteDialog(m_Inspector.Data);
            else
                Debug.LogWarning("No lobby to invite to");
        }

        public void InviteUser(UserData user)
        {
            if (m_Inspector.Data.IsValid)
                m_Inspector.Data.InviteUserToLobby(user);
            else
                Debug.LogWarning("No lobby to invite to");
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyJoinOnInvite), true)]
    public class SteamLobbyJoinOnInviteEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif