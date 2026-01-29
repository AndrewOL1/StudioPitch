#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
#endif
using UnityEngine;
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationEvents))]
    public class SteamAuthenticationRpcInvoke : MonoBehaviour
    {
        private SteamAuthenticationEvents m_Events;

        private void Awake()
        {
            m_Events = GetComponent<SteamAuthenticationEvents>();
            m_Events.onChange.AddListener(HandleTicketChanged);
        }

        private void HandleTicketChanged(AuthenticationTicket arg0)
        {
            if (arg0.Verified && arg0.Result == EResult.k_EResultOK)
                m_Events.onRpcInvoke?.Invoke(UserData.Me, arg0.Data);
            else
                m_Events.onError?.Invoke(arg0.Result);
        }
    }
}
#endif