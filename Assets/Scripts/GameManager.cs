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
    //[SerializeField] private SyncList<PlayerData> playerDataList = new(true);
    //testing
    [SerializeField] private bool launchWithoutLobby;
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
        playerFinished.onChanged += OnPlayerFinishedChanged;
        playerScore.onChanged += OnPlayerScoreChanged;
        //playerDataList.onChanged += OnPlayerDataListChanged;
        //playerProgressDict.onChanged += OnPlayerProgressDictChanged;
    }
    /*
    private void OnPlayerDataListChanged(SyncListChange<PlayerData> change)
    {
        Debug.Log($"PlayerDataListChanged updated: {change}");
    }
    [ServerRpc]
    public void PlayersScoreChanged(PlayerID key,int value)
    {
        var playerData = playerDataList[0];
        playerData.score = value;
    }
    */

    # region PlayerScore
    private void OnPlayerScoreChanged(SyncDictionaryChange<PlayerID, int> change)
    {
        Debug.Log($"PlayersScore updated: {change}");
    }
    [ServerRpc]
    public void PlayerScoreChanged(PlayerID key,int value)
    {
        playerScore[key] = value;
    }

    private void InitPlayerScore()
    {
        foreach (var playerID in playersReady)
        {
            playerScore[playerID.Key] = 0;
        }
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
        playerFinished[key] = value;
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
    
    private void InitPlayerFinished()
    {
        foreach (var playerID in playersReady)
        {
            playerFinished[playerID.Key] = false; ;
        }
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
        playersReady[key] = value;
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
