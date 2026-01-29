#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/steam/features/lobby/unity-lobby/steam-lobby-response-display")]
    public class SteamLobbyResponseDisplay : MonoBehaviour
    {
        [Header("Configuration")]
        public float hideAfterSeconds = 0;

        public SteamText Success = new("Success");
        public SteamText DoesntExist = new("Chat doesn't exist (probably closed)");
        public SteamText NotAllowed = new("General Denied - You don't have the permissions needed to join the chat");
        public SteamText Full = new("Chat room has reached its maximum size");
        public SteamText Error = new("Unexpected Error");
        public SteamText Banned = new("You are banned from this chat room and may not join");
        public SteamText Limited = new("Joining this chat is not allowed because you are a limited user (no value on account)");
        public SteamText ClanDisabled = new("Attempt to join a clan chat when the clan is locked or disabled");
        public SteamText CommunityBan = new("Attempt to join a chat when the user has a community lock on their account");
        public SteamText MemberBlockedYou = new("Join failed - some member in the chat has blocked you from joining");
        public SteamText YouBlockedMember = new("Join failed - you have blocked some member already in the chat");
        public SteamText RateLimitExceeded = new("Join failed - to many join attempts in a very short period of time");

        [Header("Elements")]
        public TMPro.TMP_InputField outputElement;
        public GameObject displayElement;

        [Header("Events")]
        public UnityEvent onDisplay;
        public UnityEvent onHide;


        private void Start()
        {
            if (outputElement != null)
                outputElement.readOnly = true;
        }

        public string GetString(EChatRoomEnterResponse response)
        {
            switch (response)
            {
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned: return Banned;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseClanDisabled: return ClanDisabled;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan: return CommunityBan;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist: return DoesntExist;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseError: return Error;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseFull: return Full;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited: return Limited;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou: return MemberBlockedYou;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed: return NotAllowed;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded: return RateLimitExceeded;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess: return Success;
                case EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember: return YouBlockedMember;
                default: return string.Empty;
            }
        }

        public void DisplayResponse(EChatRoomEnterResponse response)
        {
            if (outputElement != null)
                outputElement.text = GetString(response);

            if (displayElement != null)
                displayElement.SetActive(true);

            onDisplay?.Invoke();

            if (hideAfterSeconds > 0)
            {
                CancelInvoke("HideDisplay");
                Invoke("HideDisplay", hideAfterSeconds);
            }
        }

        public void HideDisplay()
        {
            if (displayElement != null)
                displayElement.SetActive(false);

            onHide?.Invoke();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyResponseDisplay), true)]
    public class SteamLobbyResponseDisplayEditor : UnityEditor.Editor
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