#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using System.Collections.Generic;
using Steamworks;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// The Leaderboard Display is a simple tool that can be used to display LeaderboardEntry records. The Leaderboard UI List can be used in conjunction with the Leaderboard Manager to display Leaderboards in your UI with a zero code setup as is demonstrated in the Leaderboard Sample Scene.
    /// </summary>
    [ModularComponent(typeof(SteamLeaderboardData), "Display", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardDisplay : MonoBehaviour
    {
        /// <summary>
        /// This will be the parent of any records instantiated by the tool. Most often you would set this to be the Content GameObject of a ScrollRect or similar.
        /// </summary>
        [ElementField("Display")]
        public Transform collectionRoot;
        /// <summary>
        /// This is the "template" that will instantiated for each record the list displays. This template should implement a component that inherits from <see cref="ILeaderboardEntryDisplay"/>. You can either create your own UI Control script and implement the <see cref="ILeaderboardEntryDisplay"/> interface or you can use the <see cref="SteamLeaderboardEntryUI"/> we provide which has a basic implementation already done.
        /// </summary>
        [TemplateField("Display")]
        public GameObject entryTemplate;

        private SteamLeaderboardData m_Inspector;
        private readonly List<GameObject> createdRecords = new();

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLeaderboardData>();
        }

        public void GetTopEntries(int count)
        {
            if(m_Inspector.Data.IsValid)
            {
                m_Inspector.Data.GetTopEntries(count, 0, HandleBoardResults);
            }
        }

        public void GetNearByEntries(int count)
        {
            if (m_Inspector.Data.IsValid)
            {
                m_Inspector.Data.GetEntries(Steamworks.ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -count / 2, count / 2, 0, HandleBoardResults);
            }
        }

        public void GetEntries(ELeaderboardDataRequest request, int start, int end, int maxDetailEntries)
        {
            if (m_Inspector.Data.IsValid)
            {
                m_Inspector.Data.GetEntries(request, start, end, maxDetailEntries, HandleBoardResults);
            }
        }

        private void HandleBoardResults(LeaderboardEntry[] entries, bool ioError)
        {
            if(!ioError)
                Display(entries);
        }

        /// <summary>
        /// <para>
        /// Calling this method will cause the Leaderboard UI List to clear any currently displayed records and to instantiate the "Template" for each entry passed in. It will attempt to get the <see cref="ILeaderboardEntryDisplay"/> component on the Template and set it's Entry field.
        /// </para>
        /// <para>
        /// You can connect this method to the <see cref="LeaderboardManager.evtQueryCompleted"/> to automatically display the results of any query ran on the manager. Doing this will give you a "code free" solution for displaying leaderboard entries.
        /// </para>
        /// </summary>
        /// <param name="entries"></param>
        public void Display(LeaderboardEntry[] entries)
        {
            foreach(var entry in createdRecords)
            {
                Destroy(entry);
            }
            createdRecords.Clear();

            foreach(var entry in entries)
            {
                var go = Instantiate(entryTemplate, collectionRoot);
                createdRecords.Add(go);

                var display = go.GetComponent<ILeaderboardEntryDisplay>();
                if (display != null)
                    display.Entry = entry;
            }
        }
    }
}
#endif