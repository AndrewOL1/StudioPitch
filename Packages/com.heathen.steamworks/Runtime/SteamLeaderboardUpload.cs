#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Features for uploading and attaching data to a leaderboard
    /// </summary>
    [ModularComponent(typeof(SteamLeaderboardData), "Upload", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardUpload : MonoBehaviour
    {
        public enum Mode
        {
            KeepBest,
            ForceUpdate
        }

        [SettingsField(header = "Upload")]
        public Mode mode = Mode.KeepBest;
        [SettingsField(header = "Upload")]
        public int score = 0;
        [SettingsField(header = "Upload")]
        public List<int> details = new();

        private SteamLeaderboardData m_Inspector;
        private SteamLeaderboardDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLeaderboardData>();
            m_Events = GetComponent<SteamLeaderboardDataEvents>();
        }

        public void Upload()
        {
            if(m_Inspector.Data.IsValid)
            {
                if (m_Events == null)
                {
                    if (mode == Mode.KeepBest)
                        m_Inspector.Data.UploadScoreKeepBest(score, details.ToArray());
                    else
                        m_Inspector.Data.UploadScoreForceUpdate(score, details.ToArray());
                }
            }
        }

        public void Upload<T>(T attachment)
        {
            if (!typeof(T).IsSerializable)
                throw new InvalidOperationException($"{typeof(T)} must be [Serializable]");

            if (m_Inspector.Data.IsValid)
            {
                if (mode == Mode.KeepBest)
                    m_Inspector.Data.UploadScoreKeepBest(score, details.ToArray());
                else
                    m_Inspector.Data.UploadScoreForceUpdate(score, details.ToArray());
            }
        }
    }
}
#endif