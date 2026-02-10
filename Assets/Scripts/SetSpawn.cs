using PurrNet;
using UnityEngine;

public class SetSpawn : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InstanceHandler.GetInstance<GameManager>().spawnPosition = this.transform;
    }
}
