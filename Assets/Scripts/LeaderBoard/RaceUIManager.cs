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
        
        public void ForceShowLeaderboard(int raceIndex)
        {
            //activate leaderboard
            leaderboard.SetActive(true);
            leaderboardRaceText.text = "Race: " + raceIndex;
        }
        public void ForceCloseLeaderboard()
        {
            //disable leaderboard
            leaderboard.SetActive(false);
            ClearLeaderboard();
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public void AddLeaderboardEntry(int score, string name)
        {
            _leaderboardWidget = Instantiate(leaderboardWidget, leaderboardWidgetContainer);
            LeaderboardEntryWidget leaderboardEntryWidget = _leaderboardWidget.GetComponent<LeaderboardEntryWidget>();
            leaderboardEntryWidget.points = score;
            leaderboardEntryWidget.name = name;
        }

        
        private void ClearLeaderboard()
        {
            for (int i = leaderboardWidgetContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(leaderboardWidgetContainer.GetChild(i).gameObject);
            }
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
