#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamWorkshopItemDetailData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent onChange;

        [EventField]
        public UnityEvent<PublishedFileId_t> onSubscribed = new();        
        [EventField]
        public UnityEvent<PublishedFileId_t> onUnsubscribed = new();        
        [EventField]
        public UnityEvent<PublishedFileId_t> onDelete = new();        
        [EventField]
        public UnityEvent<PublishedFileId_t> onVoteSet = new();        
        [EventField]
        public UnityEvent<PublishedFileId_t> onPlayStarted = new();        
        [EventField]
        public UnityEvent<PublishedFileId_t> onPlayEnded = new();
        [EventField]
        public UnityEvent<PublishedFileId_t> onEdited = new();


        [EventField]
        public UnityEvent<EResult> onSubscribeFailed = new();
        [EventField]
        public UnityEvent<EResult> onUnsubscribeFailed = new();
        [EventField]
        public UnityEvent<EResult> onDeleteFailed = new();
        [EventField]
        public UnityEvent<EResult> onVoteSetFailed = new();
        [EventField]
        public UnityEvent<EResult> onPlayStartedFailed = new();
        [EventField]
        public UnityEvent<EResult> onPlayEndedFailed = new();
        [EventField]
        public UnityEvent<EResult> onEditFailed = new();

        [EventField]
        public UnityEvent<bool> onSetIsSubscribed = new();
        [EventField]
        public UnityEvent<bool> onSetIsNotSubscribed = new();

        [EventField]
        public UnityEvent<bool> onSetIsInstalled = new();
        [EventField]
        public UnityEvent<bool> onSetIsNotInstalled = new();

        [EventField]
        public UnityEvent<byte[]> onPreviewImageLoaded = new();

        private SteamWorkshopItemDetailData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamWorkshopItemDetailData>();
            onChange.AddListener(HandleOnChange);
        }

        private void HandleOnChange()
        {
            if (m_Inspector.Data != null)
            {
                onSetIsSubscribed?.Invoke(m_Inspector.Data.IsSubscribed);
                onSetIsNotSubscribed?.Invoke(!m_Inspector.Data.IsSubscribed);
                onSetIsInstalled?.Invoke(m_Inspector.Data.IsInstalled);
                onSetIsNotInstalled?.Invoke(!m_Inspector.Data.IsInstalled);
            }
        }
    }
}
#endif