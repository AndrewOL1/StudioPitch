using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    bool _updateRace = false;
    
    NetSceneManager _netSceneManager;
    public Transform spawnPosition;
    [SerializeField] private TMP_Text positionUIText;
    [SerializeField] private int positionTest;
    [SerializeField] private SyncDictionary<PlayerID,PlayerData> playerData = new(true);
    private RaceUIManager _raceUIManager;
    //testing
    [SerializeField] private bool launchWithoutLobby;
    [SerializeField] private List<PlayerData> playersInOrder = new();
    [SerializeField] private float leaderboardDisplayTime;

    private bool _resettingReady = false,_resettingFinished=false;
    private int _raceIndex=1;
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
        playerData.onChanged += OnPlayerDataChanged;
        //playerProgressDict.onChanged += OnPlayerProgressDictChanged;
    }
    
    private void OnPlayerDataChanged(SyncDictionaryChange<PlayerID,PlayerData> change)
    {
       // Debug.Log($"PlayerDataListChanged updated: {change}"); ANNOYING
       if (!launchWithoutLobby)
       {
           if(!_resettingReady)
               CheckReady();
           if(!_resettingFinished)
               CheckFinished();
       }
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
    
    

    # region PlayerScore
    [ServerRpc]
    public void PlayerScoreChanged(PlayerID key,int value)
    {
        PlayerData tempPlayerData = playerData[key];
        tempPlayerData.score = value;
        playerData[key] = tempPlayerData;
    }
    #endregion

    # region PlayerFinished
    
    [ServerRpc]
    public void PlayerFinished(PlayerID key,bool value)
    {
        PlayerData tempPlayerData = playerData[key];
        tempPlayerData.finished = value;
        playerData[key] = tempPlayerData;
    }
    [ContextMenu("CheckFinished")]
    private void CheckFinished()
    {
        foreach (var player in playerData.Values)
        {
            if (!player.finished)
                return;
        }
        //display leaderboard
        StopRace();
        ResetFinished();
    }
    # endregion

    # region PlayerReady
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
        foreach (var player in playerData.Values)
        {
            if (!player.ready)
                return;
        }
        Debug.Log($"All players are ready: {playerData.Count}");
        StartCoroutine(SceneDelay());
        StartRace();
        ResetReady();
    }

    private void ResetReady()
    {
        _resettingReady = true;
        foreach (var player in PlayerTeleport.allPlayers)
        {
            PlayerData tempPlayerData = playerData[player.Key];
            tempPlayerData.ready = false;
            playerData[player.Key] = tempPlayerData;
        }
        _resettingFinished = false;
    }
    private void ResetFinished()
    {
        _resettingFinished = true;
        foreach (var player in PlayerTeleport.allPlayers)
        {
            PlayerData tempPlayerData = playerData[player.Key];
            tempPlayerData.finished = false;
            playerData[player.Key] = tempPlayerData;
        }
        _resettingFinished = false;
    }
    
    # endregion

    public void FixedUpdate()
    {
        if (_updateRace)
            UpdateRaceUI();
    }

    private void UpdateRaceUI()
    {
        // call once every interval
        // get players and for each call the update progress
        if (!_raceUIManager)
        {
            _raceUIManager = InstanceHandler.GetInstance<RaceUIManager>();
            return;
        }

        PlayerProgress();
        var sorted = playerData.OrderByDescending(kvp => kvp.Value.progress).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        int count = 0;
        foreach (var player in sorted)
        {
            UpdatePositionUI(player.Key,count);
            count++;
        }
        // sort in order of progress
    }

    public void StartRace()
    {
        //needs to be called once all players have loaded the scene
        //start Race
        Debug.Log("StartRace in...");
        StartCoroutine(StartCountdown());
    }

    public void StopRace()
    {
        //stop race
        //delay
        //show updated leaderboard
        _updateRace = false;
        foreach (var player in playerData)
        {
            _raceUIManager.AddLeaderboardEntry(player.Value.score, player.Value.name);
        }
        _raceUIManager.ForceShowLeaderboard(_raceIndex);
        StartCoroutine(LeaderboardDisplay());
        
    }

    public void StartNewRace()
    {
        _raceIndex++;
        
        _netSceneManager.TeleportAllPlayers();
        StartCoroutine(StartCountdown());
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
    
    private void PlayerProgress()
    {
        _updateRace=false;
        foreach (var player in PlayerTeleport.allPlayers)
        {
            float currentPlayerProg = player.Value.UpdateProgress();
            PlayerData tempPlayerData = playerData[player.Key];
            tempPlayerData.progress = currentPlayerProg;
            playerData[player.Key] = tempPlayerData;
        }

        StartCoroutine(ProgressDelay());
    }

    [TargetRpc]
    private void UpdatePositionUI(PlayerID key,int count)
    {
        _raceUIManager.UpdateRacePosition(count,playerData.Count);
        _raceUIManager.UpdateRaceProgress(playerData[key].progress);
    }

    IEnumerator ProgressDelay()
    {
        yield return new WaitForSeconds(raceUpdateInterval);
        _updateRace = true;
    }

    IEnumerator SceneDelay()
    {
        yield return new WaitForSeconds(0.5f);
        InstanceHandler.GetInstance<NetSceneManager>().TeleportAllPlayers();
    }
    IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("...3...");
        yield return new WaitForSeconds(1f);
        Debug.Log("...2...");
        yield return new WaitForSeconds(1f);
        Debug.Log("...1...");
        yield return new WaitForSeconds(1f);
        Debug.Log("...GO...");
        foreach (var player in PlayerTeleport.allPlayers)
        {
            player.Value.StartRace();
        }
        _updateRace=true;
    }
    IEnumerator LeaderboardDisplay()
    {
        yield return new WaitForSeconds(leaderboardDisplayTime);
        //would go to power up
        StartNewRace();
    }
}
