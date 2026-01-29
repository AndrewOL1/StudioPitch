#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/Initialize Steam")]
    [HelpURL("https://kb.heathen.group/steamworks/initialization/unity-initialization#component")]
    [DisallowMultipleComponent]
    public class InitializeSteamworks : MonoBehaviour
    {
        private void Start()
        {
            SteamTools.Interface.Initialize();
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(InitializeSteamworks), true)]
        public class InitializeSteamworksEditor : UnityEditor.Editor
        {
            private SteamToolsSettings settings;

            private void OnEnable()
            {
                settings = SteamToolsSettings.GetOrCreate();
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.LinkButton("Settings"))
                    SettingsService.OpenProjectSettings("Project/Player/Steamworks");
                if (EditorGUILayout.LinkButton("Portal"))
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.Get(settings.ActiveApp.Value).applicationId);
                if (EditorGUILayout.LinkButton("Guide"))
                    Application.OpenURL("https://kb.heathen.group/steam");
                if (EditorGUILayout.LinkButton("Support"))
                    Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
                EditorGUILayout.EndHorizontal();
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
#endif