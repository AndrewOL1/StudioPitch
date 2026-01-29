#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
#endif

namespace Heathen.SteamworksIntegration
{
    public interface ISteamInputActionData
    {
        public InputActionSetData Set { get; set; }
        public InputActionSetLayerData Layer { get; set; }
        public InputActionData Action { get; set; }
    }
}
#endif