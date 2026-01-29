#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)

using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public enum OverlayDialog
    {
        friends,
        community,
        players,
        settings,
        officalgamegroup,
        stats,
        achievements,
    }
}
#endif