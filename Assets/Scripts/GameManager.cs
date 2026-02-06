using System.Collections.Generic;
using LeaderBoard;
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
    [SerializeField] Transform spawnPosition;
    [SerializeField] private SyncDictionary<PlayerID, float> playerProgressDict = new(true);
    [SerializeField] private TMP_Text positionUIText;
    [SerializeField] private int positionTest;
    [SerializeField] private SyncDictionary<PlayerID, bool> playerFinished = new(true);
    [SerializeField] private SyncDictionary<PlayerID, int> playerScore = new(true);
    [SerializeField] private SyncDictionary<PlayerID,PlayerData> playerData = new(true);
    //testing
    [SerializeField] private bool launchWithoutLobby;
    #endregion
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        DontDestroyOnLoad(this);
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
        playerFinished.onChanged += OnPlayerFinishedChanged;
        playerScore.onChanged += OnPlayerScoreChanged;
        playerData.onChanged += OnPlayerDataChanged;
        //playerProgressDict.onChanged += OnPlayerProgressDictChanged;
    }
    
    private void OnPlayerDataChanged(SyncDictionaryChange<PlayerID,PlayerData> change)
    {
        Debug.Log($"PlayerDataListChanged updated: {change}");
    }
    
    [ServerRpc]
    public void InitPlayerData()
    {
        foreach(var player in PlayerTeleport.allPlayers)
        {
            if (player.Value.PlayerID() != null)
            {
                PlayerData tempPlayerData = new PlayerData();
                tempPlayerData.name = "";
                tempPlayerData.score = 0;
                tempPlayerData.progress = 0;
                tempPlayerData.ready = false;
                tempPlayerData.finished = false;
                playerData[player.Value.PlayerID()] = tempPlayerData;
            }
        }
    }
    [ServerRpc]
    public void PlayerScoreChanged(PlayerID key,int value)
    {
        PlayerData tempPlayerData = playerData[key];
        tempPlayerData.score = value;
        playerData[key] = tempPlayerData;
    }
    

    # region PlayerScore
    private void OnPlayerScoreChanged(SyncDictionaryChange<PlayerID, int> change)
    {
        Debug.Log($"PlayersScore updated: {change}");
    }
    #endregion

    # region PlayerFinished
    private void OnPlayerFinishedChanged(SyncDictionaryChange<PlayerID, bool> change)
    {
        Debug.Log($"PlayersFinished updated: {change}");
        
    }
    [ServerRpc]
    public void PlayerFinished(PlayerID key,bool value)
    {
        PlayerData tempPlayerData = playerData[key];
        tempPlayerData.finished = value;
        playerData[key] = tempPlayerData;
    }
    [ContextMenu("CheckReady")]
    private void CheckFinished()
    {
        foreach (var player in playerFinished)
        {
            if (!player.Value)
                return;
        }
        Debug.Log($"All players are ready: {playerFinished.Count}");
        //display leaderboard
    }
    # endregion

    # region PlayerReady
    private void OnPlayersReadyChanged(SyncDictionaryChange<PlayerID, bool> change)
    {
        Debug.Log($"PlayersReady updated: {change}");
        if(!launchWithoutLobby)
            CheckReady();
    }
    [ServerRpc]
    public void SceneLoaded(PlayerID key,bool value)
    {
        PlayerData tempPlayerData = playerData[key];
        tempPlayerData.ready = value;
        playerData[key] = tempPlayerData;
    }
    [ContextMenu("CheckReady")]
    private void CheckReady()
    {
        foreach (var player in playersReady)
        {
            if (!player.Value)
                return;
        }
        Debug.Log($"All players are ready: {playersReady.Count}");
        _netSceneManager.TeleportAllPlayers(spawnPosition.position);
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
    # endregion

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

        positionUIText.text = positionTest + "/" + playerData.Count;
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

    private void OnDictionaryChanged(SyncDictionaryChange<int, float> change)
    {
        //This is called for everyone when the dictionary changes.
        //It will log out the Key, Value and operation
        Debug.Log($"Dictionary updated: {change}");
    }

    private void ChangeMyDictionary()
    {
        /*//This will change or add a value to the dictionary
        playerProgressDict[123] = 0.69f;

        //This will remove the value from the dictionary
        playerProgressDict.Remove(123);

        //This will mark the key as dirty
        playerProgressDict.SetDirty(123);*/
    }

    [ServerRpc]
    private void OnPlayerProgressDictChanged(SyncDictionaryChange<PlayerID, float> change)
    {
        
        foreach (var player in PlayerTeleport.allPlayers)
        {
            float currentPlayerProg = player.Value.GetComponent<SsxPlayerController>().UpdateProgress();
            playerProgressDict[player.Value.PlayerID()] = currentPlayerProg;


        }

        
    }
}
