using PurrNet;
using UnityEngine;

public class Snowball : NetworkBehaviour
{
    public Vector3 playerVelocity;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 thrownDirection;
    protected override void OnSpawned()
    {
        
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
    }
}
