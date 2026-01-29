#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A simple implementation of the <see cref="ILeaderboardEntryDisplay"/> interface.
    /// </summary>
    [RequireComponent(typeof(SteamUserData))]
    [HelpURL("")]
    public class SteamLeaderboardEntryUI : MonoBehaviour, ILeaderboardEntryDisplay
    {
        /// <summary>
        /// Read or write the <see cref="LeaderboardEntry"/> for this object
        /// </summary>
        public LeaderboardEntry Entry
        {
            get => m_Entry;
            set => SetEntry(value);
        }

        [SerializeField]
        private TMPro.TextMeshProUGUI score;
        [SerializeField]
        private TMPro.TextMeshProUGUI rank;

        private SteamUserData m_UserData;
        private LeaderboardEntry m_Entry;

        private void Awake()
        {
            m_UserData = GetComponent<SteamUserData>();
        }

        private void SetEntry(LeaderboardEntry entry)
        {
            m_UserData.Data = entry.User;

            if (score != null)
                score.text = entry.Score.ToString();
            if (rank != null)
                rank.text = entry.Rank.ToString();

            m_Entry = entry;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLeaderboardEntryUI), true)]
    public class SteamLeaderboardEntryUIEditor : UnityEditor.Editor
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