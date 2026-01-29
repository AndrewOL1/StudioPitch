#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Create", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyCreate : MonoBehaviour
    {
        public enum SteamLobbyType : int
        {
            Private = 0,        // only way to join the lobby is to invite to someone else
            FriendsOnly = 1,    // shows for friends or invitees, but not in lobby list
            Public = 2,         // visible for friends and in lobby list
            Invisible = 3,      // returned by search, but not visible to other friends
        }

        /// <summary>
        /// If true and creating a Party it will leave any existing lobby first, if true when creating a session it will notify any existing party of the new session lobby.
        /// </summary>
        [SettingsField(synchronized = true)]
        [Tooltip("If true and creating a Party it will leave any existing lobby first, if true when creating a session it will notify any existing party of the new session lobby.")]
        public bool partyWise = false;
        /// <summary>
        /// How will this lobby be used? This is an optional feature. If set to Party or Session then features of the LobbyData object can be used in code to fetch the created lobby such as LobbyData.GetGroup(...)
        /// </summary>
        [SettingsField(header = "Create")]
        [Tooltip("How will this lobby be used? This is an optional feature. If set to Party or Session then features of the LobbyData object can be used in code to fetch the created lobby such as LobbyData.GetGroup(...)")]
        public SteamLobbyModeType usageHint = SteamLobbyModeType.Session;
        /// <summary>
        /// The number of slots the newly created lobby should have
        /// </summary>
        [SettingsField(header = "Create")]
        [Tooltip("The number of slots the newly created lobby should have")]
        public int slots;
        /// <summary>
        /// The type of lobby to create
        /// </summary>
        [SettingsField(header = "Create")]
        [Tooltip("The type of lobby to create")]
        public SteamLobbyType type;
        
        private SteamLobbyData m_Inspector;
        private SteamLobbyDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Events = GetComponent<SteamLobbyDataEvents>();
        }

        public void Create()
        {
            LobbyData partyLobby = CSteamID.Nil;

            if (partyWise && LobbyData.PartyLobby(out partyLobby) && usageHint == SteamLobbyModeType.Party)
            {
                partyLobby.Leave();
                partyLobby = CSteamID.Nil;
            }

            if(partyLobby.IsValid && !partyLobby.IsOwner)
            {
                Debug.LogWarning("Only a party lobby leader can create lobbies");
                return;
            }

            LobbyData.Create((ELobbyType)type, usageHint, slots, (createResult, createdLobby, createIoError) =>
            {
                if (!createIoError && createResult == EResult.k_EResultOK)
                {
                    m_Inspector.Data = createdLobby;

                    if(m_Events != null)
                        m_Events.onCreate?.Invoke(createdLobby);

                    if (partyLobby.IsValid && usageHint == SteamLobbyModeType.Session)
                    {
                        if(type == SteamLobbyType.Private)
                        {
                            foreach(var member in partyLobby.Members)
                            {
                                if (!member.user.IsMe)
                                    member.user.InviteToLobby(createdLobby);
                            }
                        }
                        else
                            partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                    }
                }
                else
                {
                    if(m_Events != null)
                        m_Events.onCreationFailure?.Invoke(createResult);
                }
            });
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyCreate), true)]
    public class SteamLobbyCreateEditor : UnityEditor.Editor
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