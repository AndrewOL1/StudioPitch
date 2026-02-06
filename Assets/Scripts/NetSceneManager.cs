using System.Collections;
using PurrNet;
using PurrNet.Modules;
using UnityEngine;

namespace Scripts
{
    public class NetSceneManager : NetworkBehaviour
    {
        [PurrScene] public string _sceneName;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float delay;
        GameManager _gameManager;
        
        [SerializeField] private bool launchWithoutLobby;
        /// <summary>
        /// Make This and instance then wait for all players to send a onLoadComplete
        /// </summary>
        
        private void Awake() 
        {
            //We register the GameManager instance
            InstanceHandler.RegisterInstance(this);
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnDestroy() 
        {
            //Upon being destroyed, we unregister the game manager instance
            InstanceHandler.UnregisterInstance<NetSceneManager>();
        }
        
        [ContextMenu("ChangeScene")]
        public void ChangeScene()
        {
            _gameManager.InitPlayerData();
            networkManager.sceneModule.LoadSceneAsync(_sceneName);
            foreach(var player in PlayerTeleport.allPlayers) {
                player.Value.NewScene();
            }
        }

        private void Start()
        {
            _gameManager = InstanceHandler.GetInstance<GameManager>();
        }
        public void TeleportPlayer(PlayerID targetPlayer) 
        {
            //Returns true if it finds the player
            if(!PlayerTeleport.TryGetPlayer(targetPlayer, out var player))
                return;
            
            player.Teleport(spawnPoint.position,targetPlayer);
        }

        public void TeleportLocalPlayer() 
        {
            //Returns true if it finds the local player
            if(!PlayerTeleport.TryGetLocal(out var player))
                return;
            
            player.Teleport(spawnPoint.position,(PlayerID)localPlayer);
        }
    
        public void TeleportAllPlayers() 
        {
            //allPlayers gives you a dictionary of all the players registered
            foreach(var player in PlayerTeleport.allPlayers) {
                player.Value.Teleport(spawnPoint.position,player.Key);
            }
        }
        public void TeleportAllPlayers(Vector3 position) 
        {
            //allPlayers gives you a dictionary of all the players registered
            foreach(var player in PlayerTeleport.allPlayers) {
                Debug.Log(player.Key);
                player.Value.Teleport(position, player.Key);
            }
        }
    }
}
