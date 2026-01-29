#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamGameServerData))]
    public class SteamGameServerEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange;
    }
}
#endif