#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [ModularComponent(typeof(SteamLobbyData), "Hex Labels", nameof(label))]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyHexIdLabel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            if (m_Inspector.Data.IsValid)
                label.text = m_Inspector.Data.ToString();
        }

        private void OnDestroy()
        {
            m_Inspector.onChanged.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (m_Inspector.Data.IsValid)
            {
                if (label != null)
                    label.text = m_Inspector.Data.ToString();
            }
            else
            {
                if (label != null)
                    label.text = string.Empty;
            }
        }
    }
}
#endif