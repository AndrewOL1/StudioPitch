#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamAchievementData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;
        private SteamAchievementData m_data;

        private void Awake()
        {
            m_data = GetComponent<SteamAchievementData>();

            if (!string.IsNullOrEmpty(m_data.apiName))
            {
                if (API.App.Initialized)
                    label.text = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(m_data.apiName, AchievementAttributes.name);
                else
                    API.App.onSteamInitialized.AddListener(Refresh);
            }
        }

        public void Refresh()
        {
            if (!string.IsNullOrEmpty(m_data.apiName))
                label.text = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(m_data.apiName, AchievementAttributes.name);

            API.App.onSteamInitialized.RemoveListener(Refresh);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamAchievementName), true)]
    public class SteamAchievementNameEditor : UnityEditor.Editor
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