#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class SteamLobbyPlayButtons : MonoBehaviour
    {
        public GameObject playUiElement;
        public GameObject waitingUiElement;
        public GameObject leaveUiElement;

        private void Awake()
        {
            API.Matchmaking.Client.OnLobbyEnterSuccess?.AddListener(HandleLobbyEnter);
            API.Matchmaking.Client.OnLobbyLeave?.AddListener(HandleLobbyLeave);
        }

        private void OnDestroy()
        {
            API.Matchmaking.Client.OnLobbyEnterSuccess?.RemoveListener(HandleLobbyEnter);
            API.Matchmaking.Client.OnLobbyLeave?.RemoveListener(HandleLobbyLeave);
        }

        private void HandleLobbyEnter(LobbyEnter_t arg0) => UpdateLobbyButtons();

        private void HandleLobbyLeave(LobbyData arg0) => UpdateLobbyButtons();

        private void UpdateLobbyButtons()
        {
            if(LobbyData.PartyLobby(out var partyLobby) && !partyLobby.IsOwner)
            {
                // We are a party member, so we just wait
                waitingUiElement.SetActive(true);
                playUiElement.SetActive(false);
                leaveUiElement.SetActive(false);
            }
            else
            {
                // We are either solo or a party leader, so we play & leave
                waitingUiElement.SetActive(false);
                if (LobbyData.SessionLobby(out var _))
                {
                    // We dont have a lobby yet so show the play option
                    playUiElement.SetActive(false);
                    leaveUiElement.SetActive(true);
                }
                else
                {
                    // We are in a lobby so show the leave option
                    playUiElement.SetActive(true);
                    leaveUiElement.SetActive(false);
                }
            }
        }
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyPlayButtons), true)]
    public class SteamLobbyPlayButtonsEditor : UnityEditor.Editor
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