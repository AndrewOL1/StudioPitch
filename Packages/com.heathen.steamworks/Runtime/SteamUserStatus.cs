#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamUserData), "Status", nameof(settings))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserStatus : MonoBehaviour
    {
        [Serializable]
        public class Options
        {
            [Serializable]
            public class References
            {
                public Sprite icon;
                public bool setIconColor;
                public Color iconColor;
                [Tooltip("You can use %gameName% and it will be replaced with the name of the game the player is currently playing. This is only relivent for In This Game and In Another Game options.")]
                public SteamText message;
                public bool setMessageColor;
                public Color messageColor;

                public void Set(Image image, TextMeshProUGUI label, FriendGameInfo_t? gameInfo)
                {
                    if (image != null)
                    {
                        image.gameObject.SetActive(true);
                        image.sprite = icon;
                        if (setIconColor)
                            image.color = iconColor;
                    }

                    if (message != null)
                    {
                        if (gameInfo.HasValue)
                        {
                            AppData.LoadNames(() =>
                            {
                                AppData appData = gameInfo.Value.m_gameID;
                                label.text = message.Get().Replace("%gameName%", appData.Name);
                            });

                        }
                        else
                        {
                            label.text = message;
                        }

                        if (setMessageColor)
                            label.color = messageColor;
                    }
                }
            }

            public References InThisGame = new References
            {
                setIconColor = false,
                iconColor = new Color(0.8862f, 0.9960f, 0.7568f, 1),
                message = new("Playing %gameName%"),
                setMessageColor = false,
                messageColor = new Color(0.8862f, 0.9960f, 0.7568f, 1)
            };
            public References InAnotherGame = new References
            {
                setIconColor = false,
                iconColor = new Color(0.5686f, 0.7607f, 0.3411f, 1),
                message = new("Playing %gameName%"),
                setMessageColor = false,
                messageColor = new Color(0.5686f, 0.7607f, 0.3411f, 1)
            };
            public References Online = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Online"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            public References Offline = new References
            {
                setIconColor = false,
                iconColor = new Color(0.887f, 0.887f, 0.887f, 1),
                message = new("Offline"),
                setMessageColor = false,
                messageColor = new Color(0.887f, 0.887f, 0.887f, 1)
            };
            public References Busy = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Busy"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            public References Away = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Away"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            public References Snooze = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Snooze"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            public References LookingToTrade = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Looking to Trade"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
            public References LookingToPlay = new References
            {
                setIconColor = false,
                iconColor = new Color(0.4117f, 0.7803f, 0.9254f, 1),
                message = new("Looking to Play"),
                setMessageColor = false,
                messageColor = new Color(0.4117f, 0.7803f, 0.9254f, 1)
            };
        }
        [Serializable]
        public class Settings
        {
            public Options configuration;
            [Header("Elements")]
            public List<Image> images = new();
            public List<TextMeshProUGUI> labels = new();
        }

        public Settings settings = new();

        private SteamUserData m_Data;

        private void Awake()
        {
            m_Data = GetComponent<SteamUserData>();
            m_Data.onChanged.AddListener(InternalPersonaStateChange);
            API.Friends.Client.OnFriendRichPresenceUpdate.AddListener(InternalRichPresenceUpdate);
        }

        private void OnDestroy()
        {
            m_Data.onChanged.RemoveListener(InternalPersonaStateChange);
            API.Friends.Client.OnFriendRichPresenceUpdate.RemoveListener(InternalRichPresenceUpdate);
        }

        private void InternalRichPresenceUpdate(FriendRichPresenceUpdate arg0)
        {
            Refresh();
        }

        private void InternalPersonaStateChange(PersonaStateChange arg0)
        {
            Refresh();
        }

        public void Refresh()
        {
            var max = math.max(settings.images.Count, settings.labels.Count);
            for (int i = 0; i < max; i++)
            {
                Image icon = settings.images.Count > i ? settings.images[i] : null;
                TextMeshProUGUI message = settings.labels.Count > i ? settings.labels[i] : null;

                if (m_Data.Data.GetGamePlayed(out var gameInfo))
                {
                    if (gameInfo.Game.IsMe)
                        settings.configuration.InThisGame.Set(icon, message, gameInfo);
                    else
                        settings.configuration.InAnotherGame.Set(icon, message, gameInfo);
                }
                else
                {
                    switch (m_Data.Data.State)
                    {
                        case EPersonaState.k_EPersonaStateAway:
                            settings.configuration.Away.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateBusy:
                            settings.configuration.Busy.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateOnline:
                            settings.configuration.Online.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateSnooze:
                            settings.configuration.Snooze.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateLookingToPlay:
                            settings.configuration.LookingToPlay.Set(icon, message, null);
                            break;
                        case EPersonaState.k_EPersonaStateLookingToTrade:
                            settings.configuration.LookingToTrade.Set(icon, message, null);
                            break;
                        default:
                            settings.configuration.Offline.Set(icon, message, null);
                            break;
                    }
                }
            }
        }
    }
}
#endif