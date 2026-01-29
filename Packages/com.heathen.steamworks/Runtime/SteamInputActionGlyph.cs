#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the controller button reported for this specific action
    /// </summary>
    [ModularComponent(typeof(SteamInputActionData), "Glyphs", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamInputActionData))]
    public class SteamInputActionGlyph : MonoBehaviour
    {
        /// <summary>
        /// The image the icon will be displayed with
        /// </summary>
        public UnityEngine.UI.RawImage image;

        private SteamInputActionData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamInputActionData>();

            if (!SteamTools.Interface.IsReady)
                SteamTools.Interface.OnReady += HandleInitialization;
            else
                HandleInitialization();
        }

        private void HandleInitialization()
        {
            SteamTools.Interface.OnReady -= HandleInitialization;

            RefreshImage();
        }

        private void OnEnable()
        {
            RefreshImage();
        }
        /// <summary>
        /// Refresh the image
        /// </summary>
        public void RefreshImage()
        {
            if (m_Inspector != null && !string.IsNullOrEmpty(m_Inspector.Action.Name) && image != null)
            {
                if (m_Inspector.Set.Handle > 0)
                {
                    if (API.Input.Client.connectedControllers.Count > 0)
                    {
                        var textures = m_Inspector.Action.GetInputGlyphs(API.Input.Client.connectedControllers[0], m_Inspector.Set);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(m_Inspector.Layer.layerName))
                {
                    if (API.Input.Client.connectedControllers.Count > 0)
                    {
                        var textures = m_Inspector.Action.GetInputGlyphs(API.Input.Client.connectedControllers[0], m_Inspector.Layer);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
            }
        }
    }
}
#endif