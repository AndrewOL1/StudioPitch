#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
#endif
using UnityEngine;
using Heathen.SteamworksIntegration.API;
using System;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAuthenticationEvents))]
    public class SteamAuthenticationSessions : MonoBehaviour
    {
        [Flags]
        public enum AuthSessionResponseMask
        {
            None = 0,
            OK = 1 << 0,
            NotConnectedToSteam = 1 << 1,
            NoLicenseOrExpired = 1 << 2,
            VACBanned = 1 << 3,
            LoggedInElseWhere = 1 << 4,
            VACCheckTimedOut = 1 << 5,
            Canceled = 1 << 6,
            AlreadyUsed = 1 << 7,
            Invalid = 1 << 8,
            PublisherIssuedBan = 1 << 9,
            IdentityFailure = 1 << 10,
        }

        [SettingsField]
        public AuthSessionResponseMask acceptedResponses;
        public List<AuthenticationSession> Sessions => Authentication.activeSessions;

        private SteamAuthenticationEvents m_Events;

        private void Awake()
        {
            m_Events = GetComponent<SteamAuthenticationEvents>();
        }

        public void Begin(ulong user, byte[] ticket)
        {
            var result = Authentication.BeginAuthSession(ticket, user, session =>
            {
                // Map Steam response to mask
                AuthSessionResponseMask mask = session.Response switch
                {
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseOK => AuthSessionResponseMask.OK,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseUserNotConnectedToSteam => AuthSessionResponseMask.NotConnectedToSteam,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseNoLicenseOrExpired => AuthSessionResponseMask.NoLicenseOrExpired,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseVACBanned => AuthSessionResponseMask.VACBanned,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseLoggedInElseWhere => AuthSessionResponseMask.LoggedInElseWhere,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseVACCheckTimedOut => AuthSessionResponseMask.VACCheckTimedOut,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketCanceled => AuthSessionResponseMask.Canceled,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalidAlreadyUsed => AuthSessionResponseMask.AlreadyUsed,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalid => AuthSessionResponseMask.Invalid,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponsePublisherIssuedBan => AuthSessionResponseMask.PublisherIssuedBan,
                    Steamworks.EAuthSessionResponse.k_EAuthSessionResponseAuthTicketNetworkIdentityFailure => AuthSessionResponseMask.IdentityFailure,
                    _ => 0
                };

                if ((acceptedResponses & mask) != 0)
                    m_Events.onSessionStart?.Invoke(session.Response);
                else
                {
                    session.End();
                    m_Events.onInvalidSession?.Invoke(session.Response);
                }
            });

            if (result != Steamworks.EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                m_Events.onInvalidTicket?.Invoke(result);
        }
        public void End(ulong user) => Authentication.EndAuthSession(user);
        public void End(SteamUserData user)
        {
            if (user != null && user.Data.IsValid)
                Authentication.EndAuthSession(user.Data);
        }
        public void EndAll() => Authentication.EndAllSessions();
    }
}
#endif