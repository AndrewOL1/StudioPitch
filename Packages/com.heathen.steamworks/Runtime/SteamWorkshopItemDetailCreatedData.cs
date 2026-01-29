#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Date Created", nameof(settings))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailCreatedData : MonoBehaviour
    {
        [Serializable]
        public class Settings
        {
            public string format = "yyyy-MMM-dd HH:mm";
            public TMPro.TextMeshProUGUI label;
        }

        public Settings settings;

        private SteamWorkshopItemDetailData m_Inspector;
        private SteamWorkshopItemDetailDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamWorkshopItemDetailData>();
            m_Events = GetComponent<SteamWorkshopItemDetailDataEvents>();

            m_Events.onChange.AddListener(HandleChanged);
            HandleChanged();
        }

        private void HandleChanged()
        {
            if (settings.label != null)
            {
                if (m_Inspector.Data != null)
                    settings.label.text = m_Inspector.Data.TimeCreated.ToString(settings.format);
                else
                    settings.label.text = string.Empty;
            }
        }
    }
}
#endif