#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Game Server")]
    [HelpURL("https://kb.heathen.group/steam/features/authentication")]
    public class SteamGameServerData : MonoBehaviour
    {
        public GameServerData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if (m_Events != null)
                    m_Events.onChange?.Invoke();
            }
        }

        private GameServerData m_Data;
        private SteamGameServerEvents m_Events;

        private void Awake()
        {
            m_Events = GetComponent<SteamGameServerEvents>();
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SteamGameServerData), true)]
    public class SteamGameServerDataEditor : Editor
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