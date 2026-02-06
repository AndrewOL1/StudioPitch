using System;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using UnityEngine;

public class PlayerTeleport : PlayerIdentity<PlayerTeleport>
{
    [SerializeField] SsxPlayerController playerMovement;
    
    private GameManager _gameManager;
    bool _lobbySceneLoad = false;
    
    [SerializeField] private bool launchWithoutLobby;
    public void Teleport(Vector3 destination,PlayerID playerID)
    {
        ServerTeleport(playerID,destination);
    }
    [TargetRpc]
    private void ServerTeleport(PlayerID playerID,Vector3 destination) {
        playerMovement.StopMovement();
        Debug.Log($"Teleporting "+(PlayerID)this.GetComponent<NetworkIdentity>().owner+" to {destination}");
        transform.position = destination;
        Debug.Log(transform.position);
    }

    private void Start()
    {
        _gameManager = InstanceHandler.GetInstance<GameManager>();
        networkManager.sceneModule.onSceneLoaded += HandleSceneLoaded();
        //testing
        if (launchWithoutLobby)
        {
            _gameManager.SceneLoaded((PlayerID)this.GetComponent<NetworkIdentity>().owner,true);
        }
        
    }

    public void NewScene()
    {
        networkManager.sceneModule.onSceneLoaded += HandleSceneLoaded();
    }

    private OnSceneActionEvent HandleSceneLoaded()
    {
        if (!_lobbySceneLoad)
        {
            _lobbySceneLoad = true;
            return null;
        }
        else
        {
            _gameManager.SceneLoaded((PlayerID)this.GetComponent<NetworkIdentity>().owner,true);
            return null;
        }
    }

    public PlayerID PlayerID()
    {
        return (PlayerID)this.GetComponent<NetworkIdentity>().owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            // set time and send Finished bool to game manager
            _gameManager.PlayerFinished(PlayerID(),true);
        }
            
    }
}
