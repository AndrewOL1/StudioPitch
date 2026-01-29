#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System.IO;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamWorkshopItemDetailData), "Edit", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamWorkshopItemDetailDataEvents))]
    [RequireComponent(typeof(SteamWorkshopItemDetailData))]
    public class SteamWorkshopItemDetailEdit : MonoBehaviour
    {
        [SettingsField(header = "Editor")]
        public SteamWorkshopItemEditorData component;
        [SettingsField(header = "Quick Edits")]
        public TMPro.TMP_InputField changeNote;
        [SettingsField(header = "Quick Edits")]
        public TMPro.TMP_InputField title;
        [SettingsField(header = "Quick Edits")]
        public TMPro.TMP_InputField description;
        [SettingsField(header = "Quick Edits")]
        public TMPro.TMP_InputField contentFolder;
        [SettingsField(header = "Quick Edits")]
        public TMPro.TMP_InputField previewImageFile;
        [SettingsField(header = "Quick Edits")]
        public TMPro.TMP_InputField metadata;

        private SteamWorkshopItemDetailData m_Inspector;
        private SteamWorkshopItemDetailDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamWorkshopItemDetailData>();
            m_Events = GetComponent<SteamWorkshopItemDetailDataEvents>();

            m_Events.onChange.AddListener(HandleOnChanged);
        }

        private void HandleOnChanged()
        {
            if (title != null)
                title.text = m_Inspector.Data != null ? m_Inspector.Data.Title : string.Empty;
            if (description != null)
                description.text = m_Inspector.Data != null ? m_Inspector.Data.Title : string.Empty;
            if (metadata != null)
                metadata.text = m_Inspector.Data != null ? m_Inspector.Data.Title : string.Empty;
        }

        private string GetChangeNote()
        {
            if(changeNote != null)
                return changeNote.text;
            else
                return string.Empty;
        }

        public void SetEditor()
        {
            if (m_Inspector.Data != null && component != null)
            {
                component.Data = new()
                {
                    appId = m_Inspector.Data.ConsumerApp,
                    description = m_Inspector.Data.Description,
                    metadata = m_Inspector.Data.metadata,
                    title = m_Inspector.Data.Title,
                    publishedFileId = m_Inspector.Data.FileId,
                    visibility = m_Inspector.Data.Visibility,
                    tags = m_Inspector.Data.Tags,
                    content = m_Inspector.Data.FolderPath,
                };
            }
        }

        public void UpdateTitle()
        {
            if (m_Inspector.Data != null
                && title != null
                && !string.IsNullOrEmpty(title.text))
            {
                m_Inspector.Data.UpdateTitle(title.text, GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateDescription()
        {
            if (m_Inspector.Data != null
                && description != null
                && !string.IsNullOrEmpty(description.text))
            {
                m_Inspector.Data.UpdateDescription(description.text, GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateContent()
        {
            if (m_Inspector.Data != null
                && contentFolder != null
                && !string.IsNullOrEmpty(contentFolder.text)
                && Directory.Exists(contentFolder.text))
            {
                m_Inspector.Data.UpdateContent(new(contentFolder.text), GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdatePreviewImage()
        {
            if (m_Inspector.Data != null
                && previewImageFile != null
                && !string.IsNullOrEmpty(previewImageFile.text)
                && File.Exists(previewImageFile.text))
            {
                m_Inspector.Data.UpdatePreviewImage(new(previewImageFile.text), GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateMetadata()
        {
            if (m_Inspector.Data != null
                && metadata != null
                && !string.IsNullOrEmpty(metadata.text))
            {
                m_Inspector.Data.UpdateMetadata(metadata.text, GetChangeNote(), HandleEditResult);
            }
        }

        public void UpdateAll()
        {
            if (m_Inspector.Data == null)
                return;

            if ((title == null || string.IsNullOrEmpty(title.text))
                && (description == null || string.IsNullOrEmpty(description.text))
                && (contentFolder == null || string.IsNullOrEmpty(contentFolder.text) || !Directory.Exists(contentFolder.text))
                && (previewImageFile == null || string.IsNullOrEmpty(previewImageFile.text) || !File.Exists(previewImageFile.text))
                && (metadata == null || string.IsNullOrEmpty(metadata.text)))
                return;

            var handle = API.UserGeneratedContent.Client.StartItemUpdate(m_Inspector.Data.ConsumerApp, m_Inspector.Data.FileId);

            if (title != null && !string.IsNullOrEmpty(title.text))
                SteamUGC.SetItemTitle(handle, title.text);

            if (description != null && !string.IsNullOrEmpty(description.text))
                SteamUGC.SetItemDescription(handle, description.text);

            if (contentFolder != null && !string.IsNullOrEmpty(contentFolder.text) && Directory.Exists(contentFolder.text))
                SteamUGC.SetItemContent(handle, new DirectoryInfo(contentFolder.text).FullName);

            if (previewImageFile != null && !string.IsNullOrEmpty(previewImageFile.text) && File.Exists(previewImageFile.text))
                SteamUGC.SetItemPreview(handle, new FileInfo(previewImageFile.text).FullName);

            if (metadata != null && !string.IsNullOrEmpty(metadata.text))
                SteamUGC.SetItemMetadata(handle, metadata.text);


            API.UserGeneratedContent.Client.SubmitItemUpdate(handle, GetChangeNote(), HandleEditResult);
            
        }

        private void HandleEditResult(SubmitItemUpdateResult_t t, bool arg2)
        {
            if(m_Events != null)
            {
                if (!arg2 && t.m_eResult == EResult.k_EResultOK)
                    m_Events.onEdited?.Invoke(t.m_nPublishedFileId);
                else
                    m_Events.onEditFailed?.Invoke(t.m_eResult);
            }
        }
    }
}
#endif