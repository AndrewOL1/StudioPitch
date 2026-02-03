using PurrNet;
using UnityEngine;

public class PlayerContainerReset : PlayerIdentity<PlayerContainerReset>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Teleport()
    {
        transform.position = Vector3.zero;
    }
}
