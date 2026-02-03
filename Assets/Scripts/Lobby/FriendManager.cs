using Heathen.SteamworksIntegration;
using PurrNet;
using SteamExample;
using UnityEngine;

namespace Lobby
{
    public class FriendManager : MonoBehaviour
    {
        [SerializeField] private GameObject friendWidget;
        [SerializeField] private Transform content;

        private void Start()
        {
            if (SteamTools.Interface.IsReady)
            {
                Interface_OnReady();
            }
            else
            {
                // Not ready yet, listen for On Ready
                SteamTools.Interface.OnReady += Interface_OnReady;
            }
        }

        private void Interface_OnReady()
        {
            UserData[] myFriends = UserData.MyFriends;
            
            //You can now loop through that list and read the profile information
            foreach(var friend in myFriends)
            {
                //Init friendslist
                //if (friend.State == EPersonaState.k_EPersonaStateOffline) return;
                GameObject newFriend = Instantiate(friendWidget, content);
                InviteFriend inviteFriend = newFriend.GetComponent<InviteFriend>();
                friend.LoadAvatar((Texture2D avatarTexture) => 
                {
                    if (avatarTexture != null)
                    {
                        // 2. Assign the texture to your UI once it arrives
                        inviteFriend.avatar.texture = avatarTexture;
                    }
                    else
                    {
                        Debug.LogError("Failed to load avatar.");
                    }
                });
                inviteFriend.userName.text = friend.Name;
                inviteFriend.userData = friend;
            }
        }
    }
}
