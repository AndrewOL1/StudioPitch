using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LeaderBoard;
using UnityEngine;
using PurrNet;
using PurrNet.Packing;
using Scripts;
using SteamTools;
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

    private bool _resettingReady = false,_resettingFinished=false,_raceStarted =false,_raceStopped =false;
    private int _raceIndex=1;
    public GameState gameState;
    #endregion

    public enum GameState
    {
        Lobby,
        Starting,
        Gameplay,
        Leaderboard,
        PowerUp
    }
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        DontDestroyOnLoad(this);
        gameState = GameState.Lobby;
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
       /*
       if (!launchWithoutLobby)
       {
           if(!_resettingReady)
               CheckReady();
           if(!_resettingFinished)
               CheckFinished();
       }
       */
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
        gameState = GameState.Leaderboard;
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
        if (playerData.Values.Count == 0) return;
        foreach (var player in playerData.Values)
        {
            if (!player.ready)
                return;
        }
        Debug.Log($"All players are ready: {playerData.Count}");
        StartCoroutine(SceneDelay());
        gameState = GameState.Starting;
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
        switch (gameState)
        {
            case GameState.Lobby: CheckReady();
                break;
            case GameState.Starting: 
                if(!_raceStarted)
                    StartRace();
                break;
            case GameState.Gameplay: 
                if (!_updateRace) return;
                UpdateRaceUI();
                CheckFinished();
                break;
            case GameState.Leaderboard:
                if (!_raceStopped)
                {
                    StopRace();
                }
                break;
            case GameState.PowerUp: 
                break;
            default: Debug.Log("GameState FUCKUP"); break;
        }
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
        if (_raceStarted) return;
        Debug.Log("StartRace in...");
        StartCoroutine(StartCountdown());
        _raceStarted = true;
        _raceStopped = false;
    }

    public void StopRace()
    {
        //stop race
        //delay
        //show updated leaderboard
        if(_raceStopped)return;
        gameState = GameState.Leaderboard;
        _raceStopped = true;
        Debug.Log("STOPRACE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        _raceStarted = false;
        _updateRace = false;
        CalculateScore();
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
        _netSceneManager = InstanceHandler.GetInstance<NetSceneManager>();
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

    private void CalculateScore()
    {
        var sorted = playerData.OrderByDescending(kvp => kvp.Value.progress).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        int count = 0;
        foreach (var player in sorted)
        {
            PlayerData tempPlayerData = playerData[player.Key];
            switch (count)
            {
                case 0:
                    tempPlayerData.score += 9;
                    break;
                case 1: tempPlayerData.score += 7;
                    break;
                case 2: tempPlayerData.score += 6;
                    break;
                case 3: tempPlayerData.score += 5;
                    break;
                case 4: tempPlayerData.score += 4;
                    break;
                case 5: tempPlayerData.score += 3;
                    break;
                case 6: tempPlayerData.score += 2;
                    break;
                case 7: tempPlayerData.score += 1;
                    break;
                default: Debug.Log("Score FUCKUP"); break;
            }
            playerData[player.Key] = tempPlayerData;
            count++;
        }
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
        _raceStopped = false;
        gameState = GameState.Gameplay;
    }
    IEnumerator LeaderboardDisplay()
    {
        yield return new WaitForSeconds(leaderboardDisplayTime);
        //would go to power up
        _raceUIManager.ForceCloseLeaderboard();
        StartNewRace();
    }
}
