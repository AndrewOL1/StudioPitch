#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Events;
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationData))]
    public class SteamAuthenticationEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent<AuthenticationTicket> onChange;
        [EventField]
        public UnityEvent<ulong, byte[]> onRpcInvoke;
        [EventField]
        public UnityEvent<EResult> onError;
        [EventField]
        public UnityEvent<EBeginAuthSessionResult> onInvalidTicket;
        [EventField]
        public UnityEvent<EAuthSessionResponse> onInvalidSession;
        [EventField]
        public UnityEvent<EAuthSessionResponse> onSessionStart;
    }
}
#endif