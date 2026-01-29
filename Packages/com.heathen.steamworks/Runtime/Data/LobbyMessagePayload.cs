#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used with the <see cref="LobbyData.Authenticate(LobbyMessagePayload)"/> feature to authenticate a lobby member with the host of the lobby optionally validating inventory ownership as well.
    /// </summary>
    [Serializable]
    public struct LobbyMessagePayload
    {
        public ulong id;
        public byte[] data;
        public byte[] inventory;
    }
}
#endif