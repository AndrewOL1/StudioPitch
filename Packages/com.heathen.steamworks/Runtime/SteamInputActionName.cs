#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the name of the indicated action
    /// </summary>
    [ModularComponent(typeof(SteamInputActionData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputActionData))]
    public class SteamInputActionName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamInputActionData m_Inspector;

        private void Start()
        {
            m_Inspector = GetComponent<SteamInputActionData>();

            if (!API.App.Initialized)
                API.App.onSteamInitialized.AddListener(HandleInitialization);
            else
                HandleInitialization();
        }

        private void HandleInitialization()
        {
            API.App.onSteamInitialized.RemoveListener(HandleInitialization);

            RefreshName();
        }

        private void OnEnable()
        {
            RefreshName();
        }

        public void RefreshName()
        {
            if (m_Inspector != null && !string.IsNullOrEmpty(m_Inspector.Action.Name) && label != null)
            {
                if (m_Inspector.Set.Handle > 0)
                {
                    if (API.Input.Client.connectedControllers.Count > 0)
                    {
                        var names = m_Inspector.Action.GetInputNames(API.Input.Client.connectedControllers[0], m_Inspector.Set);
                        if (names.Length > 0)
                        {
                            label.text = names[0];
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(m_Inspector.Layer.layerName))
                {
                    if (API.Input.Client.connectedControllers.Count > 0)
                    {
                        var names = m_Inspector.Action.GetInputNames(API.Input.Client.connectedControllers[0], m_Inspector.Layer);
                        if (names.Length > 0)
                        {
                            label.text = names[0];
                        }
                    }
                }
            }
        }
    }
}
#endif