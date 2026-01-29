#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Join Session", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    [RequireComponent(typeof(SteamLobbyJoin))]
    public class SteamLobbyJoinSessionLobby : MonoBehaviour
    {
        [SettingsField(header = "Join")]
        public SteamLobbyData partyLobbyData;
        [SettingsField(header = "Join")]
        public bool leaveOnSessionClear = true;
        private SteamLobbyData m_Inspector;
        private SteamLobbyJoin m_Join;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            
            if (partyLobbyData != null)
                partyLobbyData.onChanged.AddListener(HandleChange);

            API.Matchmaking.Client.OnLobbyDataUpdate.AddListener(HandleDataUpdate);
        }

        private void OnDestroy()
        {
            if (partyLobbyData != null)
                partyLobbyData.onChanged.RemoveListener(HandleChange);

            API.Matchmaking.Client.OnLobbyDataUpdate.RemoveListener(HandleDataUpdate);
        }

        private void HandleChange(LobbyData arg0)
        {
            // A new party lobby has been set, we should check if this party lobby is valid and if so is a session declared on it
            // Check if the party is valid
            if(arg0.IsValid)
            {
                // Get the session ID if any is declared
                var sessionId = arg0[LobbyData.DataSessionLobby];
                
                // If this is not null then we have a declared session lobby ID
                if(!string.IsNullOrEmpty(sessionId))
                {
                    var sessionLobby = LobbyData.Get(sessionId);
                    JoinSessionLobby(sessionLobby);
                }
            }
        }

        private void HandleDataUpdate(LobbyDataUpdateEventData arg0)
        {
            // If we are tracking a part, this update is for this party and the update is a lobby update not a member update
            if (partyLobbyData != null && partyLobbyData.Data == arg0.lobby && !arg0.member.HasValue)
            {
                // Get the session ID if any is declared
                var sessionId = partyLobbyData.Data[LobbyData.DataSessionLobby];

                // If this is not null then we have a declared session lobby ID
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var sessionLobby = LobbyData.Get(sessionId);
                    JoinSessionLobby(sessionLobby);
                }
                else if (leaveOnSessionClear && m_Inspector.Data.IsValid)
                {
                    // empty session lobby so we should leave if we are in a session lobby
                    m_Inspector.Data.Leave();
                    m_Inspector.Data = CSteamID.Nil;
                }
            }
        }

        private void JoinSessionLobby(LobbyData sessionLobby)
        {
            // If its a valid lobby check if we are members of it
            if (sessionLobby.IsValid)
            {
                // If our current lobby is not the same as this lobby ... leave the current
                if (m_Inspector.Data != sessionLobby)
                {
                    if (m_Inspector.Data.IsValid)
                        m_Inspector.Data.Leave();
                }

                // If we are not already a member of this session lobby, join it
                if (!sessionLobby.IsAMember(UserData.Me))
                {
                    m_Join.Join(sessionLobby);
                }
                // We are already a member so set it as the tracked lobby on this parent
                else
                {
                    m_Inspector.Data = sessionLobby;
                }
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyJoinSessionLobby), true)]
    public class SteamLobbyJoinSessionLobbyEditor : UnityEditor.Editor
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