#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Up Votes", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailUpVoteLabel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

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
            if (label != null)
            {
                if (m_Inspector.Data != null)
                    label.text = m_Inspector.Data.UpVotes.ToString();
                else
                    label.text = "0";
            }
        }
    }
}
#endif