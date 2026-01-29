#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Leave", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyLeave : MonoBehaviour
    {
        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
        }

        public void Leave()
        {
            if (m_Inspector.Data.IsValid)
            {
                m_Inspector.Data.Leave();
                m_Inspector.Data = CSteamID.Nil;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyLeave), true)]
    public class SteamLobbyLeaveEditor : UnityEditor.Editor
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