#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLeaderboardData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLeaderboardData m_Inspector;
        private SteamLeaderboardDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLeaderboardData>();
            m_Events = GetComponent<SteamLeaderboardDataEvents>();
            if(m_Events != null)
                m_Events.onChange?.AddListener(HandleOnChanged);

            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.DisplayName;
        }

        private void OnDestroy()
        {
            if (m_Events != null)
                m_Events.onChange?.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged()
        {
            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.DisplayName;
            else
                label.text = string.Empty;
        }
    }
}
#endif