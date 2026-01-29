#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Ratings Fill", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailRatingFill : MonoBehaviour
    {
        public Image image;

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
            if (image != null)
            {
                if (m_Inspector.Data != null)
                    image.fillAmount = m_Inspector.Data.VoteScore;
                else
                    image.fillAmount = 0;
            }
        }
    }
}
#endif