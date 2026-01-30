using Heathen.SteamworksIntegration;
using PurrNet;
using SteamExample;
using UnityEngine;

namespace Lobby
{
    public class FriendManager : MonoBehaviour
    {
        private UserData[] myFriends;

        void Awake()
        {
            InstanceHandler.RegisterInstance(this);
            DontDestroyOnLoad(this);
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }
        void OnDestroy() {
            InstanceHandler.UnregisterInstance<FriendManager>();
        }

        // Update is called once per frame
        void Update()
        {
            myFriends = UserData.MyFriends;
        }

        public void InviteFriend(string friendName)
        {
            foreach(var friend in myFriends)
            {
                //Get the name and whatever else you might like to do with it
                if (friend.Name == friendName)
                {
                    friend.InviteToGame(InstanceHandler.GetInstance<ConnectionManager>().GetHostAddress());
                }
            }
        }
    }
}
