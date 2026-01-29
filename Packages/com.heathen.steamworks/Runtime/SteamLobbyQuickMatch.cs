#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Quick Match", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyQuickMatch : MonoBehaviour
    {
        public enum SteamLobbyType : int
        {
            Private = 0,        // only way to join the lobby is to invite to someone else
            FriendsOnly = 1,    // shows for friends or invitees, but not in lobby list
            Public = 2,         // visible for friends and in lobby list
            Invisible = 3,      // returned by search, but not visible to other friends
        }

        public enum LobbyDistanceFilter : int
        {
            Close,        // only lobbies in the same immediate region will be returned
            Default,      // only lobbies in the same region or near by regions
            Far,          // for games that don't have many latency requirements, will return lobbies about half-way around the globe
            Worldwide,    // no filtering, will match lobbies as far as India to NY (not recommended, expect multiple seconds of latency between the clients)
        }

        [SettingsField(synchronized = true)]
        /// <summary>
        /// If true the search will check if there is currently a party lobby if so it will search for a lobby enough slots for each party member, else it will obey the slots field.
        /// </summary>
        [Tooltip("If true the search will check if there is currently a party lobby if so it will search for a lobby enough slots for each party member, else it will obey the slots field.")]
        public bool partyWise = false;
        /// <summary>
        /// The type of lobby to create
        /// </summary>
        [SettingsField(header = "Quick Match")]
        [Tooltip("The type of lobby to create")]
        public SteamLobbyType type = SteamLobbyType.Public;
        [Header("Search Arguments")]
        /// <summary>
        /// The distance from teh searching user that should be considered when searching
        /// </summary>
        [SettingsField(header = "Quick Match")]
        [Tooltip("The distance from the searching user that should be considered when searching")]
        public LobbyDistanceFilter distance = LobbyDistanceFilter.Default;
        /// <summary>
        /// Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the results
        /// </summary>
        [SettingsField(header = "Quick Match")]
        [Tooltip("Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the results")]
        public List<NearFilter> nearValues = new();
        /// <summary>
        /// Metadata values that should be compared as numeric values e.g. should follow typical maths rules for concepts such as less than, greater than, etc.
        /// </summary>
        [SettingsField(header = "Quick Match")]
        [Tooltip("Metadata values that should be compared as numeric values e.g. should follow typical maths rules for concepts such as less than, greater than, etc.")]
        public List<NumericFilter> numericFilters = new();
        /// <summary>
        /// Metadata values that should be compared as strings
        /// </summary>
        [SettingsField(header = "Quick Match")]
        [Tooltip("Metadata values that should be compared as strings")]
        public List<StringFilter> stringFilters = new();

        private int slots = 1;
        private SteamLobbyData m_Inspector;
        private SteamLobbyDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Events = GetComponent<SteamLobbyDataEvents>();
        }

        public void Match()
        {
            LobbyData partyLobby = CSteamID.Nil;

            if (partyWise && LobbyData.PartyLobby(out partyLobby))
            {
                if (!partyLobby.IsOwner)
                {
                    Debug.LogWarning("Only a party lobby leader can create or join lobbies");
                    return;
                }
                else
                    slots = partyLobby.MemberCount;
            }
            else
                slots = 1;

            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter((ELobbyDistanceFilter)distance);

            if (slots > 0)
                API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(slots);

            foreach (var near in nearValues)
                API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

            foreach (var numeric in numericFilters)
                API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

            foreach (var text in stringFilters)
                API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(1);

            API.Matchmaking.Client.RequestLobbyList((r, e) =>
            {
                if (!e)
                {
                    if (r.Length > 0)
                    {
                        // We found a lobby ... try and join it
                        r[0].Join((enterLobby, enterIoError) =>
                        {
                            if (!enterIoError && enterLobby.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            {
                                m_Inspector.Data = enterLobby.Lobby;
                                if (m_Events != null)
                                    m_Events.onEnterSuccess?.Invoke(enterLobby.Lobby);

                                if (partyLobby.IsValid)
                                {
                                    partyLobby[LobbyData.DataSessionLobby] = enterLobby.Lobby.ToString();
                                }
                            }
                            else
                            {
                                // We failed to join said lobby ... so create one instead
                                LobbyData.Create((ELobbyType)type, SteamLobbyModeType.Session, slots, (createResult, createdLobby, createIoError) =>
                                {
                                    if (!createIoError && createResult == EResult.k_EResultOK)
                                    {
                                        m_Inspector.Data = createdLobby;

                                        if (m_Events != null)
                                            m_Events.onCreate?.Invoke(createdLobby);

                                        if (partyLobby.IsValid)
                                        {
                                            partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (m_Events != null)
                                            m_Events.onCreationFailure?.Invoke(createResult);
                                    }
                                });
                            }
                        });
                    }
                    else
                    {
                        // No lobby found create a new one
                        LobbyData.Create((ELobbyType)type, SteamLobbyModeType.Session, slots, (createResult, createdLobby, createIoError) =>
                        {
                            if (!createIoError && createResult == EResult.k_EResultOK)
                            {
                                m_Inspector.Data = createdLobby;

                                if (m_Events != null)
                                    m_Events.onCreate?.Invoke(createdLobby);

                                if (partyLobby.IsValid)
                                {
                                    partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                                }
                            }
                            else
                            {
                                if (m_Events != null)
                                    m_Events.onCreationFailure?.Invoke(createResult);
                            }
                        });
                    }
                }
                else
                {
                    // We failed to search for a lobby ... try and create one
                    LobbyData.Create((ELobbyType)type, SteamLobbyModeType.Session, slots, (createResult, createdLobby, createIoError) =>
                    {
                        if (!createIoError && createResult == EResult.k_EResultOK)
                        {
                            m_Inspector.Data = createdLobby;

                            if (m_Events != null)
                                m_Events.onCreate?.Invoke(createdLobby);

                            if (partyLobby.IsValid)
                            {
                                partyLobby[LobbyData.DataSessionLobby] = createdLobby.ToString();
                            }
                        }
                        else
                        {
                            if (m_Events != null)
                                m_Events.onCreationFailure?.Invoke(createResult);
                        }
                    });
                }
            });
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyQuickMatch), true)]
    public class SteamLobbyQuickMatchEditor : UnityEditor.Editor
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