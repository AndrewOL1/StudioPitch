#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using Steamworks;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [CustomPropertyDrawer(typeof(ERemoteStoragePublishedFileVisibility))]
    public class SteamVisibilityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = System.Enum.GetNames(typeof(ERemoteStoragePublishedFileVisibility))
                .Select(n => n.Replace("k_ERemoteStoragePublishedFileVisibility", ""))
                .ToArray();

            var values = System.Enum.GetValues(typeof(ERemoteStoragePublishedFileVisibility))
                .Cast<int>()
                .ToArray();

            int index = System.Array.IndexOf(values, property.intValue);
            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            if (newIndex >= 0 && newIndex < values.Length)
                property.intValue = values[newIndex];
        }
    }

    [CustomPropertyDrawer(typeof(EItemPreviewType))]
    public class ItemPreviewTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var names = System.Enum.GetNames(typeof(EItemPreviewType))
                .Select(n => n.Replace("k_EItemPreviewType_", ""))
                .ToArray();

            var values = System.Enum.GetValues(typeof(EItemPreviewType))
                .Cast<int>()
                .ToArray();

            int index = System.Array.IndexOf(values, property.intValue);
            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            if (newIndex >= 0 && newIndex < values.Length)
                property.intValue = values[newIndex];
        }
    }

    [CustomPropertyDrawer(typeof(EResult))]
    public class EResultDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get enum names and remove Steam prefix
            var names = System.Enum.GetNames(typeof(EResult))
                .Select(n => n.Replace("k_EResult", ""))
                .ToArray();

            var values = System.Enum.GetValues(typeof(EResult))
                .Cast<int>()
                .ToArray();

            int index = System.Array.IndexOf(values, property.intValue);
            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            if (newIndex >= 0 && newIndex < values.Length)
                property.intValue = values[newIndex];
        }
    }
}
#endif
#endif