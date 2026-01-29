#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Provides Lobby Metadata access to the Unity Inspector
    /// </summary>
    [ModularComponent(typeof(SteamLobbyData), "Metadata", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyMetadata : MonoBehaviour
    {
        /// <summary>
        /// Defines a key and string event used to track changes to lobby metadata
        /// </summary>
        [Serializable]
        public struct KeyEventMap
        {
            public string key;
            public UnityEvent<string> onUpdate;
            [HideInInspector]
            [NonSerialized]
            public string previousValue;
        }
        /// <summary>
        /// A collection of key and values to be set when the Set() function is called.
        /// </summary>
        [SettingsField(header = "Metadata")]
        [Tooltip("A collection of key and values to be set when the Set() function is called.")]
        public List<StringKeyValuePair> dataToSet = new();
        /// <summary>
        /// A collection of key and event, the events will be invoked when the key's data changes on the lobby.
        /// </summary>
        [SettingsField(header = "Metadata")]
        [Tooltip("A collection of key and event, the events will be invoked when the key's data changes on the lobby.")]
        public List<KeyEventMap> onChanged = new();

        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleOnChanged);
            API.Matchmaking.Client.OnLobbyDataUpdate.AddListener(HandleMetadataChange);
            RefreshKeyValues();
        }

        private void HandleMetadataChange(LobbyDataUpdateEventData arg0)
        {
            if(arg0.lobby == m_Inspector.Data)
            {
                for (int i = 0; i < onChanged.Count; i++)
                {
                    var map = onChanged[i];
                    var currentValue = m_Inspector.Data[map.key];
                    if (currentValue != map.previousValue)
                    {
                        map.previousValue = currentValue;
                        onChanged[i] = map;
                        map.onUpdate?.Invoke(map.previousValue);
                    }
                }
            }
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            RefreshKeyValues();
        }

        /// <summary>
        /// Refresh the stored values for all On Changed keys.
        /// This will automatically be called when the lobby data is updated.
        /// </summary>
        public void RefreshKeyValues()
        {
            for (int i = 0; i < onChanged.Count; i++)
            {
                var map = onChanged[i];
                if (m_Inspector.Data.IsValid)
                    map.previousValue = m_Inspector.Data[map.key];
                else
                    map.previousValue = string.Empty;

                onChanged[i] = map;
                map.onUpdate?.Invoke(map.previousValue);
            }
        }

        /// <summary>
        /// Set the metadata on the lobby for all Data to Set entries
        /// This is not automatically called but can be connected to Steam Lobby Data Events On Create or similar to effectively call it on creation of a lobby.
        /// </summary>
        public void Set()
        {
            if (m_Inspector.Data.IsValid
                && dataToSet.Count > 0
                && m_Inspector.Data.IsOwner)
            {
                foreach (var kvp in dataToSet)
                {
                    API.Matchmaking.Client.SetLobbyData(m_Inspector.Data, kvp.key, kvp.value);
                }
            }
        }

        /// <summary>
        /// Set a key and value on the lobby metadata
        /// This will not modify the Data to Set collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            if (m_Inspector.Data.IsValid)
            {
                if (m_Inspector.Data.IsOwner)
                    API.Matchmaking.Client.SetLobbyData(m_Inspector.Data, key, value);
                else
                    Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] Only the owner can set data");
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] No lobby to set");
            }
        }

        /// <summary>
        /// Set a key and value on the lobby metadata
        /// This will not modify the Data to Set collection
        /// </summary>
        /// <param name="data"></param>
        public void Set(StringKeyValuePair data)
        {
            Set(data.key, data.value);
        }

        /// <summary>
        /// Get the value of a given metadata key
        /// </summary>
        /// <param name="key">The key of the data to read</param>
        /// <returns>The string value of the key</returns>
        public string Get(string key)
        {
            if (m_Inspector.Data.IsValid)
                return m_Inspector.Data[key];
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] No lobby to read");
                return string.Empty;
            }
        }

        /// <summary>
        /// Does this lobby have a value for this key
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if it has a value and is not string empty, else false</returns>
        public bool HasKey(string key)
        {
            if (m_Inspector.Data.IsValid)
                return !string.IsNullOrEmpty(m_Inspector.Data[key]);
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyMetadata)}] No lobby to test");
                return false;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMetadata), true)]
    public class SteamLobbyMetadataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif