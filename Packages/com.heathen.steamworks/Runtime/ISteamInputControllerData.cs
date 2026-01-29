#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    public interface ISteamInputControllerData
    {
        public InputHandle_t? Data { get; set; }
    }
}
#endif