#if !DISABLESTEAMWORKS && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamUserData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserDataEvents : MonoBehaviour, IPointerClickHandler
    {
        [EventField]
        public PersonaStateChangeEvent onChange;
        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that will be invoked when the user clicks the UI element
        /// </summary>
        [EventField]
        public UnityUserAndPointerDataEvent onClick;

        private SteamUserData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamUserData>();
            m_Inspector.onChanged?.AddListener(onChange.Invoke);
        }

        private void OnDestroy()
        {
            if(m_Inspector != null)
                m_Inspector.onChanged?.RemoveListener(onChange.Invoke);
        }

        /// <summary>
        /// An implementation of <see cref="IPointerClickHandler"/> this will be invoked when the user clicks on the UI element
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke(new UserAndPointerData(m_Inspector.Data, eventData));
        }
    }
}
#endif