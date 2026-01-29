#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamInventoryItemData), "Names", nameof(label))]
    [RequireComponent(typeof(SteamInventoryItemDataEvents))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamInventoryItemData m_Inspector;
        private SteamInventoryItemDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamInventoryItemData>();
            m_Events = GetComponent<SteamInventoryItemDataEvents>();

            m_Events.onChange?.AddListener(HandleChange);
        }

        private void HandleChange()
        {
            if (label != null)
                label.text = m_Inspector.Data.Name;
        }
    }
}
#endif