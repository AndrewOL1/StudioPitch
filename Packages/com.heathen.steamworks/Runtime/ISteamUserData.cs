#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
namespace Heathen.SteamworksIntegration
{
    public interface ISteamUserData
    {
        public UserData Data { get; set; }
    }
}
#endif