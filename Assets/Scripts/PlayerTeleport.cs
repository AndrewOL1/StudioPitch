using System;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.StateMachine;
using States;
using UnityEngine;

public class PlayerTeleport : PlayerIdentity<PlayerTeleport>
{
    //[SerializeField] SsxPlayerController playerMovement;
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private StopedState stopedState;
    [SerializeField] private RaceFinishedState raceFinishedState;
    private SplineRaceTracker _raceTracker;
    private GameManager _gameManager;
    bool _lobbySceneLoad = false;
    
    [SerializeField] private bool launchWithoutLobby;
    public void Teleport(Vector3 destination,PlayerID playerID)
    {
        ServerTeleport(playerID,destination);
    }
    [TargetRpc]
    private void ServerTeleport(PlayerID playerID,Vector3 destination) {
        //playerMovement.StopMovement();
        transform.position = destination;
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

    public void SetStopState()
    {
        stateMachine.SetState(stopedState);
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
            stateMachine.SetState(raceFinishedState);
        }
            
    }

    public float UpdateProgress()
    {
        if (!_raceTracker)
            _raceTracker = FindFirstObjectByType<SplineRaceTracker>();
        float progress,dist;
        (progress,dist)=_raceTracker.GetPlayerProgress(transform.position);
        return progress;
    }

    public void StartRace()
    {
        stopedState.StartRace();
    }
}
