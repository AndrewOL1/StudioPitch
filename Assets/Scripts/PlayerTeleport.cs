using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using UnityEngine;

public class PlayerTeleport : PlayerIdentity<PlayerTeleport>
{
    [SerializeField] SsxPlayerController playerMovement;
    
    private GameManager _gameManager;
    bool _lobbySceneLoad = false;
    public void Teleport(Vector3 destination)
    {
        ServerTeleport(destination);
    }
    [ServerRpc]
    private void ServerTeleport(Vector3 destination) {
        playerMovement.StopMovement();
        Debug.Log($"Teleporting to {destination}");
        transform.position = destination;
        Debug.Log(transform.position);
    }

    private void Start()
    {
        _gameManager = InstanceHandler.GetInstance<GameManager>();
        networkManager.sceneModule.onSceneLoaded += HandleSceneLoaded();
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
}
