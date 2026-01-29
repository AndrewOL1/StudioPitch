#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamAchievementData))]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementChanged : MonoBehaviour
    {
        [EventField]
        public UnityEvent<bool> onChanged;
        private SteamAchievementData m_data;

        private void Awake()
        {
            m_data = GetComponent<SteamAchievementData>();
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.AddListener(HandleChange);
        }

        private void OnDestroy()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.RemoveListener(HandleChange);
        }
        private void HandleChange(string arg0, bool arg1)
        {
            if (arg0 == m_data.apiName)
                onChanged?.Invoke(arg1);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamAchievementChanged), true)]
    public class SteamAchievementChangedEditor : UnityEditor.Editor
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