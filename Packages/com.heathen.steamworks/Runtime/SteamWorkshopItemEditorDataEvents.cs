#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamWorkshopItemEditorData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemEditorData))]
    public class SteamWorkshopItemEditorDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange;
        [EventField]
        public UnityEvent onCreateUpdateSuccess;
        [EventField]
        public UnityEvent<EResult, string> onCreateUpdateError;
        [EventField]
        public UnityEvent onUserNeedsToAcceptWorkshopAgreement;
    }
}
#endif