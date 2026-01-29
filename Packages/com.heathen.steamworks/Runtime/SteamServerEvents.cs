#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class SteamServerEvents : MonoBehaviour
    {
        /// <summary>
        /// An event raised when by Steamworks debugging on disconnected.
        /// This is only available in server builds.
        /// </summary>
        public API.App.Server.DisconnectedEvent onDisconnected;
        /// <summary>
        /// An event raised by Steamworks debugging on connected.
        /// This is only available in server builds.
        /// </summary>
        public API.App.Server.ConnectedEvent onConnected;
        /// <summary>
        /// An event raised by Steamworks debugging on failure.
        /// This is only available in server builds.
        /// </summary>
        public API.App.Server.FailureEvent onFailure;

        private void Awake()
        {
            API.App.Server.onDisconnected.AddListener(onDisconnected.Invoke);
            API.App.Server.onConnected.AddListener(onConnected.Invoke);
            API.App.Server.onFailure.AddListener(onFailure.Invoke);
        }

        private void OnDestroy()
        {
            API.App.Server.onDisconnected.RemoveListener(onDisconnected.Invoke);
            API.App.Server.onConnected.RemoveListener(onConnected.Invoke);
            API.App.Server.onFailure.RemoveListener(onFailure.Invoke);
        }
    }
}
#endif