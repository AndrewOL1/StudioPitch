#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using Steamworks;
using System;
using Heathen.SteamworksIntegration.UI;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A simple chat message UI behaviour
    /// </summary>
    [AddComponentMenu("")]
    [ModularComponent(typeof(SteamLobbyMemberData), "Chat Message", "")]
    [RequireComponent(typeof(SteamLobbyMemberData))]
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-member-chat-message")]
    public class SteamLobbyMemberChatMessage : MonoBehaviour
    {
        [ElementField("Chat Message")]
        public GameObject expansionPanel;
        [SettingsField(header = "Chat Display")]
        [SerializeField]
        private string dateTimeFormat = "HH:mm:ss";
        [ElementField("Chat Message")]
        [SerializeField]
        private TMPro.TextMeshProUGUI datetime;
        [ElementField("Chat Message")]
        [SerializeField]
        private TMPro.TextMeshProUGUI message;

        public UserData User => m_Data.Data.user;
        public byte[] Data { get; private set; }
        public string Message { get; private set; }
        public DateTime ReceivedAt { get; private set; }
        public EChatEntryType Type { get; private set; }
        public bool IsExpanded
        {
            get
            {
                if (expansionPanel != null)
                    return expansionPanel.activeSelf;
                else
                    return false;
            }
            set
            {
                if (expansionPanel != null)
                    expansionPanel.SetActive(value);
            }
        }

        private SteamLobbyMemberData m_Data;

        private void Awake()
        {
            m_Data = GetComponent<SteamLobbyMemberData>();
        }

        /// <summary>
        /// Initialize the message given a source <see cref="LobbyChatMsg" />
        /// </summary>
        /// <param name="message">The message to initialize</param>
        public void Initialize(LobbyChatMsg message)
        {
            m_Data.Data = new() { lobby = message.lobby, user = message.sender };
            Type = message.type;
            Message = message.Message;
            Data = message.data;
            ReceivedAt = DateTime.Now;

            this.message.text = Message;

            if (datetime != null)
                datetime.text = ReceivedAt.ToString(dateTimeFormat);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMemberChatMessage), true)]
    public class BasicChatMessageEditor : UnityEditor.Editor
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