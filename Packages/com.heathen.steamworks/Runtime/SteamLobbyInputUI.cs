#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Input UI", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyInputUI : MonoBehaviour
    {
        [SettingsField(header = "Input UI")]
        public bool inputAlwaysReadOnly = true;
        [SettingsField(header = "Input UI")]
        public bool onlyOwnerCanInvite = false;
        [SettingsField(header = "Input UI")]
        public int minimalIdLength = 8;

        [ElementField(header = "Input UI")]
        public TMPro.TMP_InputField idInput;
        [ElementField(header = "Input UI")]
        public GameObject createElement;
        [ElementField(header = "Input UI")]
        public GameObject joinElement;
        [ElementField(header = "Input UI")]
        public GameObject leaveElement;
        [ElementField(header = "Input UI")]
        public GameObject inviteElement;
        [ElementField(header = "Input UI")]
        public GameObject membersElement;
        [ElementField(header = "Input UI")]
        public GameObject chatElement;

        private SteamLobbyData m_Inspector;

        private void Start()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            idInput.onValueChanged.AddListener(HandleOnValueChanged);
            HandleOnChanged(m_Inspector.Data);
        }

        private void OnDestroy()
        {
            m_Inspector.onChanged.RemoveListener(HandleOnChanged);
            idInput.onValueChanged.RemoveListener(HandleOnValueChanged);
        }

        private void HandleOnValueChanged(string arg0)
        {
            if (joinElement != null)
            {
                if (idInput.text.Length >= minimalIdLength)
                {
                    var lobby = LobbyData.Get(idInput.text);
                    joinElement.SetActive(lobby.IsValid);
                }
                else
                    joinElement.SetActive(false);
            }
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (arg0.IsValid)
            {
                if (createElement != null)
                    createElement.SetActive(false);
                if (joinElement != null)
                    joinElement.SetActive(false);
                if (leaveElement != null)
                    leaveElement.SetActive(true);
                if (inviteElement != null)
                {
                    if (onlyOwnerCanInvite)
                        inviteElement.SetActive(arg0.IsOwner);
                    else
                        inviteElement.SetActive(true);
                }
                if (membersElement != null)
                    membersElement.SetActive(true);
                if (chatElement != null)
                    chatElement.SetActive(true);

                idInput.text = arg0.ToString();
                idInput.readOnly = true;
            }
            else
            {
                if (createElement != null)
                    createElement.SetActive(true);
                if (joinElement != null)
                {
                    if (idInput.text.Length >= minimalIdLength)
                    {
                        var lobby = LobbyData.Get(idInput.text);
                        joinElement.SetActive(lobby.IsValid);
                    }
                    else
                        joinElement.SetActive(false);
                }
                if (leaveElement != null)
                    leaveElement.SetActive(false);
                if (inviteElement != null)
                    inviteElement.SetActive(false);
                if (membersElement != null)
                    membersElement.SetActive(false);
                if (chatElement != null)
                    chatElement.SetActive(false);

                idInput.text = string.Empty;
                idInput.readOnly = inputAlwaysReadOnly;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyInputUI), true)]
    public class SteamLobbyInputUIEditor : UnityEditor.Editor
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