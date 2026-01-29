#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
namespace Heathen.SteamworksIntegration
{
    public interface ISteamLobbyData
    {
        public LobbyData Data { get; set; }
    }
}
#endif