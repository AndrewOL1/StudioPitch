#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Hex Inputs", nameof(input))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyHexIdInputField : MonoBehaviour
    {
        public TMPro.TMP_InputField input;

        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            if (input != null)
                input.text = m_Inspector.Data.ToString();
        }

        private void OnDestroy()
        {
            m_Inspector.onChanged.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            if (m_Inspector.Data.IsValid)
            {
                if (input != null)
                    input.text = m_Inspector.Data.ToString();
            }
            else
            {
                if (input != null)
                    input.text = string.Empty;
            }
        }
    }
}
#endif