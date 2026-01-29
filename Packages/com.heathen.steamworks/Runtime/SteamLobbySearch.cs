#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-search")]
    [AddComponentMenu("Steamworks/Lobby Search")]
    public class SteamLobbySearch : MonoBehaviour
    {
        [Header("Configuration")]
        /// <summary>
        /// If true the search will check if there is currently a party lobby if so it will search for a lobby enough slots for each party member, else it will obey the slots field.
        /// </summary>
        [Tooltip("If true the search will check if there is currently a party lobby if so it will search for a lobby enough slots for each party member, else it will obey the slots field.")]
        public bool partyWise = false;
        /// <summary>
        /// If less than or equal to 0 then we wont use the open slot filter
        /// </summary>
        [Tooltip("If less than or equal to 0 then we wont use the open slot filter")]
        public int slots = 0;
        /// <summary>
        /// The distance from teh searching user that should be considered when searching
        /// </summary>
        [Tooltip("The distance from the searching user that should be considered when searching")]
        public ELobbyDistanceFilter distance = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
        /// <summary>
        /// Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the results
        /// </summary>
        [Tooltip("Metadata values that should be used to sort the results e.g. values `closer` to these values will be weighted higher in the results")]
        public List<NearFilter> nearValues = new();
        /// <summary>
        /// Metadata values that should be compared as numeric values e.g. should follow typical maths rules for concepts such as less than, greater than, etc.
        /// </summary>
        [Tooltip("Metadata values that should be compared as numeric values e.g. should follow typical maths rules for concepts such as less than, greater than, etc.")]
        public List<NumericFilter> numericFilters = new();
        /// <summary>
        /// Metadata values that should be compared as strings
        /// </summary>
        [Tooltip("Metadata values that should be compared as strings")]
        public List<StringFilter> stringFilters = new();
        [Range(1, 50)]
        public int maxResults = 50;

        [Header("Elements")]
        public SteamLobbyData template;
        public Transform content;

        [Header("Events")]
        [Tooltip("Invoked when the lobby search completes. Returns an array of LobbyData.")]
        public LobbyDataListEvent OnLobbiesFound;

        private readonly List<SteamLobbyData> lobbies = new();

        /// <summary>
        /// Searches for lobbies that match the <see cref="searchArguments"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remember Lobbies are a matchmaking feature, the first lobby returned is generally he best, lobby search is not intended to return all possible results simply the best matching options.
        /// </para>
        /// </remarks>
        /// <param name="maxResults">The maximum number of lobbies to return. lower values are better.</param>
        public void Search()
        {
            if (maxResults <= 0)
                return;

            if (partyWise && LobbyData.PartyLobby(out var lobby))
            {
                if (lobby.IsOwner)
                    slots = lobby.MemberCount;
                else
                {
                    Debug.LogWarning("Only the owner of a party can search for a match.");
                    return;
                }
            }

            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(distance);

            if (slots > 0)
                API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(slots);

            foreach (var near in nearValues)
                API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

            foreach (var numeric in numericFilters)
                API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

            foreach (var text in stringFilters)
                API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResults);

            API.Matchmaking.Client.RequestLobbyList((r, e) =>
            {
                if (!e)
                {
                    foreach(var lobby in lobbies)
                    {
                        Destroy(lobby.gameObject);
                    }

                    lobbies.Clear();

                    foreach(var lobby in r)
                    {
                        var comp = Instantiate(template, content); 
                        comp.Data = lobby;
                        lobbies.Add(comp);
                    }

                    OnLobbiesFound?.Invoke(r);
                }
                else
                {
                    OnLobbiesFound?.Invoke(new LobbyData[0]);
                }
            });
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbySearch), true)]
    public class SteamLobbySearchEditor : UnityEditor.Editor
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