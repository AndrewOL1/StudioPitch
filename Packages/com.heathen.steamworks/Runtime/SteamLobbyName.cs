#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration.API;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            Matchmaking.Client.OnLobbyDataUpdate.AddListener(HandleLobbyDataChange);
            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.Name;
        }

        private void OnDestroy()
        {
            m_Inspector.onChanged.RemoveListener(HandleOnChanged);
            Matchmaking.Client.OnLobbyDataUpdate.RemoveListener(HandleLobbyDataChange);
        }

        private void HandleLobbyDataChange(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == m_Inspector.Data)
                label.text = m_Inspector.Data.Name;
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.Name;
            else
                label.text = string.Empty;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyName), true)]
    public class SteamLobbyNameEditor : UnityEditor.Editor
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