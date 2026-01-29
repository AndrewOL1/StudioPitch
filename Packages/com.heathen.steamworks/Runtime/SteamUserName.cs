#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamUserData m_SteamUserData;

        private void Awake()
        {
            m_SteamUserData = GetComponent<SteamUserData>();
            if (m_SteamUserData.Data.IsValid)
            {
                label.text = m_SteamUserData.Data.Name;
            }
            m_SteamUserData.onChanged.AddListener(HandlePersonaStateChanged);
        }

        private void OnDestroy()
        {
            m_SteamUserData.onChanged.RemoveListener(HandlePersonaStateChanged);
        }

        private void HandlePersonaStateChanged(PersonaStateChange arg0)
        {
            if (m_SteamUserData.Data.IsValid && Friends.Client.PersonaChangeHasFlag(arg0.Flags, EPersonaChange.k_EPersonaChangeName))
            {
                label.text = m_SteamUserData.Data.Name;
            }
        }
    }
}
#endif