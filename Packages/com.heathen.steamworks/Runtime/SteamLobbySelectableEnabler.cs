#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    public class SteamLobbySelectableEnabler : MonoBehaviour
    {
        public enum EnableWhenRule
        {
            IsSet,
            IsNotSet,
            AmITheOwner,
            AmINotTheOwner,
            AmIMember,
            AmINotMember,
            IsParty,
            IsSession,
            IsNotParty,
            IsNotSession
        }

        [System.Flags]
        public enum ConditionMode
        {
            None = 0,
            OR = 1 << 0,
            AND = 1 << 1
        }

        public SteamLobbyData targetLobby;
        [Tooltip("The conditions that will enable this GameObject.")]
        public EnableWhenRule[] conditions;
        [Tooltip("Should conditions be combined with AND or OR logic?")]
        public ConditionMode mode = ConditionMode.OR;

        private Selectable[] selectable;

        private void Awake()
        {
            if (targetLobby != null)
            {
                targetLobby.onChanged?.AddListener(HandleOnChanged);
                HandleOnChanged(targetLobby.Data);
            }
            else
                HandleOnChanged(CSteamID.Nil);

            selectable = GetComponents<Selectable>();
        }

        private void OnDestroy()
        {
            if (targetLobby != null)
            {
                targetLobby.onChanged?.RemoveListener(HandleOnChanged);
            }
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            bool result = mode == ConditionMode.AND ? true : false;

            foreach (var condition in conditions)
            {
                bool conditionMet = CheckCondition(condition);

                if (mode == ConditionMode.OR)
                    result |= conditionMet; // OR logic
                else if (mode == ConditionMode.AND)
                    result &= conditionMet; // AND logic
            }

            foreach(var sel in selectable)
                sel.interactable = result;
        }

        private bool CheckCondition(EnableWhenRule condition)
        {
            return condition switch
            {
                EnableWhenRule.IsSet => targetLobby != null && targetLobby.Data.IsValid,
                EnableWhenRule.IsNotSet => targetLobby == null || !targetLobby.Data.IsValid,
                EnableWhenRule.AmITheOwner => targetLobby != null && targetLobby.Data.IsOwner,
                EnableWhenRule.AmINotTheOwner => targetLobby == null || !targetLobby.Data.IsOwner,
                EnableWhenRule.AmIMember => targetLobby != null && targetLobby.Data.IsAMember(UserData.Me),
                EnableWhenRule.AmINotMember => targetLobby == null || !targetLobby.Data.IsAMember(UserData.Me),
                EnableWhenRule.IsParty => targetLobby != null && targetLobby.Data.IsParty,
                EnableWhenRule.IsSession => targetLobby != null && targetLobby.Data.IsSession,
                EnableWhenRule.IsNotParty => targetLobby == null || !targetLobby.Data.IsParty,
                EnableWhenRule.IsNotSession => targetLobby == null || !targetLobby.Data.IsSession,
                _ => false,
            };
        }
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbySelectableEnabler), true)]
    public class SteamLobbySelectableEnablerEditor : UnityEditor.Editor
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