#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputControllerData))]
    public class SteamInputControllerDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange;
        [EventField]
        public ControllerDataEvent onUpdate;

        private SteamInputControllerData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamInputControllerData>();
            m_Inspector.onChanged?.AddListener(onChange.Invoke);
            API.Input.Client.onInputDataChanged.AddListener(HandleEvent);
        }

        private void OnDestroy()
        {
            if (m_Inspector != null)
                m_Inspector.onChanged?.RemoveListener(onChange.Invoke);

            API.Input.Client.onInputDataChanged.RemoveListener(HandleEvent);
        }

        private void HandleEvent(InputControllerStateData state)
        {
            if(m_Inspector.Data.HasValue
                && state.handle == m_Inspector.Data.Value)
            {
                onUpdate?.Invoke(state);
            }
        }
    }
}
#endif