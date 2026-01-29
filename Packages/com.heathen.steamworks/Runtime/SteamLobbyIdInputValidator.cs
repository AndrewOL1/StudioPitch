#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-input-validator")]
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class SteamLobbyIdInputValidator : MonoBehaviour
    {
        [Header("Configuration")]
        public int minimalIdLength = 8;

        [Header("Events")]
        public UnityEvent OnValid;

        private TMPro.TMP_InputField m_InputField;

        private void Awake()
        {
            m_InputField = GetComponent<TMPro.TMP_InputField>();
            m_InputField.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDestroy()
        {
            m_InputField.onValueChanged.RemoveListener(HandleValueChanged);
        }

        private void HandleValueChanged(string arg0)
        {
            if (arg0.Length >= minimalIdLength)
            {
                var lobby = LobbyData.Get(arg0);
                if (lobby.IsValid)
                    OnValid?.Invoke();
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyIdInputValidator), true)]
    public class SteamLobbyIdInputValidatorEditor : UnityEditor.Editor
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