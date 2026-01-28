using System;
using PurrNet;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;

namespace Lobby
{
    public class ConnectionManager : NetworkBehaviour
    {
        public ConnectionManager Instance;
        [SerializeField] Button hostButton;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
            DontDestroyOnLoad(gameObject);
        }

        public void StartHost()
        {
            SteamAPI.Init();
        }

        public void StartConnection()
        {
            
        }
    }
}
