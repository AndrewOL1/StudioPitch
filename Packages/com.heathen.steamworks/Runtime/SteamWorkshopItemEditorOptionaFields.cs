#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemEditorData), "Create & Update", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemEditorData))]
    public class SteamWorkshopItemEditorCreateAndUpdate : MonoBehaviour
    {
        /// <summary>
        /// Any metadata associated with the item, this is optional
        /// </summary>
        [SettingsField(header = "Optional")]
        public string metadata;
        /// <summary>
        /// The YouTube video ID of additional preview videos
        /// </summary>]
        [SettingsField(header = "Optional")]
        public string[] additionalYouTubeIds;
        /// <summary>
        /// Additional preview images
        /// </summary>
        [SettingsField(header = "Optional")]
        public WorkshopItemPreviewFile[] additionalPreviews;
        /// <summary>
        /// additional KVP tags
        /// </summary>
        [SettingsField(header = "Optional")]
        public WorkshopItemKeyValueTag[] additionalKeyValueTags;
        /// <summary>
        /// Any tags associated with the item, this is optional
        /// </summary>
        [SettingsField(header = "Optional")]
        public string[] tags;
        

        private SteamWorkshopItemEditorData m_Inspector;
        private SteamWorkshopItemEditorDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamWorkshopItemEditorData>();
            m_Events = GetComponent<SteamWorkshopItemEditorDataEvents>();
        }

        public void CreateNew()
        {
            var data = m_Inspector.Data;
            data.visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
            data.metadata = metadata;
            data.tags = tags;
            m_Inspector.Data = data;

            data.Create(additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, HandleCompleted, HandleUploaded, HandleFileCreated);
        }

        public void CreateOrUpdate()
        {
            var data = m_Inspector.Data;
            data.metadata = metadata;
            data.tags = tags;
            m_Inspector.Data = data;

            if (data.publishedFileId.HasValue)
                data.Update(HandleUpdateCompleted, HandleUploaded);
            else
            {
                data.visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
                data.Create(additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, HandleCompleted, HandleUploaded, HandleFileCreated);
            }
        }

        private void HandleUpdateCompleted(WorkshopItemDataUpdateStatus status)
        {
            m_Inspector.Data = status.data;

            if (status.hasError)
            {
                if (m_Events != null)
                {
                    EResult resultCode = EResult.k_EResultFail;
                    if (status.submitItemUpdateResult.HasValue)
                        resultCode = status.submitItemUpdateResult.Value.m_eResult;

                    m_Events.onCreateUpdateError?.Invoke(resultCode, status.errorMessage);
                }
            }
            else
            {
                if (m_Events != null)
                {
                    m_Events.onCreateUpdateSuccess?.Invoke();
                }
            }
        }

        private void HandleFileCreated(CreateItemResult_t t)
        {
            if (t.m_eResult == EResult.k_EResultOK)
            {
                var data = m_Inspector.Data;
                data.publishedFileId = t.m_nPublishedFileId;
            }
        }

        private void HandleUploaded(UGCUpdateHandle_t t)
        {
            // Future use
        }

        private void HandleCompleted(WorkshopItemDataCreateStatus status)
        {
            m_Inspector.Data = status.data;

            if(status.hasError)
            {
                EResult resultCode = EResult.k_EResultFail;
                if (status.submitItemUpdateResult.HasValue)
                    resultCode = status.submitItemUpdateResult.Value.m_eResult;
                else
                if (status.createItemResult.HasValue)
                    resultCode = status.createItemResult.Value.m_eResult;

                if (m_Events != null)
                    m_Events.onCreateUpdateError?.Invoke(resultCode, status.errorMessage);
            }
            else
            {
                if (m_Events != null)
                {
                    m_Events.onCreateUpdateSuccess?.Invoke();
                    if (status.createItemResult.Value.m_bUserNeedsToAcceptWorkshopLegalAgreement)
                        m_Events.onUserNeedsToAcceptWorkshopAgreement?.Invoke();
                }
            }
        }
    }
}
#endif