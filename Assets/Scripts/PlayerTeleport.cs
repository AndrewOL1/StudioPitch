using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using UnityEngine;

public class PlayerTeleport : PlayerIdentity<PlayerTeleport>
{
    [SerializeField] SsxPlayerController playerMovement;
    
    private GameManager _gameManager;
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

    private OnSceneActionEvent HandleSceneLoaded()
    {
        _gameManager.SceneLoaded(localPlayerForced.id,true);
        return null;
    }

    public PackedULong PlayerULong()
    {
        return localPlayerForced.id;
    }
}
