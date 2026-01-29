#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
#if UNITY_EDITOR
using UnityEditor;
#endif
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [AddComponentMenu("Steamworks/GameObject Enable when Lobby")]
    [HelpURL("https://kb.heathen.group/steam/features/lobby")]
    public class SteamLobbyGameObjectEnabler : MonoBehaviour
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
            IsNotSession,
            IsNotFull,
            IsFull
        }

        public enum ConditionMode
        {
            OR = 1 << 0,
            AND = 1 << 1
        }

        [System.Serializable]
        public struct Condition
        {
            public string Note;

            [Tooltip("The rules that will be evaluated together.")]
            public EnableWhenRule[] rules;

            [Tooltip("How the rules are combined.")]
            public ConditionMode mode; // OR or AND
        }

        public SteamLobbyData targetLobby;
        [Tooltip("The conditions that must all be true for this GameObject to be enabled.")]
        public List<Condition> conditions = new();

        private void Awake()
        {
            if (targetLobby != null)
            {
                targetLobby.onChanged?.AddListener(HandleOnChanged);
                HandleOnChanged(targetLobby.Data);
            }
            else
                HandleOnChanged(CSteamID.Nil);
        }

        private void OnDestroy()
        {
            if (targetLobby != null)
                targetLobby.onChanged?.RemoveListener(HandleOnChanged);
        }

        private void HandleOnChanged(LobbyData arg0)
        {
            bool active = true; // top-level AND for all conditions

            foreach (var condition in conditions)
            {
                bool conditionResult = EvaluateCondition(condition);
                if (!conditionResult)
                {
                    active = false;
                    break; // early exit
                }
            }

            gameObject.SetActive(active);
        }

        private bool EvaluateCondition(Condition condition)
        {
            if (condition.rules == null || condition.rules.Length == 0)
                return true;

            bool result = condition.mode == ConditionMode.AND;

            foreach (var rule in condition.rules)
            {
                bool ruleResult = CheckRule(rule);
                if (condition.mode == ConditionMode.OR)
                    result |= ruleResult;
                else
                    result &= ruleResult;
            }

            return result;
        }

        private bool CheckRule(EnableWhenRule rule)
        {
            return rule switch
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
                EnableWhenRule.IsNotFull => targetLobby == null || !targetLobby.Data.Full,
                EnableWhenRule.IsFull => targetLobby != null && targetLobby.Data.IsValid && targetLobby.Data.Full,
                _ => false
            };
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyGameObjectEnabler), true)]
    public class SteamLobbyGameObjectEnablerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(SteamLobbyGameObjectEnabler.Condition))]
    public class SteamLobbyConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.indentLevel++;
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

            // Draw Note
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("Note"));
            rect.y += lineHeight;

            // Draw Mode
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("mode"));
            rect.y += lineHeight;

            // Draw Rules label + [+] button
            SerializedProperty rules = property.FindPropertyRelative("rules");
            Rect labelRect = new Rect(rect.x, rect.y, rect.width - 25, lineHeight);
            Rect addButtonRect = new Rect(rect.x + rect.width - 25, rect.y, 25, lineHeight);
            EditorGUI.LabelField(labelRect, "Rules");
            if (GUI.Button(addButtonRect, EditorGUIUtility.IconContent("Toolbar Plus")))
            {
                rules.arraySize++;
            }
            rect.y += lineHeight;

            // Draw each rule with [-] button
            for (int i = 0; i < rules.arraySize; i++)
            {
                SerializedProperty element = rules.GetArrayElementAtIndex(i);
                Rect fieldRect = new Rect(rect.x, rect.y, rect.width - 25, lineHeight);
                Rect removeRect = new Rect(rect.x + rect.width - 25, rect.y, 25, lineHeight);

                EditorGUI.PropertyField(fieldRect, element, GUIContent.none);
                if (GUI.Button(removeRect, EditorGUIUtility.IconContent("Toolbar Minus")))
                {
                    rules.DeleteArrayElementAtIndex(i);
                }

                rect.y += lineHeight;
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty rules = property.FindPropertyRelative("rules");
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            // Note + Mode + Rules label + each rule
            return (2 + 1 + rules.arraySize) * lineHeight;
        }
    }
#endif
}
#endif