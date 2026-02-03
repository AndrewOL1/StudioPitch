using Heathen.SteamworksIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class InviteFriend : MonoBehaviour
    {
        public RawImage avatar;
        public TextMeshProUGUI userName;
        
        [SerializeField] private string gameCode;
        [SerializeField] private OverlayManager overlayManager;
        public bool isHost = false;
        public UserData userData;
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
            
        }

        public void InviteToGame()
        {
            Debug.Log("InviteToGame: steamcode "+ UserData.Me.HexId);
            userData.InviteToGame(UserData.Me.HexId);
            
        }
        
        
    }
}