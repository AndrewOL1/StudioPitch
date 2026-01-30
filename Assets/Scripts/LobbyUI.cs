using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]GameObject lobbyUI;

    public void ToggleLobbyUI()
    {
        lobbyUI.SetActive(!lobbyUI.activeSelf);
    }
}
