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
    [ModularComponent(typeof(SteamInventoryItemData), "Exchange", "")]
    [RequireComponent(typeof(SteamInventoryItemData))]
    [AddComponentMenu("")]
    public class SteamInventoryExchange : MonoBehaviour
    {
        [Serializable]
        public struct RecipeEntry
        {
            public int id;
            public uint count;
        }

        [SettingsField]
        public List<RecipeEntry> recipe = new();

        public bool IsCanExchange
        {
            get
            {
                if (recipe.Count == 0)
                {
                    return false;
                }

                bool allPass = true;
                foreach (var entry in recipe)
                {
                    ItemData item = entry.id;
                    if (!item.GetExchangeEntry(entry.count, out var _))
                    {
                        allPass = false;
                        break;
                    }
                }

                return allPass;
            }
        }

        private SteamInventoryItemData m_Inspector;
        private SteamInventoryItemDataEvents m_Events;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamInventoryItemData>();
            m_Events = GetComponent<SteamInventoryItemDataEvents>();

            API.Inventory.Client.OnSteamInventoryResultReady.AddListener(HandleInventoryUpdated);

            SteamTools.Interface.WhenReady(RefreshCanExchange);
        }

        private void OnDestroy()
        {
            API.Inventory.Client.OnSteamInventoryResultReady.RemoveListener(HandleInventoryUpdated);
        }

        private void HandleInventoryUpdated(InventoryResult _)
        {
            RefreshCanExchange();
        }

        public void RefreshCanExchange()
        {
            if (m_Events != null)
                m_Events.onCanExchangeChange?.Invoke(IsCanExchange);
        }

        public void Exchange()
        {
            List<ExchangeEntry> exchangeReagents = new();
            bool allPass = true;
            foreach (var entry in recipe)
            {
                ItemData item = entry.id;
                if (item.GetExchangeEntry(entry.count, out var reagents))
                {
                    exchangeReagents.AddRange(reagents);
                }
                else
                {
                    allPass = false;
                    break;
                }
            }

            if (allPass)
                m_Inspector.Data.Exchange(exchangeReagents, result =>
                {
                    if (result.result == EResult.k_EResultOK)
                    {
                        if (m_Events != null)
                            m_Events.onExchangeComplete?.Invoke(result.items);
                    }
                    else if (m_Events != null)
                        m_Events.onExchangeFailed?.Invoke(result.result);
                });
            else if (m_Events != null)
                m_Events.onExchangeRejected?.Invoke();
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SteamInventoryExchange.RecipeEntry))]
    public class RecipeEntryDrawer : PropertyDrawer
    {
        private string[] _options;
        private int[] _ids;
        private int _selectedIndex;
        private SteamToolsSettings settings;
        private bool _initialized;

        private void Init(SerializedProperty property)
        {
            if (_initialized) return;
            _initialized = true;

            settings = SteamToolsSettings.GetOrCreate();
            var list = settings != null && settings.items != null
                ? settings.items
                : new List<SteamToolsSettings.NameAndID>();

            if (list.Count > 0)
            {
                _options = new string[list.Count];
                _ids = new int[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    _options[i] = list[i].name;
                    _ids[i] = list[i].id;
                }

                var idProp = property.FindPropertyRelative("id");
                int current = idProp.intValue;
                _selectedIndex = Mathf.Max(0, System.Array.IndexOf(_ids, current));
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
            }
            else
            {
                _options = new string[] { "No Items Configured" };
                _ids = new int[] { 0 };
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            var idProp = property.FindPropertyRelative("id");
            var countProp = property.FindPropertyRelative("count");

            EditorGUI.BeginProperty(position, label, property);

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, label);

            EditorGUI.indentLevel++;
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            // Item dropdown
            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUI.Popup(rect, "Item", _selectedIndex, _options);
            if (EditorGUI.EndChangeCheck() && _selectedIndex >= 0 && _selectedIndex < _ids.Length)
                idProp.intValue = _ids[_selectedIndex];

            // Count field
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, countProp);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 3 lines: label + dropdown + count
            return EditorGUIUtility.singleLineHeight * 3 + 6;
        }
    }
#endif
}
#endif