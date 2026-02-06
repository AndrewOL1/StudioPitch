using PurrNet;
using UnityEngine;

namespace LeaderBoard
{
    public class RaceUIManager : NetworkBehaviour
    {
        [SerializeField] private GameObject leaderboard;
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

        public void ForceShowLeaderboard()
        {
            //activate leaderboard
            leaderboard.SetActive(true);
        }
        public void ForceCloseLeaderboard()
        {
            //disable leaderboard
            leaderboard.SetActive(false);
        }
        
        
    }
}
