using UnityEngine;
using PurrNet;
public class GameManager : NetworkBehaviour
{
    # region variables
    [SerializeField]private float raceUpdateInterval = 0.5f;
    [SerializeField] private SyncDictionary<int, float> playerProgressDict = new(true);

    #endregion
    void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy() 
    {
        //Upon being destroyed, we unregister the game manager instance
        InstanceHandler.UnregisterInstance<GameManager>();
    }

    [ServerRpc]
    private void UpdateRaceUI()
    {
        // call once every interval
        // get players and for each call the update progress
        // sort in order of progress
        // store the vars in a sync dictionary <Name,position>

        foreach (var player in PlayerTeleport.allPlayers)
        {
            
            float currentPlayerProg = player.Value.GetComponent<SsxPlayerController>().UpdateProgress();
            playerProgressDict[1/*Player reference*/] = currentPlayerProg;
        }
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

    protected override void OnSpawned()
    {
        //Subscribing to changes made to the dictionary
        playerProgressDict.onChanged += OnDictionaryChanged;
    }

    private void OnDictionaryChanged(SyncDictionaryChange<int, float> change)
    {
        //This is called for everyone when the dictionary changes.
        //It will log out the Key, Value and operation
        Debug.Log($"Dictionary updated: {change}");
    }

    private void ChangeMyDictionary()
    {
        //This will change or add a value to the dictionary
        playerProgressDict[123] = 0.69f;

        //This will remove the value from the dictionary
        playerProgressDict.Remove(123);

        //This will mark the key as dirty
        playerProgressDict.SetDirty(123);
    }
}
