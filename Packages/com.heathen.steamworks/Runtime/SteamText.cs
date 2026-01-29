#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A localized FText like feature inspired by Unreal Engine's localization model
    /// </summary>
    [Serializable]
    public struct SteamText
    {
#if LOCALIZED
        public UnityEngine.Localization.LocalizedString Localized;
#endif
        public string Default;

        public SteamText(string value)
        {
#if LOCALIZED
            Localized = new();
#endif
            Default = value;
        }

        public readonly string Get()
        {
#if LOCALIZED
            if (Localized != null &&
                Localized.TableReference.ReferenceType != UnityEngine.Localization.Tables.TableReference.Type.Empty &&
                Localized.TableEntryReference.ReferenceType != UnityEngine.Localization.Tables.TableEntryReference.Type.Empty)
            {
                string value = Localized.GetLocalizedString();
                return string.IsNullOrEmpty(value) ? Default : value;
            }
#endif
            return Default;
        }

        public override string ToString() => Get();

        public static implicit operator string(SteamText l) => l.Get();
    }
}
#endif