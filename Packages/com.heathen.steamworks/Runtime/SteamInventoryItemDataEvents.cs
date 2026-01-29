#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamInventoryItemData))]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryItemDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange = new();
        [EventField]
        public UnityEvent onStateChanged = new();
        [EventField]
        public UnityEvent<ItemDetail[]> onConsumeRequestComplete = new();
        [EventField]
        public UnityEvent<SteamInventoryStartPurchaseResult_t> onPurchaseStarted = new();
        [EventField]
        public UnityEvent<ulong> onOrderAuthorized = new();
        [EventField]
        public UnityEvent<ulong> onOrderNotAuthorized = new();
        [EventField]
        public UnityEvent<ItemDetail[]> onAddPromoComplete = new();
        [EventField]
        public UnityEvent<bool> onCanExchangeChange = new();
        [EventField]
        public UnityEvent<ItemDetail[]> onExchangeComplete = new();

        [EventField]
        public UnityEvent onConsumeRequestRejected = new();
        [EventField]
        public UnityEvent<EResult> onConsumeRequestFailed = new();
        [EventField]
        public UnityEvent<EResult> onPurchaseStartFailed = new();
        [EventField]
        public UnityEvent onAddPromoRejected = new();
        [EventField]
        public UnityEvent<EResult> onAddPromoFailed = new();
        [EventField]
        public UnityEvent onExchangeRejected = new();
        [EventField]
        public UnityEvent<EResult> onExchangeFailed = new();

        private SteamInventoryItemData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamInventoryItemData>();
            API.Inventory.Client.OnSteamMicroTransactionAuthorizationResponse.AddListener(HandleTransactionAuth);
            API.Inventory.Client.OnSteamInventoryResultReady.AddListener(HandleInvResultReady);
        }

        private void OnDestroy()
        {
            API.Inventory.Client.OnSteamMicroTransactionAuthorizationResponse.RemoveListener(HandleTransactionAuth);
            API.Inventory.Client.OnSteamInventoryResultReady.RemoveListener(HandleInvResultReady);
        }

        private void HandleTransactionAuth(AppId_t arg0, ulong arg1, bool arg2)
        {
            if(arg0 == AppData.Me)
            {
                if (arg2)
                    onOrderAuthorized?.Invoke(arg1);
                else
                    onOrderNotAuthorized?.Invoke(arg1);
            }
        }

        private void HandleInvResultReady(InventoryResult arg0)
        {
            if(arg0.result == EResult.k_EResultOK)
            {
                bool found = false;
                foreach(var item in arg0.items)
                {
                    if(item.Definition == m_Inspector.Data)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    onStateChanged?.Invoke();
            }
        }
    }
}
#endif