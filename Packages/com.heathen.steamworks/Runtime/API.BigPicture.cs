#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    public static class BigPicture
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                m_GamepadTextInputDismissed_t = null;

                m_OnGamepadTextInputDismissed = new();
                m_OnGamepadTextInputShown = new();
            }

            /// <summary>
            /// Invoked when Show Text Input is successfully called.
            /// </summary>
            public static UnityEvent OnGamepadTextInputShown
            {
                get
                {
                    if (m_OnGamepadTextInputShown == null)
                        m_OnGamepadTextInputShown = new();

                    return m_OnGamepadTextInputShown;
                }
            }

            /// <summary>
            /// Invoked when the gamepad text input is dismissed and returns the resulting input string.
            /// </summary>
            public static UnityEvent<string> OnGamepadTextInputDismissed
            {
                get
                {
                    if (m_OnGamepadTextInputDismissed == null)
                        m_OnGamepadTextInputDismissed = new();

                    if (m_GamepadTextInputDismissed_t == null)
                        m_GamepadTextInputDismissed_t = Callback<GamepadTextInputDismissed_t>.Create(HandleGameTextInputDismissed);

                    return m_OnGamepadTextInputDismissed;
                }
            }

            private static void HandleGameTextInputDismissed(GamepadTextInputDismissed_t result)
            {
                if (result.m_bSubmitted)
                {
                    if (SteamUtils.GetEnteredGamepadTextInput(out string textValue, result.m_unSubmittedText))
                    {
                        m_OnGamepadTextInputDismissed.Invoke(textValue);
                    }
                }
            }

            private static UnityEvent<string> m_OnGamepadTextInputDismissed = new();
            private static UnityEvent m_OnGamepadTextInputShown = new();

#pragma warning disable IDE0052 // Remove unread private members
            private static Callback<GamepadTextInputDismissed_t> m_GamepadTextInputDismissed_t;
#pragma warning restore IDE0052 // Remove unread private members

            public static bool IsInBigPicture => SteamUtils.IsSteamInBigPictureMode();
            public static bool IsRunningOnDeck => SteamUtils.IsSteamRunningOnSteamDeck();

            /// <summary>
            /// Activates the Big Picture text input dialog which only supports gamepad input.
            /// </summary>
            /// <param name="inputMode">Selects the input mode to use, either Normal or Password (hidden text)</param>
            /// <param name="lineMode">Controls whether to use single or multi line input.</param>
            /// <param name="description">Sets the description that should inform the user what the input dialog is for.</param>
            /// <param name="maxLength">The maximum number of characters that the user can input.</param>
            /// <param name="currentText">Sets the preexisting text which the user can edit.</param>
            /// <returns>True if the big picture overlay is running; otherwise, false.</returns>
            public static bool ShowTextInput(EGamepadTextInputMode inputMode, EGamepadTextInputLineMode lineMode, string description, uint maxLength, string currentText)
            {
                if (SteamUtils.ShowGamepadTextInput(inputMode, lineMode, description, maxLength, currentText))
                {
                    m_OnGamepadTextInputShown.Invoke();
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Activates the Big Picture text input dialog which only supports gamepad input.
            /// </summary>
            /// <param name="inputMode">Selects the input mode to use, either Normal or Password (hidden text)</param>
            /// <param name="lineMode">Controls whether to use single or multi line input.</param>
            /// <param name="description">Sets the description that should inform the user what the input dialog is for.</param>
            /// <param name="maxLength">The maximum number of characters that the user can input.</param>
            /// <param name="currentText">Sets the preexisting text which the user can edit.</param>
            /// <returns>True if the big picture overlay is running; otherwise, false.</returns>
            public static bool ShowTextInput(EGamepadTextInputMode inputMode, EGamepadTextInputLineMode lineMode, string description, int maxLength, string currentText)
            {
                if(SteamUtils.ShowGamepadTextInput(inputMode, lineMode, description, System.Convert.ToUInt32(maxLength), currentText))
                {
                    m_OnGamepadTextInputShown.Invoke();
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
#endif