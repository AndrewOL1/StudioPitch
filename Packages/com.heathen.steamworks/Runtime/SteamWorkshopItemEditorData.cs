#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Workshop Item Editor")]
    [HelpURL("https://kb.heathen.group/steam/features/workshop")]
    public class SteamWorkshopItemEditorData : MonoBehaviour
    {
        /// <summary>
        /// The id of the application that will use this content
        /// </summary>
        public uint consumingAppId;
        /// <summary>
        /// The title of the item
        /// </summary>
        public TMPro.TMP_InputField title;
        /// <summary>
        /// The description of the item
        /// </summary>
        public TMPro.TMP_InputField description;
        /// <summary>
        /// The local folder where the item's content is located
        /// </summary>
        public TMPro.TMP_InputField contentFolderPath;
        /// <summary>
        /// The local file that is the item's main preview image, this must be smaller than the max size allowed in the app's Cloud Storage
        /// </summary>
        public TMPro.TMP_InputField previewFilePath;

        public WorkshopItemEditorData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if (m_Events != null)
                    m_Events.onChange?.Invoke();
            }
        }

        private WorkshopItemEditorData m_Data;
        private SteamWorkshopItemEditorDataEvents m_Events;
        [SerializeField]
        private List<string> m_Delegates = new();

        private void Awake()
        {
            m_Events = GetComponent<SteamWorkshopItemEditorDataEvents>();

            if (title != null)
                title.onValueChanged.AddListener(HandleTitleUpdate);

            if (description != null)
                description.onValueChanged.AddListener(HandleDescriptionUpdate);

            if (contentFolderPath != null)
                contentFolderPath.onValueChanged.AddListener(HandleContentFolderUpdate);

            if (previewFilePath != null)
                previewFilePath.onValueChanged.AddListener(HandlePreviewFileUpdate);
        }

        private void HandleTitleUpdate(string arg0)
        {
            m_Data.title = arg0;
        }

        private void HandleDescriptionUpdate(string arg0)
        {
            m_Data.description = arg0;
        }

        private void HandleContentFolderUpdate(string arg0)
        {
            m_Data.content = new(arg0);
        }

        private void HandlePreviewFileUpdate(string arg0)
        {
            m_Data.preview = new(arg0);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SteamWorkshopItemEditorData), true)]
    public class SteamWorkshopItemEditorDataEditor : ModularEditor
    {
        private SteamToolsSettings settings;
        private SerializedProperty title;
        private SerializedProperty description;
        private SerializedProperty contentFolderPath;
        private SerializedProperty previewFilePath;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamWorkshopItemEditorCreateAndUpdate),
            typeof(SteamWorkshopItemEditorDataEvents),
        };

        private void OnEnable()
        {
            settings = SteamToolsSettings.GetOrCreate();
            title = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.title));
            description = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.description));
            contentFolderPath = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.contentFolderPath));
            previewFilePath = serializedObject.FindProperty(nameof(SteamWorkshopItemEditorData.previewFilePath));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefault(
                "Project/Player/Steamworks"
                , $"https://partner.steamgames.com/apps/landing/{settings.Get(settings.ActiveApp.Value).applicationId}"
                , "https://kb.heathen.group/steam/features/workshop"
                , "https://discord.gg/heathen-group-463483739612381204"
                , new[] { title, description, contentFolderPath, previewFilePath });

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif