#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Steamworks/Inventory Item")]
    [HelpURL("https://kb.heathen.group/steam/features/achievements")]
    public class SteamInventoryItemData : MonoBehaviour
    {
        public int id;
        public ItemData Data
        {
            get => id;
            set
            {
                id = value.id;
                if (m_Events != null)
                    m_Events.onChange?.Invoke();
            }
        }

        [SerializeField]
        private List<string> m_Delegates;
        private SteamInventoryItemDataEvents m_Events;

        private void Awake()
        {
            m_Events = GetComponent<SteamInventoryItemDataEvents>();
        }

        public void ConsumeOne()
        {
            var requestResult = Data.Consume(result =>
            {
                if (result.result != EResult.k_EResultOK)
                {
                    if (m_Events != null)
                        m_Events.onConsumeRequestFailed?.Invoke(result.result);
                }
                else if (m_Events != null)
                    m_Events.onConsumeRequestComplete?.Invoke(result.items);
            });
            if (!requestResult && m_Events != null)
                m_Events.onConsumeRequestRejected?.Invoke();
        }

        public void ConsumeMany(int quantity)
        {
            var requestResult = Data.Consume(Convert.ToUInt32(quantity), result =>
            {
                if (result.result != EResult.k_EResultOK)
                {
                    if (m_Events != null)
                        m_Events.onConsumeRequestFailed?.Invoke(result.result);
                }
                else if (m_Events != null)
                    m_Events.onConsumeRequestComplete?.Invoke(result.items);
            });

            if (!requestResult && m_Events != null)
                m_Events.onConsumeRequestRejected?.Invoke();
        }

        public void AddPromo()
        {
            var requestResult = Data.AddPromoItem(HandleAddPromoResults);
            if(!requestResult && m_Events != null)
                m_Events.onAddPromoRejected?.Invoke();
        }

        public void GetAll()
        {
            API.Inventory.Client.GetAllItems(null);
        }

        public void StartPurchase()
        {
            Data.StartPurchase((result, ioError) =>
            {
                if (!ioError && result.m_result == EResult.k_EResultOK)
                    m_Events.onPurchaseStarted?.Invoke(result);
                else
                    m_Events.onPurchaseStartFailed?.Invoke(result.m_result);
            });
        }

        private void HandleAddPromoResults(InventoryResult results)
        {
            if (results.result != EResult.k_EResultOK)
            {
                if (m_Events != null)
                    m_Events.onAddPromoFailed?.Invoke(results.result);
            }
            else if (m_Events != null)
                m_Events.onAddPromoComplete?.Invoke(results.items);
        }
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamInventoryItemData), true)]
    public class SteamInventoryItemDataEditor : ModularEditor
    {
        private string[] _options;
        private int[] _ids;
        private SteamToolsSettings.NameAndID[] _nameAndIds;
        private int _selectedIndex;
        private SerializedProperty idProp;
        private SteamToolsSettings settings;

        // --- Allowed types for this editor ---
        protected override Type[] AllowedTypes => new Type[]
        {            
            typeof(SteamInventoryItemQuantity),
            typeof(SteamInventoryItemName),
            typeof(SteamInventoryItemCurrentPrice),
            typeof(SteamInventoryItemBasePrice),
            typeof(SteamInventoryExchange),
            typeof(SteamInventoryItemDataEvents),
        };

        private void OnEnable()
        {
            idProp = serializedObject.FindProperty("id");
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            settings = SteamToolsSettings.GetOrCreate();
            var list = settings != null && settings.items != null
                ? settings.items
                : new List<SteamToolsSettings.NameAndID>();

            if (list.Count > 0)
            {
                _nameAndIds = list.ToArray();
                _options = new string[list.Count];
                _ids = new int[list.Count];

                for(int i = 0; i < list.Count; i++)
                {
                    _options[i] = _nameAndIds[i].name;
                    _ids[i] = _nameAndIds[i].id;
                }

                var current = idProp.intValue;
                _selectedIndex = Mathf.Max(0, System.Array.IndexOf(_ids, current));
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
            }
            else
            {
                _options = null;
            }
        }

        public override void OnInspectorGUI()
        {
            if (settings != null)
                settings = SteamToolsSettings.GetOrCreate();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Player/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.Get(settings.ActiveApp.Value).applicationId.ToString());
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/inventory");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // === Achievement dropdown ===
            if (_options == null || _options.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No items found!.\n\n" +
                    "Open Project Settings > Player > Steamworks to configure your inventory items.",
                    MessageType.Warning
                );

                serializedObject.ApplyModifiedProperties();
                return;
            }

            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _options);
            if (_selectedIndex >= 0 && _selectedIndex < _options.Length)
                idProp.intValue = _ids[_selectedIndex];

            EditorGUILayout.Space();

            // --- Features Dropdown ---
            HideAllAllowedComponents();
            DrawAddFieldDropdown();

            // --- Draw existing components via attributes ---
            EditorGUI.indentLevel++;
            DrawModularComponents();
            EditorGUI.indentLevel--;

            // --- Draw Functions as Flags (single-instance components) ---
            DrawFunctionFlags();

            // --- Draw Settings / Elements / Templates / Events ---
            DrawFields<SettingsFieldAttribute>("Settings");
            DrawFields<ElementFieldAttribute>("Elements");
            DrawFields<TemplateFieldAttribute>("Templates");
            DrawEventFields();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif