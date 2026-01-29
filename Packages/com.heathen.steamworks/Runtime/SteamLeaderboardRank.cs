#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLeaderboardData), "Ranks", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    [RequireComponent(typeof(SteamLeaderboardDataEvents))]
    public class SteamLeaderboardRank : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLeaderboardData m_Inspector;
        private SteamLeaderboardDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLeaderboardData>();
            m_Events = GetComponent<SteamLeaderboardDataEvents>();

            if (m_Events != null)
            {
                m_Events.onChange?.AddListener(HandleOnChanged);
                m_Events.onRankChanged?.AddListener(HandleRankChange);
            }

            if (m_Inspector.Data.IsValid)
            {
                m_Inspector.Data.GetUserEntry(0, (entry, ioError) =>
                {
                    if (!ioError && entry != null)
                        label.text = entry.Rank.ToString();
                    else
                        label.text = string.Empty;
                });
            }
        }

        private void OnDestroy()
        {
            if (m_Events != null)
            {
                m_Events.onChange?.RemoveListener(HandleOnChanged);
                m_Events.onRankChanged?.RemoveListener(HandleRankChange);
            }
        }

        private void HandleRankChange(LeaderboardScoreUploaded arg0)
        {
            label.text = arg0.GlobalRankNew.ToString();
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