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

        [ContextMenu("ChangeScene")]
        public void ChangeScene()
        {
            networkManager.sceneModule.LoadSceneAsync(_sceneName);
        }

        private void Start()
        {
            StartCoroutine(SetToSpawn(delay));
        }
        public void TeleportPlayer(PlayerID targetPlayer) 
        {
            //Returns true if it finds the player
            if(!PlayerTeleport.TryGetPlayer(targetPlayer, out var player))
                return;
            
            player.Teleport(spawnPoint.position);
        }

        public void TeleportLocalPlayer() 
        {
            //Returns true if it finds the local player
            if(!PlayerTeleport.TryGetLocal(out var player))
                return;
            
            player.Teleport(spawnPoint.position);
        }
    
        public void TeleportAllPlayers() 
        {
            //allPlayers gives you a dictionary of all the players registered
            foreach(var player in PlayerTeleport.allPlayers) {
                player.Value.Teleport(spawnPoint.position);
            }
        }
        public void RestAllPlayers() 
        {
            //allPlayers gives you a dictionary of all the players registered
            foreach(var player in PlayerContainerReset.allPlayers) {
                player.Value.Teleport();
            }
        }

        IEnumerator SetToSpawn(float time)
        {
            yield return new WaitForSeconds(time);
            RestAllPlayers();
            TeleportAllPlayers();
        }
        
    }
}
