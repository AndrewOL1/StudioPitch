#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Steamworks;
using Heathen.SteamworksIntegration.API;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/steamworks/features/lobby/unity-lobby#lobby-manager")]
    public class LobbyManager : MonoBehaviour
    {
        public enum ManagedLobbyEvents
        {
            AuthenticationSessionResults,
            LobbyChatMessageReceived,
            LobbyCreationFailed,
            LobbyCreationSuccess,
            LobbyInviteReceived,
            LobbyJoinFailure,
            LobbyJoinSuccess,
            LobbyLeave,
            MetadataUpdated,
            OtherUserLeft,
            OtherUserJoined,
            QuickMatchFailed,
            SearchResultsReady,
            SessionConnectionUpdated,
            YouAreAskedToLeave,
        }

        [SerializeField]
        private List<ManagedLobbyEvents> m_Delegates;

        [Tooltip("The values to be used when searching for a lobby")]
        public SearchArguments searchArguments = new();
        [Tooltip("The values to be used when creating a new lobby")]
        public CreateArguments createArguments = new();
        /// <summary>
        /// The Rich Presence fields to be set when a lobby is created.
        /// </summary>
        [Tooltip("The Rich Presence fields to be set when joining a lobby.\nDoes not apply to creating a lobby, use the Create Arguments for that.")]
        public List<StringKeyValuePair> richPresenceFields = new();
        [Header("Events")]
        public LobbyInviteEvent evtLobbyInvite;
        public LobbyChatMsgEvent evtChatMsgReceived;
        public LobbyDataListEvent evtFound;
        /// <summary>
        /// Occurs when the local user enters a lobby as a response to a join
        /// </summary>
        public LobbyDataEvent evtEnterSuccess;
        /// <summary>
        /// Occurs when the local user tried but failed to enter a lobby
        /// </summary>
        public LobbyResponseEvent evtEnterFailed;
        /// <summary>
        /// Occurs when the local user creates a lobby
        /// </summary>
        public LobbyDataEvent evtCreated;
        /// <summary>
        /// Occurs when the local user tried but failed to create a lobby
        /// </summary>
        public EResultEvent evtCreateFailed;
        /// <summary>
        /// Occurs when the local user attempts to quick match but fails to find a match or resolve the quick match
        /// </summary>
        public UnityEvent evtQuickMatchFailed;
        /// <summary>
        /// Occurs when any data is updated on the lobby be that lobby metadata or a members metadata
        /// </summary>
        public LobbyDataUpdateEvent evtDataUpdated;
        /// <summary>
        /// Occurs when the local user leaves the managed lobby
        /// </summary>
        public UnityEvent evtLeave;
        /// <summary>
        /// Occurs when the local user is asked to leave the lobby via Heathen's Kick system
        /// </summary>
        public UnityEvent evtAskedToLeave;
        /// <summary>
        /// Occurs when the <see cref="GameServer"/> information is first set on the lobby
        /// </summary>
        public GameServerSetEvent evtGameCreated;
        /// <summary>
        /// Occurs when the local user is a member of a lobby and a new member joins that lobby
        /// </summary>
        public UserDataEvent evtUserJoined;
        /// <summary>
        /// Occurs when the local user is a member of a lobby and another fellow member leaves the lobby
        /// </summary>
        public UserLeaveEvent evtUserLeft;
        /// <summary>
        /// Occurs when the local user who is the owner of the lobby receives and starts an auth session request for a user
        /// </summary>
        public LobbyAuthenticaitonSessionEvent evtAuthenticationSessionResult;
        /// <summary>
        /// The lobby this manager is currently managing
        /// </summary>
        /// <remarks>
        /// This will automatically be updated when you use the Lobby Manager to create, join or leave a lobby.
        /// If you manually create, join or leave a lobby you must update this field your self.
        /// To clear the value assign <see cref="CSteamID.Nil"/>
        /// </remarks>
        public LobbyData Data
        {
            get;
            set;
        }
        /// <summary>
        /// Returns true if the <see cref="Data"/> value is populated and resolves to a non-empty lobby
        /// </summary>
        public bool HasLobby => Data != CSteamID.Nil.m_SteamID && SteamMatchmaking.GetNumLobbyMembers(Data) > 0;
        /// <summary>
        /// Returns true if the local user is the owner of the managed lobby
        /// </summary>
        public bool IsPlayerOwner => HasLobby && Data.IsOwner;
        /// <summary>
        /// Returns true if all members in the lobby have indicated that they are Read via the Heathen Ready Check system
        /// </summary>
        public bool AllPlayersReady => HasLobby && Data.AllPlayersReady;
        /// <summary>
        /// Is the local player ready
        /// </summary>
        /// <remarks>
        /// You can assign this value to update the local player's LobbyMember accordingly for the Heathen Ready Check system
        /// </remarks>
        public bool IsPlayerReady
        {
            get => HasLobby && API.Matchmaking.Client.GetLobbyMemberData(Data, API.User.Client.Id, LobbyData.DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(Data, LobbyData.DataReady, value.ToString().ToLower());
        }
        /// <summary>
        /// Returns true when the managed lobby is full e.g. unable to take more members
        /// </summary>
        public bool Full => HasLobby && Data.Full;
        [Obsolete("Use " + nameof(MaxMembers) + " instead.")]
        public int Slots => HasLobby ? SteamMatchmaking.GetLobbyMemberLimit(Data) : 0;
        public int MemberCount => HasLobby ? SteamMatchmaking.GetNumLobbyMembers(Data) : 0;
        /// <summary>
        /// Returns true if the Heathen TypeSet feature has been populated to indicate the type of lobby this is
        /// </summary>
        public bool IsTypeSet => HasLobby && Data.IsTypeSet;
        /// <summary>
        /// Returns the type of lobby this is, this is a feature of Heathen's Lobby tools. Valve does not actually expose this so this will only work for lobbies
        /// created by Heathen's tools such as <see cref="SteamLobbyData"/> and of course <see cref="API.Matchmaking.Client"/>
        /// </summary>
        public ELobbyType Type
        {
            get => Data.Type;
            set
            {
                var l = Data;
                l.Type = value;
            }
        }
        /// <summary>
        /// The max number of members this lobby supports
        /// </summary>
        /// <remarks>
        /// The owner of the lobby can set this value to update the max allowed
        /// </remarks>
        public int MaxMembers
        {
            get => HasLobby ? API.Matchmaking.Client.GetLobbyMemberLimit(new CSteamID(Data)) : 0;
            set => API.Matchmaking.Client.SetLobbyMemberLimit(new CSteamID(Data), value);
        }
        /// <summary>
        /// Does the managed lobby have game server data set on it?
        /// </summary>
        /// <remarks>
        /// <see cref="LobbyData.SetGameServer"/> for more information
        /// </remarks>
        public bool HasServer => SteamMatchmaking.GetLobbyGameServer(Data, out _, out _, out _);
        /// <summary>
        /// The game server information set on the managed lobby if any
        /// </summary>
        public LobbyGameServer GameServer => API.Matchmaking.Client.GetLobbyGameServer(Data);

        public string this[string key]
        {
            get => Data[key];
            set
            {
                var lobby = Data;
                lobby[key] = value;
            }
        }

        public LobbyMemberData this[UserData user] => Data[user];

        private void OnEnable()
        {
            API.Matchmaking.Client.OnLobbyAskedToLeave.AddListener(HandleAskedToLeave);
            API.Matchmaking.Client.OnLobbyDataUpdate.AddListener(HandleLobbyDataUpdate);
            API.Matchmaking.Client.OnLobbyLeave.AddListener(HandleLobbyLeave);
            API.Matchmaking.Client.OnLobbyGameCreated.AddListener(HandleGameServerSet);
            API.Matchmaking.Client.OnLobbyChatUpdate.AddListener(HandleChatUpdate);
            API.Matchmaking.Client.OnLobbyAuthenticationRequest.AddListener(HandleAuthRequest);
            API.Matchmaking.Client.OnLobbyChatMsg.AddListener(HandleChatMessage);
            API.Matchmaking.Client.OnLobbyInvite.AddListener(evtLobbyInvite.Invoke);
        }

        private void OnDisable()
        {
            API.Matchmaking.Client.OnLobbyAskedToLeave.RemoveListener(HandleAskedToLeave);
            API.Matchmaking.Client.OnLobbyDataUpdate.RemoveListener(HandleLobbyDataUpdate);
            API.Matchmaking.Client.OnLobbyLeave.RemoveListener(HandleLobbyLeave);
            API.Matchmaking.Client.OnLobbyGameCreated.RemoveListener(HandleGameServerSet);
            API.Matchmaking.Client.OnLobbyChatUpdate.RemoveListener(HandleChatUpdate);
            API.Matchmaking.Client.OnLobbyAuthenticationRequest.RemoveListener(HandleAuthRequest);
            API.Matchmaking.Client.OnLobbyChatMsg.RemoveListener(HandleChatMessage);
            API.Matchmaking.Client.OnLobbyInvite.RemoveListener(evtLobbyInvite.Invoke);
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == Data)
            {
                evtChatMsgReceived.Invoke(message);
            }
        }

        private void HandleAuthRequest(LobbyData lobby, UserData sender, byte[] ticket, byte[] inventory)
        {
            if (lobby == Data)
            {
                Authentication.BeginAuthSession(ticket, sender, (session) =>
                {
                    evtAuthenticationSessionResult.Invoke(session, inventory);
                });
            }
        }

        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Data)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    evtUserJoined?.Invoke(arg0.m_ulSteamIDUserChanged);
                else
                    evtUserLeft?.Invoke(new UserLobbyLeaveData { user = arg0.m_ulSteamIDUserChanged, state = state });
            }
        }

        private void HandleGameServerSet(LobbyGameCreated_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Data)
                evtGameCreated.Invoke(GameServer);
        }

        private void HandleLobbyLeave(LobbyData arg0)
        {
            if (arg0 == Data)
            {
                Data = default;
                evtLeave.Invoke();
            }
        }

        private void HandleAskedToLeave(LobbyData arg0)
        {
            if (arg0 == Data)
                evtAskedToLeave.Invoke();
        }

        private void HandleLobbyDataUpdate(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == Data)
                evtDataUpdated.Invoke(arg0);
        }

        /// <summary>
        /// Changes the type of the current lobby if any
        /// </summary>
        /// <remarks>
        /// This will also update the type in the <see cref="createArguments"/> record
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool SetType(ELobbyType type)
        {
            createArguments.type = type;
            return API.Matchmaking.Client.SetLobbyType(Data, type);
        }
        /// <summary>
        /// Sets the lobby joinable or not
        /// </summary>
        /// <param name="makeJoinable"></param>
        /// <returns></returns>
        public bool SetJoinable(bool makeJoinable) => HasLobby && Data.SetJoinable(makeJoinable);
        /// <summary>
        /// Searches for a match based on <see cref="searchArguments"/>, if none is found it will create a lobby matching the <see cref="createArguments"/>
        /// </summary>
        public void QuickMatch()
        {
            if (HasLobby)
            {
                Data.Leave();
                Data = CSteamID.Nil.m_SteamID;
            }

            LobbyData.QuickMatch(searchArguments, createArguments, (result, lobby, ioError) =>
            {
                if (!ioError && result == EResult.k_EResultOK)
                {
                    Data = lobby;
                    if (lobby.IsOwner)
                    {
                        evtCreated?.Invoke(lobby);
                    }
                    else
                    {
                        evtEnterSuccess?.Invoke(lobby);
                    }
                }
                else
                {
                    switch (result)
                    {
                        case EResult.k_EResultBanned:
                            evtEnterFailed?.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned); break;
                        case EResult.k_EResultLimitExceeded:
                            evtEnterFailed?.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseFull); break;
                        case EResult.k_EResultLimitedUserAccount:
                            evtEnterFailed?.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited); break;
                        case EResult.k_EResultRateLimitExceeded:
                            evtEnterFailed?.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded); break;
                        default:
                            evtEnterFailed?.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseError); break;
                    }
                }
            });
        }
        /// <summary>
        /// Creates a new lobby with the data found in <see cref="createArguments"/>
        /// </summary>
        public void Create() => Create(null);
        public void Create(string name, ELobbyType type, int slots, params MetadataTemplate[] metadata)
        {
            createArguments.name = name;
            createArguments.type = type;
            createArguments.slots = slots;
            createArguments.metadata.Clear();
            createArguments.metadata.AddRange(metadata);
            createArguments.richPresenceFields.Clear();
            Create(null);
        }
        public void Create(ELobbyType type, int slots, params MetadataTemplate[] metadata)
        {
            createArguments.name = string.Empty;
            createArguments.type = type;
            createArguments.slots = slots;
            createArguments.metadata.Clear();
            createArguments.metadata.AddRange(metadata);
            createArguments.richPresenceFields.Clear();
            Create(null);
        }
        /// <summary>
        /// Creates a new lobby with the data found in <see cref="createArguments"/> and invokes the callback when complete
        /// </summary>
        public void Create(Action<EResult, LobbyData, bool> callback)
        {
            if (HasLobby)
            {
                Data.Leave();
                Data = CSteamID.Nil.m_SteamID;
            }

            LobbyData.Create(createArguments, (result, lobby, ioError) =>
            {
                if (!ioError)
                {
                    if (result == EResult.k_EResultOK)
                    {
                        if (API.App.isDebugging)
                            Debug.Log("New lobby created.");

                        Data = lobby;

                        lobby[LobbyData.DataName] = createArguments.name;
                        foreach (var data in createArguments.metadata)
                        {
                            if (data.value.Contains("@[") && data.value.Contains("]"))
                            {
                                var workingString = data.value;
                                while (API.Utilities.FindToken("@[", "]", workingString, out var resultString))
                                {
                                    switch (resultString)
                                    {
                                        case "@[userName]":
                                            workingString.Replace(resultString, UserData.Me.Name);
                                            break;
                                        case "@[userLevel]":
                                            workingString.Replace(resultString, UserData.Me.Level.ToString());
                                            break;
                                        default:
                                            workingString.Replace(resultString, "");
                                            break;
                                    }
                                }
                            }
                            else
                                lobby[data.key] = data.value;
                        }

                        if (createArguments != null
                        && createArguments.richPresenceFields != null)
                            foreach (var kvp in createArguments.richPresenceFields)
                            {
                                if (kvp.value.Contains("@[") && kvp.value.Contains("]"))
                                {
                                    var workingString = kvp.value;
                                    while (API.Utilities.FindToken("@[", "]", workingString, out var resultString))
                                    {
                                        switch (resultString)
                                        {
                                            case "@[lobbyId]":
                                                workingString.Replace(resultString, lobby.ToString());
                                                break;
                                            default:
                                                workingString.Replace(resultString, lobby[resultString[2..^1]]);
                                                break;
                                        }
                                    }
                                }
                                else
                                    Friends.Client.SetRichPresence(kvp.key, kvp.value);
                            }

                        evtCreated?.Invoke(lobby);
                    }
                    else
                    {
                        Debug.Log($"No lobby created Steam API response code: {result}");
                        evtCreateFailed?.Invoke(result);
                    }
                }
                else
                {
                    Debug.LogError("Lobby creation failed with message: IOFailure\nSteam API responded with a general IO Failure.");
                    evtCreateFailed?.Invoke(EResult.k_EResultIOFailure);
                }

                callback?.Invoke(result, lobby, ioError);
            });
        }
        /// <summary>
        /// Searches for lobbies that match the <see cref="searchArguments"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remember Lobbies are a matchmaking feature, the first lobby returned is generally he best, lobby search is not intended to return all possible results simply the best matching options.
        /// </para>
        /// </remarks>
        /// <param name="maxResults">The maximum number of lobbies to return. lower values are better.</param>
        public void Search(int maxResults)
        {
            if (maxResults <= 0)
                return;

            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(searchArguments.distance);

            if (searchArguments.slots > 0)
                API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(searchArguments.slots);

            foreach (var near in searchArguments.nearValues)
                API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

            foreach (var numeric in searchArguments.numericFilters)
                API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

            foreach (var text in searchArguments.stringFilters)
                API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResults);

            API.Matchmaking.Client.RequestLobbyList((r, e) =>
            {
                if (!e)
                {
                    evtFound?.Invoke(r);
                }
                else
                {
                    evtFound?.Invoke(new LobbyData[0]);
                }
            });
        }
        /// <summary>
        /// Joins the indicated steam lobby
        /// </summary>
        /// <param name="lobby"></param>
        public void Join(LobbyData lobby)
        {
            if (HasLobby)
            {
                Data.Leave();
                Data = CSteamID.Nil.m_SteamID;
            }

            lobby.Join(LobbyJoined);
        }

        public void Join(ulong lobby)
        {
            if (HasLobby)
            {
                Data.Leave();
                Data = CSteamID.Nil.m_SteamID;
            }

            LobbyData.Join(lobby, LobbyJoined);
        }

        public void Join(string lobbyIdAsString)
        {
            if (HasLobby)
            {
                Data.Leave();
                Data = CSteamID.Nil.m_SteamID;
            }

            LobbyData.Join(lobbyIdAsString, LobbyJoined);
        }

        private void LobbyJoined(LobbyEnter enterData, bool ioError)
        {
            if (!ioError)
            {
                if (enterData.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                {
                    if (API.App.isDebugging)
                        Debug.Log("Joined lobby: " + enterData.Lobby.ToString());

                    Data = enterData.Lobby;

                    if (richPresenceFields != null)
                        foreach (var kvp in richPresenceFields)
                        {
                            if (kvp.value.Contains("@[") && kvp.value.Contains("]"))
                            {
                                var workingString = kvp.value;
                                while (API.Utilities.FindToken("@[", "]", workingString, out var resultString))
                                {
                                    switch (resultString)
                                    {
                                        case "@[lobbyId]":
                                            workingString.Replace(resultString, enterData.Lobby.ToString());
                                            break;
                                        default:
                                            workingString.Replace(resultString, enterData.Lobby[resultString[2..^1]]);
                                            break;
                                    }
                                }
                            }
                            else
                                Friends.Client.SetRichPresence(kvp.key, kvp.value);
                        }

                    evtEnterSuccess.Invoke(enterData.Lobby);
                }
                else
                    evtEnterFailed.Invoke(enterData.Response);
            }
            else
                evtEnterFailed.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseError);
        }

        public void Leave()
        {
            Data.Leave();
            Data = CSteamID.Nil.m_SteamID;
        }
        public bool SetLobbyData(string key, string value) => API.Matchmaking.Client.SetLobbyData(Data, key, value);
        public void SetMemberData(string key, string value) => API.Matchmaking.Client.SetLobbyMemberData(Data, key, value);
        public LobbyMemberData GetLobbyMember(UserData member) => new LobbyMemberData { lobby = Data, user = member };
        public string GetLobbyData(string key) => API.Matchmaking.Client.GetLobbyData(Data, key);
        public string GetMemberData(UserData member, string key) => API.Matchmaking.Client.GetLobbyMemberData(Data, member, key);
        public bool IsMemberReady(UserData member) => API.Matchmaking.Client.GetLobbyMemberData(Data, member, LobbyData.DataReady) == "true";
        public void KickMember(UserData member) => Data.KickMember(member);
        public void Invite(UserData user) => API.Matchmaking.Client.InviteUserToLobby(Data, user);
        public void Invite(uint FriendId) => API.Matchmaking.Client.InviteUserToLobby(Data, UserData.Get(FriendId));
        public LobbyMemberData[] Members => API.Matchmaking.Client.GetLobbyMembers(Data);
        public void Authenticate(Action<AuthenticationTicket, bool> callback) => Data.Authenticate(callback);
        public bool Authenticate(LobbyMessagePayload data) => Data.Authenticate(data);
        public bool SendChatMessage(string message) => Data.SendChatMessage(message);
        public bool SendChatMessage(byte[] data) => Data.SendChatMessage(data);
        public bool SendChatMessage(object jsonObject)
        {
            return SendChatMessage(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonObject)));
        }

        /// <summary>
        /// The same as SendChatMessage, this only exists for use in Unity Inspector where overloads don't play nice with some editor features
        /// </summary>
        /// <param name="message"></param>
        public void SendChatMessageString(string message) => SendChatMessage(message);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LobbyManager), true)]
    public class LobbyInspectorEditor : Editor
    {
        SerializedProperty m_DelegatesProperty;
        SerializedProperty m_searchProperty;
        SerializedProperty m_createProperty;
        SerializedProperty m_richPresenceProperty;

        GUIContent m_IconToolbarMinus;
        GUIContent m_EventIDName;
        GUIContent[] m_EventTypes;
        GUIContent m_AddButtonContent;

        protected virtual void OnEnable()
        {
            m_DelegatesProperty = serializedObject.FindProperty("m_Delegates");
            m_searchProperty = serializedObject.FindProperty(nameof(LobbyManager.searchArguments));
            m_createProperty = serializedObject.FindProperty(nameof(LobbyManager.createArguments));
            m_richPresenceProperty = serializedObject.FindProperty(nameof(LobbyManager.richPresenceFields));
            m_AddButtonContent = new GUIContent("Add New Event Type");
            m_EventIDName = new GUIContent("");
            // Have to create a copy since otherwise the tooltip will be overwritten.
            m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
            m_IconToolbarMinus.tooltip = "Remove all events in this list.";

            string[] eventNames = Enum.GetNames(typeof(LobbyManager.ManagedLobbyEvents));
            m_EventTypes = new GUIContent[eventNames.Length];
            for (int i = 0; i < eventNames.Length; ++i)
            {
                m_EventTypes[i] = new GUIContent(eventNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_searchProperty, true);
            EditorGUILayout.PropertyField(m_createProperty, true);
            EditorGUILayout.PropertyField(m_richPresenceProperty, true);

            int toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

            for (int i = 0; i < m_DelegatesProperty.arraySize; ++i)
            {
                SerializedProperty delegateProperty = m_DelegatesProperty.GetArrayElementAtIndex(i);
                m_EventIDName.text = delegateProperty.enumDisplayNames[delegateProperty.enumValueIndex];

                switch ((LobbyManager.ManagedLobbyEvents)delegateProperty.enumValueIndex)
                {
                    case LobbyManager.ManagedLobbyEvents.AuthenticationSessionResults:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtAuthenticationSessionResult)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyJoinFailure:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtEnterFailed)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyJoinSuccess:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtEnterSuccess)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyChatMessageReceived:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtChatMsgReceived)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyCreationFailed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtCreateFailed)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.MetadataUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtDataUpdated)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyCreationSuccess:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtCreated)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.OtherUserJoined:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtUserJoined)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.OtherUserLeft:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtUserLeft)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.QuickMatchFailed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtQuickMatchFailed)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.SearchResultsReady:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtFound)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.SessionConnectionUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtGameCreated)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.YouAreAskedToLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtAskedToLeave)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyInviteReceived:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtLobbyInvite)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtLeave)), m_EventIDName);
                        break;
                }

                Rect callbackRect = GUILayoutUtility.GetLastRect();

                Rect removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
                if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none))
                {
                    toBeRemovedEntry = i;
                }

                EditorGUILayout.Space();
            }

            if (toBeRemovedEntry > -1)
            {
                RemoveEntry(toBeRemovedEntry);
            }

            Rect btPosition = GUILayoutUtility.GetRect(m_AddButtonContent, GUI.skin.button);
            const float addButtonWidth = 200f;
            btPosition.x = btPosition.x + (btPosition.width - addButtonWidth) / 2;
            btPosition.width = addButtonWidth;
            if (GUI.Button(btPosition, m_AddButtonContent))
            {
                ShowAddTriggerMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(int toBeRemovedEntry)
        {
            m_DelegatesProperty.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        void ShowAddTriggerMenu()
        {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < m_EventTypes.Length; ++i)
            {
                bool active = true;

                // Check if we already have a Entry for the current eventType, if so, disable it
                for (int p = 0; p < m_DelegatesProperty.arraySize; ++p)
                {
                    SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(p);
                    if (delegateEntry.enumValueIndex == i)
                    {
                        active = false;
                    }
                }
                if (active)
                    menu.AddItem(m_EventTypes[i], false, OnAddNewSelected, i);
                else
                    menu.AddDisabledItem(m_EventTypes[i]);
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void OnAddNewSelected(object index)
        {
            int selected = (int)index;

            m_DelegatesProperty.arraySize += 1;
            SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(m_DelegatesProperty.arraySize - 1);
            delegateEntry.enumValueIndex = selected;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif