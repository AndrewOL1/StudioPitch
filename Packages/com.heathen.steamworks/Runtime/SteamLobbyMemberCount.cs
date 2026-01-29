#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Member Count", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyMemberCount : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            Matchmaking.Client.OnLobbyChatUpdate.AddListener(HandleChatUpdate);
            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.MemberCount.ToString();
        }

        private void OnDestroy()
        {
            m_Inspector.onChanged.RemoveListener(HandleOnChanged);
            Matchmaking.Client.OnLobbyChatUpdate.RemoveListener(HandleChatUpdate);
        }

        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == m_Inspector.Data)
                label.text = m_Inspector.Data.MemberCount.ToString();
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.MemberCount.ToString();
            else
                label.text = "0";
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMemberCount), true)]
    public class SteamLobbyMemberCountEditor : UnityEditor.Editor
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