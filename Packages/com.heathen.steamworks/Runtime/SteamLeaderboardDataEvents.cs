#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamLeaderboardData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange;
        [EventField]
        public UnityEvent onFindOrCreate;
        [EventField]
        public UnityEvent onFindOrCreateFailure;
        [EventField]
        public UnityEvent<LeaderboardScoreUploaded> onScoreUploaded;
        [EventField]
        public UnityEvent<LeaderboardScoreUploaded> onRankChanged;
        [EventField]
        public UnityEvent<LeaderboardUGCSet> onUGCAttached;

        private SteamLeaderboardData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLeaderboardData>();
            API.Leaderboards.Client.onScoreUploaded.AddListener(HandleScoreUpload);
            API.Leaderboards.Client.onUgcAttached.AddListener(HandleUgcAttached);
        }

        private void OnDestroy()
        {
            API.Leaderboards.Client.onScoreUploaded.RemoveListener(HandleScoreUpload);
            API.Leaderboards.Client.onUgcAttached.RemoveListener(HandleUgcAttached);
        }

        private void HandleUgcAttached(LeaderboardUGCSet arg0, bool arg1)
        {
            if (!arg1
                && arg0.Leaderboard == m_Inspector.Data)
            {
                onUGCAttached.Invoke(arg0);
            }
        }

        private void HandleScoreUpload(LeaderboardScoreUploaded arg0, bool arg1)
        {
            if (!arg1
                && arg0.Leaderboard == m_Inspector.Data)
            {
                onScoreUploaded.Invoke(arg0);

                if (arg0.GlobalRankNew != arg0.GlobalRankPrevious)
                    onRankChanged?.Invoke(arg0);
            }
        }
    }
}
#endif