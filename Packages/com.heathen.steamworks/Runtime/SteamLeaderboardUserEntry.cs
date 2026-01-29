#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLeaderboardData), "User Entries", nameof(entryUI))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    [RequireComponent(typeof(SteamLeaderboardDataEvents))]
    public class SteamLeaderboardUserEntry : MonoBehaviour
    {
        public SteamLeaderboardEntryUI entryUI;

        private SteamLeaderboardData m_Inspector;
        private SteamLeaderboardDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLeaderboardData>();
            m_Events = GetComponent<SteamLeaderboardDataEvents>();

            if (m_Events != null)
            {
                m_Events.onChange?.AddListener(Refresh);
                m_Events.onRankChanged?.AddListener(HandleRankChange);
            }
        }

        private void OnDestroy()
        {
            if (m_Events != null)
            {
                m_Events.onChange?.RemoveListener(Refresh);
                m_Events.onRankChanged?.RemoveListener(HandleRankChange);
            }
        }

        public void Refresh()
        {
            if (m_Inspector.Data.IsValid && entryUI != null)
            {
                m_Inspector.Data.GetUserEntry(0, (entry, ioError) =>
                {
                    if (!ioError && entry != null)
                        entryUI.Entry = entry;
                    else
                        entryUI.Entry = new()
                        {
                            entry = new Steamworks.LeaderboardEntry_t()
                            {
                                m_steamIDUser = UserData.Me,
                                m_nGlobalRank = 0,
                                m_nScore = 0
                            },
                            details = new int[0],
                        };
                });
            }
        }

        private void HandleRankChange(LeaderboardScoreUploaded arg0)
        {
            Refresh();
        }
    }
}
#endif