using PurrNet;
using UnityEngine;

public class PlayerTeleport : PlayerIdentity<PlayerTeleport>
{
    [SerializeField] SsxPlayerController playerMovement;
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
}
