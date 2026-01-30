using Heathen.SteamworksIntegration;
using PurrNet;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Lobby
{
    public class UserInvite : MonoBehaviour
    {
        private FriendManager _myFriendManager;
        [SerializeField]private TMP_Text myUser;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _myFriendManager = InstanceHandler.GetInstance<FriendManager>();
        }

        // Update is called once per frame
        public void OnClick()
        {
            _myFriendManager.InviteFriend(myUser.text);
        }
    }
}
