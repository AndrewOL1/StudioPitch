using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using PurrNet.Packing;
using Scripts;
using UnityEngine.Serialization;
using TMPro;

public class GameManager : NetworkBehaviour
{
    # region variables
    [SerializeField]private float raceUpdateInterval = 0.5f;
    
    [SerializeField]private SyncDictionary<PlayerID,bool> playersReady = new(true);
    NetSceneManager _netSceneManager;
    [SerializeField]Vector3 spawnPosition;
    [SerializeField] private SyncDictionary<PlayerID, float> playerProgressDict = new(true);
    [SerializeField] private TMP_Text positionUIText;
    [SerializeField] private int positionTest;
    [SerializeField] private List<float> sortedPositiions;
    #endregion
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        DontDestroyOnLoad(this);
        positionTest = (int)playersReady.Count / 2;
    }

    private void OnDestroy() 
    {
        //Upon being destroyed, we unregister the game manager instance
        InstanceHandler.UnregisterInstance<GameManager>();
    }

    private void Start()
    {
        _netSceneManager=InstanceHandler.GetInstance<NetSceneManager>();
        
    }
    
    protected override void OnSpawned()
    {
        //Subscribing to changes made to the dictionary
        playersReady.onChanged += OnPlayersReadyChanged;
        playerProgressDict.onChanged += OnPlayerProgressDictChanged;
    }

    private void OnPlayersReadyChanged(SyncDictionaryChange<PlayerID, bool> change)
    {
        Debug.Log($"PlayersReady updated: {change}");
        CheckReady();
    }
    [ServerRpc]
    public void SceneLoaded(PlayerID key,bool value)
    {
        playersReady[key] = value;
    }

    private void CheckReady()
    {
        foreach (var player in playersReady)
        {
            if (!player.Value)
                return;
        }
        _netSceneManager.TeleportAllPlayers(spawnPosition);
        Debug.Log($"All players are ready: {playersReady.Count}");
    }
    
    [ServerRpc]
    public void InitPlayersReady()
    {
        foreach(var player in PlayerTeleport.allPlayers)
        {
            Debug.Log(player.Value.PlayerID());
            if (player.Value.PlayerID()!=null)
                playersReady[player.Value.PlayerID()] = false;
        }
    }

    public void FixedUpdate()
    {
        UpdateRaceUI();
    }

    private void UpdateRaceUI()
    {
        // call once every interval
        // get players and for each call the update progress
        // sort in order of progress
        // store the vars in a sync dictionary <Name,position>

        positionUIText.text = positionTest + "/" + playersReady.Count;
    }

    public void StartRace()
    {
        //needs to be called once all players have loaded the scene
        //start Race
    }

    public void StopRace()
    {
        //stop race
        //show race results
        //delay
        //show updated leaderboard
        //delay
        //start power-up selection
        
    }

    private void StartPowerUpSelection()
    {
        //activate the powerup ui
        //get the players postions and give a selection based on that
        //apply the powerups to players
        //after set time
        //clear race data
        //start next race
    }

    [ServerRpc]
    private void OnPlayerProgressDictChanged(SyncDictionaryChange<PlayerID, float> change)
    {
        
        foreach (var player in PlayerTeleport.allPlayers)
        {
            float currentPlayerProg = player.Value.GetComponent<SsxPlayerController>().UpdateProgress();
            playerProgressDict[player.Value.PlayerID()] = currentPlayerProg;

            sortedPositiions.Add(currentPlayerProg);
        }

        sortedPositiions.Sort();
        sortedPositiions.Reverse();





        sortedPositiions.Clear();
    }
}
