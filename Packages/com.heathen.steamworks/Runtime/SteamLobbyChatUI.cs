#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Chat", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyChatUI : MonoBehaviour
    {
        [SettingsField(header = "Chat UI")]
        [SerializeField]
        private int maxMessages = 200;

        [ElementField(header = "Chat UI")]
        [SerializeField]
        private GameObject chatPanel;
        [ElementField(header = "Chat UI")]
        [SerializeField]
        private TMPro.TMP_InputField inputField;
        [ElementField(header = "Chat UI")]
        [SerializeField]
        private UnityEngine.UI.ScrollRect scrollView;
        [ElementField(header = "Chat UI")]
        [SerializeField]
        private Transform messageRoot;

        [TemplateField(header = "Chat UI")]
        [SerializeField]
        private GameObject myChatTemplate;
        [TemplateField(header = "Chat UI")]
        [SerializeField]
        private GameObject theirChatTemplate;

        private SteamLobbyData m_Inspector;
        private readonly List<SteamLobbyMemberChatMessage> chatMessages = new();

        private void Start()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            API.Matchmaking.Client.OnLobbyChatMsg.AddListener(HandleChatMessage);
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (!arg0.IsValid)
                Clear();
        }

        private void OnDestroy()
        {
            API.Matchmaking.Client.OnLobbyChatMsg.RemoveListener(HandleChatMessage);
        }

        private void Update()
        {
            if (chatPanel == null || inputField == null)
                return;

            //Show or hide the chat panel based on rather or not we have a lobby
            if (m_Inspector.Data.IsValid
                && !chatPanel.activeSelf)
                chatPanel.SetActive(true);
            else if (!m_Inspector.Data.IsValid
                && chatPanel.activeSelf)
                chatPanel.SetActive(false);

            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject
#if ENABLE_INPUT_SYSTEM
                && (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame
                || UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame)
#else
                && (Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter))
#endif
                )
            {
                SendMessage();
            }
        }

        public void Clear()
        {
            if (chatMessages != null)
            {
                foreach(var message in chatMessages)
                {
                    Destroy(message.gameObject);
                }
                chatMessages.Clear();
            }
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == m_Inspector.Data
                && message.type == Steamworks.EChatEntryType.k_EChatEntryTypeChatMsg)
            {
                if (chatMessages != null)
                {
                    if (chatMessages.Count == maxMessages)
                    {
                        Destroy(chatMessages[0].gameObject);
                        chatMessages.RemoveAt(0);
                    }

                    if (message.sender == UserData.Me)
                    {
                        var go = Instantiate(myChatTemplate, messageRoot);
                        go.transform.SetAsLastSibling();
                        var cmsg = go.GetComponent<SteamLobbyMemberChatMessage>();
                        if (cmsg != null)
                        {
                            cmsg.Initialize(message);
                            if (chatMessages.Count > 0
                                && chatMessages[^1].User == cmsg.User)
                                cmsg.IsExpanded = false;

                            chatMessages.Add(cmsg);
                        }
                    }
                    else
                    {
                        var go = Instantiate(theirChatTemplate, messageRoot);
                        go.transform.SetAsLastSibling();
                        var cmsg = go.GetComponent<SteamLobbyMemberChatMessage>();
                        if (cmsg != null)
                        {
                            cmsg.Initialize(message);
                            if (chatMessages[^1].User == cmsg.User)
                                cmsg.IsExpanded = false;

                            chatMessages.Add(cmsg);
                        }
                    }

                    StartCoroutine(ForceScrollDown());
                }
            }
        }

        public void SendMessage()
        {
            if (m_Inspector.Data.IsValid
                && !string.IsNullOrEmpty(inputField.text))
            {
                m_Inspector.Data.SendChatMessage(inputField.text);
                inputField.text = string.Empty;
                StartCoroutine(SelectInputField());
            }
        }

        IEnumerator SelectInputField()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            inputField.ActivateInputField();
        }
        /// <summary>
        /// Called when a new chat message is added to the UI to force the system to scroll to the bottom
        /// </summary>
        /// <returns></returns>
        IEnumerator ForceScrollDown()
        {
            // Wait for end of frame AND force update all canvases before setting to bottom.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            scrollView.verticalNormalizedPosition = 0f;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyChatUI), true)]
    public class SteamLobbyChatUIEditor : UnityEditor.Editor
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