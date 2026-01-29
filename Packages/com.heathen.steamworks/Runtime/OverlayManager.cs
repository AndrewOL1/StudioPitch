#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using Steamworks;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.CodeDom;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Manages the Steam Overlay and exposes its related events and settings.
    /// </summary>
    [AddComponentMenu("Steamworks/Overlay")]
    [HelpURL("https://kb.heathen.group/steam/features/overlay")]
    [DisallowMultipleComponent]
    public class OverlayManager : MonoBehaviour
    {
        public enum ManagedEvents
        {
            JoinGameRequested,
            LobbyInviteAccepted,
            OverlayActivated,
            ServerChangeRequested,   
        }

        [SerializeField]
        private List<ManagedEvents> m_Delegates;

        [SerializeField]
        private ENotificationPosition notificationPosition = ENotificationPosition.k_EPositionBottomRight;
        [SerializeField]
        private Vector2Int notificationInset = Vector2Int.zero;

        public ENotificationPosition NotificationPosition
        {
            get => API.Overlay.Client.NotificationPosition;
            set => API.Overlay.Client.NotificationPosition = value;
        }
        public Vector2Int NotificationInset
        {
            get => API.Overlay.Client.NotificationInset;
            set => API.Overlay.Client.NotificationInset = value;
        }
        public bool IsShowing => API.Overlay.Client.IsShowing;
        public bool IsEnabled => API.Overlay.Client.IsEnabled;

        public GameOverlayActivatedEvent evtOverlayActivated;
        public GameLobbyJoinRequestedEvent evtGameLobbyJoinRequested;
        public GameServerChangeRequestedEvent evtGameServerChangeRequested;
        public GameRichPresenceJoinRequestedEvent evtRichPresenceJoinRequested;


        private void OnEnable()
        {
            if (API.App.Initialized)
                EnabledProcess();
            else
            {
                API.App.onSteamInitialized.AddListener(EnabledProcess);
            }
        }

        private void EnabledProcess()
        {
            NotificationPosition = notificationPosition;
            NotificationInset = notificationInset;
            API.Overlay.Client.OnGameOverlayActivated.AddListener(evtOverlayActivated.Invoke);
            API.Overlay.Client.OnGameServerChangeRequested.AddListener(evtGameServerChangeRequested.Invoke);
            API.Overlay.Client.OnGameLobbyJoinRequested.AddListener(evtGameLobbyJoinRequested.Invoke);
            API.Overlay.Client.OnGameRichPresenceJoinRequested.AddListener(evtRichPresenceJoinRequested.Invoke);
        }

        private void OnDisable()
        {
            API.Overlay.Client.OnGameOverlayActivated.RemoveListener(evtOverlayActivated.Invoke);
            API.Overlay.Client.OnGameServerChangeRequested.RemoveListener(evtGameServerChangeRequested.Invoke);
            API.Overlay.Client.OnGameLobbyJoinRequested.RemoveListener(evtGameLobbyJoinRequested.Invoke);
            API.Overlay.Client.OnGameRichPresenceJoinRequested.RemoveListener(evtRichPresenceJoinRequested.Invoke);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (API.App.Initialized)
            {
                if (notificationPosition != API.Overlay.Client.NotificationPosition)
                {
                    notificationPosition = NotificationPosition;
                    Debug.LogWarning("Notification Position cannot be updated from the inspector at runtime.");
                }
                if (notificationInset != API.Overlay.Client.NotificationInset)
                {
                    notificationInset = NotificationInset;
                    Debug.LogWarning("Notification Insert cannot be updated from the inspector at runtime.");
                }
            }
        }
#endif

        /// <summary>
        /// Activates the Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
        public void OpenDialogName(string dialog) => API.Overlay.Client.Activate(dialog);
        /// <summary>
        /// Activates the Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
        public void OpenDialog(OverlayDialog dialog) => API.Overlay.Client.Activate(dialog);
        /// <summary>
        /// Activates the Steam Overlay to open the invite dialog. Invitations sent from this dialog will be for the provided lobby.
        /// </summary>
        /// <param name="lobbyId">The Steam ID of the lobby that selected users will be invited to.</param>
        public void OpenLobbyInvite(LobbyData lobbyId) => API.Overlay.Client.ActivateInviteDialog(lobbyId);
        public void OpenLobbyInvite(SteamLobbyData lobby)
        {
            
            if (lobby != null && lobby.Data.IsValid)
                API.Overlay.Client.ActivateInviteDialog(lobby.Data);
        }
        public void OpenConnectStringInvite(string connectionString) => API.Overlay.Client.ActivateInviteDialog(connectionString);
        public void OpenRemotePlayInvite(LobbyData lobbyId) => API.Overlay.Client.ActivateRemotePlayInviteDialog(lobbyId);
        public void OpenRemotePlayInvite(SteamLobbyData lobby)
        {
            if (lobby != null && lobby.Data.IsValid)
                API.Overlay.Client.ActivateRemotePlayInviteDialog(lobby.Data);
        }
        public void OpenStore(int appId) => OpenStore(Convert.ToUInt32(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        public void OpenStoreAddToCart(int appId) => OpenStore(Convert.ToUInt32(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCart);
        public void OpenStoreAddToCartAndShow(int appId) => OpenStore(Convert.ToUInt32(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
        /// <summary>
        /// Activates the Steam Overlay to the Steam store page for the provided app.
        /// </summary>
        /// <param name="appID">The app ID to show the store page of.</param>
        /// <param name="flag">Flags to modify the behavior when the page opens.</param>
        public void OpenStore(AppData appID, EOverlayToStoreFlag flag) => API.Overlay.Client.Activate(appID, flag);
        public void OpenUserProfile(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("steamid", user.Data);
        }
        public void OpenUserChat(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("chat", user.Data);
        }
        public void OpenUserJoinTrade(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("jointrade", user.Data);
        }
        public void OpenUserStats(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("stats", user.Data);
        }
        public void OpenUserAchievements(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("achievements", user.Data);
        }
        public void OpenUserAddFriend(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("friendadd", user.Data);
        }
        public void OpenUserRemoveFriend(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("friendremove", user.Data);
        }
        public void OpenUserAcceptFriendRequest(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("friendrequestaccept", user.Data);
        }
        public void OpenUserIgnoreFriendRequest(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                API.Overlay.Client.Activate("friendrequestignore", user.Data);
        }
        /// <summary>
        /// Activates Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open.</param>
        /// <param name="steamId">The Steam ID of the context to open this dialog to.</param>
        public void OpenUser(string dialog, CSteamID steamId) => API.Overlay.Client.Activate(dialog, steamId);
        /// <summary>
        /// Activates Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open.</param>
        /// <param name="steamId">The Steam ID of the context to open this dialog to.</param>
        public void OpenUser(FriendDialog dialog, CSteamID steamId) => API.Overlay.Client.Activate(dialog.ToString(), steamId);
        /// <summary>
        /// Activates Steam Overlay web browser directly to the specified URL.
        /// </summary>
        /// <param name="url">The webpage to open. (A fully qualified address with the protocol is required, e.g. "http://www.steampowered.com")</param>
        public void OpenWebPage(string url) => API.Overlay.Client.ActivateWebPage(url);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OverlayManager), true)]
    public class OverlayManagerEditor : Editor
    {
        SerializedProperty m_DelegatesProperty;
        SerializedProperty m_notificationPosition;
        SerializedProperty m_notificationInset;

        GUIContent m_IconToolbarMinus;
        GUIContent m_EventIDName;
        GUIContent[] m_EventTypes;
        GUIContent m_AddButtonContent;

        protected virtual void OnEnable()
        {
            m_DelegatesProperty = serializedObject.FindProperty("m_Delegates");
            m_notificationPosition = serializedObject.FindProperty("notificationPosition");
            m_notificationInset = serializedObject.FindProperty("notificationInset");
            m_AddButtonContent = new GUIContent("Add New Event Type");
            m_EventIDName = new GUIContent("");
            // Have to create a copy since otherwise the tooltip will be overwritten.
            m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
            m_IconToolbarMinus.tooltip = "Remove all events in this list.";

            string[] eventNames = Enum.GetNames(typeof(OverlayManager.ManagedEvents));
            m_EventTypes = new GUIContent[eventNames.Length];
            for (int i = 0; i < eventNames.Length; ++i)
            {
                m_EventTypes[i] = new GUIContent(eventNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_notificationPosition, true);
            EditorGUILayout.PropertyField(m_notificationInset, true);

            int toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

            for (int i = 0; i < m_DelegatesProperty.arraySize; ++i)
            {
                SerializedProperty delegateProperty = m_DelegatesProperty.GetArrayElementAtIndex(i);
                m_EventIDName.text = delegateProperty.enumDisplayNames[delegateProperty.enumValueIndex];

                switch ((OverlayManager.ManagedEvents)delegateProperty.enumValueIndex)
                {
                    case OverlayManager.ManagedEvents.OverlayActivated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(OverlayManager.evtOverlayActivated)), m_EventIDName);
                        break;
                    case OverlayManager.ManagedEvents.JoinGameRequested:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(OverlayManager.evtRichPresenceJoinRequested)), m_EventIDName); 
                        break;
                    case OverlayManager.ManagedEvents.LobbyInviteAccepted:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(OverlayManager.evtGameLobbyJoinRequested)), m_EventIDName); 
                        break;
                    case OverlayManager.ManagedEvents.ServerChangeRequested:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(OverlayManager.evtGameServerChangeRequested)), m_EventIDName); 
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