using PurrNet;
using TMPro;
using UnityEngine;

namespace LeaderBoard
{
    public class RaceUIManager : NetworkBehaviour
    {
        
        [Header("Race UI")]
        [SerializeField] private RaceProgressBar raceProgressBar;
        [SerializeField] private TextMeshProUGUI positionText;
        
        [Header("LeaderboardUI")]
        [SerializeField] private GameObject leaderboard;
        [SerializeField] private TextMeshProUGUI leaderboardRaceText;
        [SerializeField] private RectTransform leaderboardWidgetContainer;
        [SerializeField] private NetworkIdentity leaderboardWidget;
        private NetworkIdentity _leaderboardWidget;
        private void Awake()
        {
            InstanceHandler.RegisterInstance(this);
            DontDestroyOnLoad(this);
        }

        private void OnDestroy() 
        {
            //Upon being destroyed, we unregister the game manager instance
            InstanceHandler.UnregisterInstance<RaceUIManager>();
        }
        [ServerRpc]
        public void ForceShowLeaderboard(int raceIndex)
        {
            //activate leaderboard
            leaderboard.SetActive(true);
            leaderboardRaceText.text = "Race: " + raceIndex;
        }
        [ServerRpc]
        public void ForceCloseLeaderboard()
        {
            //disable leaderboard
            leaderboard.SetActive(false);
            ClearLeaderboard();
        }
        // ReSharper disable Unity.PerformanceAnalysis
        [ServerRpc]
        public void AddLeaderboardEntry(int score, string name)
        {
            _leaderboardWidget = Instantiate(leaderboardWidget, leaderboardWidgetContainer);
            LeaderboardEntryWidget leaderboardEntryWidget = _leaderboardWidget.GetComponent<LeaderboardEntryWidget>();
            leaderboardEntryWidget.points = score;
            leaderboardEntryWidget.name = name;
        }

        [ServerRpc]
        private void ClearLeaderboard()
        {
            while (leaderboardWidgetContainer.childCount > 0)
                Destroy(leaderboardWidgetContainer.GetChild(0).gameObject);
        }

        public void UpdateRaceProgress(float progress)
        {
            raceProgressBar.UpdateProgress(progress);
        }
        public void UpdateRacePosition(int position,int playerCount)
        {
            positionText.text = position+1 + "/" + playerCount;
        }
    }
}
