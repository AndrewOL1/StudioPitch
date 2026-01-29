#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Invite", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyInvite : MonoBehaviour
    {
        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
        }

        public void OpenOverlay()
        {
            if (m_Inspector.Data.IsValid)
                API.Overlay.Client.ActivateInviteDialog(m_Inspector.Data);
            else
                Debug.LogWarning("No lobby to invite to");
        }

        public void InviteUser(UserData user)
        {
            if (m_Inspector.Data.IsValid)
                m_Inspector.Data.InviteUserToLobby(user);
            else
                Debug.LogWarning("No lobby to invite to");
        }

        public void InviteFromString(string id)
        {
            var user = UserData.Get(id);
            InviteUser(user);
        }

        public void InviteFromInput(TMPro.TMP_InputField input)
        {
            if(input != null)
                InviteFromString(input.text);
        }

        public void InviteFromUser(SteamUserData user)
        {
            if (user != null && user.Data.IsValid && !user.Data.IsMe)
                InviteUser(user.Data);
        }
    }
}
#endif