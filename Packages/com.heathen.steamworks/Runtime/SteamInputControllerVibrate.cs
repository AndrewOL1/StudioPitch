#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [RequireComponent(typeof(SteamInputControllerData))]
    public class SteamInputControllerVibrate : MonoBehaviour
    {
        [SettingsField(header = "Vibrate")]
        [Range(0, 1)] 
        public float left = 0f;
        [SettingsField(header = "Vibrate")]
        [Range(0, 1)]
        public float right = 0f;

        private SteamInputControllerData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamInputControllerData>();
        }

        private void Update()
        {
            if(m_Inspector.Data.HasValue)
            API.Input.Client.TriggerVibration(m_Inspector.Data.Value, (ushort)Mathf.Lerp(0, ushort.MaxValue, left), (ushort)Mathf.Lerp(0, ushort.MaxValue, right));
        }
    }
}
#endif