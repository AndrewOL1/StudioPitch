#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Workshop Item")]
    [HelpURL("https://kb.heathen.group/steam/features/workshop")]
    public class SteamWorkshopItemDetailData : MonoBehaviour
    {
        public WorkshopItemDetails Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if(m_Events != null)
                    m_Events.onChange?.Invoke();
            }
        }

        private WorkshopItemDetails m_Data;
        private SteamWorkshopItemDetailDataEvents m_Events;

        [SerializeField]
        private List<string> m_Delegates = new();

        private void Awake()
        {
            m_Events = GetComponent<SteamWorkshopItemDetailDataEvents>();
        }

        public void Get(PublishedFileId_t fileId)
        {
            WorkshopItemDetails.Get(fileId, HandleItemGet);
        }

        public void LoadPreview()
        {
            if(m_Events != null
                && m_Data != null
                && m_Data.SourceItemDetails.m_nPreviewFileSize > 0)
            {
                m_Data.GetPreviewImage((name, data) =>
                {
                    m_Events.onPreviewImageLoaded?.Invoke(data);
                });
            }
        }

        public void Subscribe()
        {
            if (m_Data != null)
            {
                m_Data.Subscribe(HandleSubscribed);
            }
        }

        public void Unsubscribe()
        {
            if (m_Data != null)
            {
                m_Data.Unsubscribe(HandleUnsubscribe);
            }
        }

        public void DownloadItem()
        {
            if (m_Data != null)
            {
                m_Data.DownloadItem(false);
            }
        }

        public void DownloadItemHighPriority()
        {
            if (m_Data != null)
            {
                m_Data.DownloadItem(true);
            }
        }

        public void Delete()
        {
            if (m_Data != null)
            {
                m_Data.DeleteItem(HandleItemDelete);
            }
        }

        public void UpVote()
        {
            if (m_Data != null)
            {
                m_Data.SetVote(true, HandleVoteSet);
            }
        }

        public void DownVote()
        {
            if (m_Data != null)
            {
                m_Data.SetVote(false, HandleVoteSet);
            }
        }

        public void StartPlaytime()
        {
            if (m_Data != null)
            {
                m_Data.StartPlayTime(HandleStartPlaytime);
            }
        }

        public void StopPlaytime()
        {
            if (m_Data != null)
            {
                m_Data.StartPlayTime(HandleEndPlaytime);
            }
        }

        private void HandleItemGet(WorkshopItemDetails details)
        {
            Data = details;
        }

        private void HandleStartPlaytime(StartPlaytimeTrackingResult_t t, bool arg2)
        {
            if (m_Events != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    m_Events.onPlayStartedFailed?.Invoke(t.m_eResult);
                else
                    m_Events.onPlayStarted?.Invoke(m_Data.FileId);
            }
        }

        private void HandleEndPlaytime(StartPlaytimeTrackingResult_t t, bool arg2)
        {
            if (m_Events != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    m_Events.onPlayEndedFailed?.Invoke(t.m_eResult);
                else
                    m_Events.onPlayEnded?.Invoke(m_Data.FileId);
            }
        }

        private void HandleVoteSet(SetUserItemVoteResult_t t, bool arg2)
        {
            if (m_Events != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    m_Events.onVoteSetFailed?.Invoke(t.m_eResult);
                else
                    m_Events.onVoteSet?.Invoke(t.m_nPublishedFileId);
            }
        }

        private void HandleItemDelete(DeleteItemResult_t t, bool arg2)
        {
            if (m_Events != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    m_Events.onDeleteFailed?.Invoke(t.m_eResult);
                else
                    m_Events.onDelete?.Invoke(t.m_nPublishedFileId);
            }
        }

        private void HandleUnsubscribe(RemoteStorageUnsubscribePublishedFileResult_t t, bool arg2)
        {
            if (m_Events != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    m_Events.onUnsubscribeFailed?.Invoke(t.m_eResult);
                else
                    m_Events.onUnsubscribed?.Invoke(t.m_nPublishedFileId);
            }
        }

        private void HandleSubscribed(RemoteStorageSubscribePublishedFileResult_t t, bool arg2)
        {
            if (m_Events != null)
            {
                if (arg2 || t.m_eResult != EResult.k_EResultOK)
                    m_Events.onSubscribeFailed?.Invoke(t.m_eResult);
                else
                    m_Events.onSubscribed?.Invoke(t.m_nPublishedFileId);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteamWorkshopItemDetailData), true)]
    public class SteamWorkshopItemDetailDataEditor : ModularEditor
    {
        private SteamToolsSettings settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamWorkshopItemDetailTitle),
            typeof(SteamWorkshopItemDetailDescription),
            typeof(SteamWorkshopItemDetailRatingFill),
            typeof(SteamWorkshopItemDetailUpVoteLabel),
            typeof(SteamWorkshopItemDetailDownVoteLabel),
            typeof(SteamWorkshopItemDetailTotalVotesLabel),
            typeof(SteamWorkshopItemDetailCreatedData),
            typeof(SteamWorkshopItemDetailModifiedDate),
            typeof(SteamWorkshopItemDetailEdit),
            typeof(SteamWorkshopItemDetailDataEvents),
        };

        private void OnEnable()
        {
            settings = SteamToolsSettings.GetOrCreate();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{settings.Get(settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/workshop"
                , "https://discord.gg/heathen-group-463483739612381204"
                , null);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif