#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Members", nameof(attributes))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyMembers : MonoBehaviour
    {
        [Serializable]
        public class Attributes
        {
            [Header("Configuration")]
            [Tooltip("If true the local user's display will be shown otherwise we skip the local user")]
            public bool showSelf = false;
            [Header("Elements")]
            [Tooltip("This game object will be instantiated for each member that joins and managed by the component")]
            public SteamLobbyMemberData template;
            [Tooltip("The container where member templates will be spawned as members join or removed from when members leave.")]
            public Transform content;
        }

        public Attributes attributes = new();

        //[Header("Configuration")]
        //[Tooltip("If true the local user's display will be shown otherwise we skip the local user")]
        //public bool showSelf = false;
        //[Header("Elements")]
        //[Tooltip("This game object will be instantiated for each member that joins and managed by the component")]
        //public SteamLobbyMemberData template;
        //[Tooltip("The container where member templates will be spawned as members join or removed from when members leave.")]
        //public Transform content;

        private SteamLobbyData m_Inspector;
        private List<SteamLobbyMemberData> m_SpawnedMembers = new();

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            m_Inspector.onChanged.AddListener(HandleLobbyChanged);
            API.Matchmaking.Client.OnLobbyChatUpdate.AddListener(GlobalChatUpdate);
        }

        private void OnDestroy()
        {
            m_Inspector?.onChanged.RemoveListener(HandleLobbyChanged);
            API.Matchmaking.Client.OnLobbyChatUpdate.RemoveListener(GlobalChatUpdate);
        }

        private void HandleLobbyChanged(LobbyData arg0)
        {
            foreach(var member in m_SpawnedMembers)
                Destroy(member.gameObject);

            m_SpawnedMembers.Clear();

            if (m_Inspector.Data.IsValid)
            {
                foreach (var member in m_Inspector.Data.Members)
                {
                    AddMember(member);
                }
            }
        }

        private void GlobalChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == m_Inspector.Data)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    AddMember(LobbyMemberData.Get(m_Inspector.Data, arg0.m_ulSteamIDUserChanged));
                else
                    RemoveMember(new UserLobbyLeaveData { user = arg0.m_ulSteamIDUserChanged, state = state });
            }
        }

        private void AddMember(LobbyMemberData data)
        {
            if (!attributes.showSelf && data.user == UserData.Me)
                return;

            if(attributes.template != null && attributes.content != null)
            {
                var go = Instantiate(attributes.template, attributes.content);
                var comp = go.GetComponent<SteamLobbyMemberData>();
                comp.Data = data;
                m_SpawnedMembers.Add(comp);
            }
        }

        private void RemoveMember(UserLobbyLeaveData data)
        {
            var target = m_SpawnedMembers.Find((p) => { return p.Data.user == data.user; });
            if(target != null)
            {
                m_SpawnedMembers.Remove(target);
                Destroy(target.gameObject);
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyMembers), true)]
    public class SteamLobbyMembersEditor : UnityEditor.Editor
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