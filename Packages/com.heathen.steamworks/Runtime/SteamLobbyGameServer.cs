#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [ModularComponent(typeof(SteamLobbyData), "Game Server", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLobbyData))]
    public class SteamLobbyGameServer : MonoBehaviour
    {
        private SteamLobbyData m_Inspector;

        private void Awake()
        {
            m_Inspector = GetComponent<SteamLobbyData>();
            API.Matchmaking.Client.OnLobbyGameCreated.AddListener(GlobalGameCreated);
        }

        private void OnDestroy()
        {
            API.Matchmaking.Client.OnLobbyGameCreated.RemoveListener(GlobalGameCreated);
        }

        private bool EnsureOwner(out LobbyData data)
        {
            data = m_Inspector.Data;
            if (!data.IsValid)
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby to set");
                return false;
            }
            if (!data.IsOwner)
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] Only the owner can set data");
                return false;
            }
            return true;
        }

        private void GlobalGameCreated(LobbyGameCreated_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == m_Inspector.Data)
            {
                var gameServer = new LobbyGameServer
                {
                    id = new CSteamID(arg0.m_ulSteamIDGameServer),
                    ipAddress = arg0.m_unIP,
                    port = arg0.m_usPort,
                };
            }
        }

        [ContextMenu("Set as Listen Server")]
        public void SetListenServer()
        {
            if (EnsureOwner(out var data))
                m_Inspector.Data.SetGameServer();
        }
        public void SetDedicatedSteamGameServer(CSteamID serverId)
        {
            if (EnsureOwner(out var data))
                m_Inspector.Data.SetGameServer(serverId);
        }
        public void SetDedicatedGenericServer(string ip, ushort port)
        {
            if (EnsureOwner(out var data))
                m_Inspector.Data.SetGameServer(ip, port);
        }
        public void SetGameServer(CSteamID id, string ip, ushort port)
        {
            if (EnsureOwner(out var data))
                m_Inspector.Data.SetGameServer(ip, port, id);
        }

        public bool HasGameServer() => m_Inspector.Data.IsValid && m_Inspector.Data.HasServer;

        public LobbyGameServer? GetGameServer()
        {
            if (m_Inspector.Data.IsValid && m_Inspector.Data.HasServer)
                return m_Inspector.Data.GameServer;
            return null;
        }

        public string GetIdAddress()
        {
            if (m_Inspector.Data.IsValid)
            {
                if (m_Inspector.Data.HasServer)
                    return m_Inspector.Data.GameServer.id.ToString();
                else
                    return string.Empty;
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby");
                return string.Empty;
            }
        }
        public string GetIpAddress()
        {
            if (m_Inspector.Data.IsValid)
            {
                if (m_Inspector.Data.HasServer)
                    return m_Inspector.Data.GameServer.IpAddress.ToString();
                else
                    return string.Empty;
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby");
                return string.Empty;
            }
        }
        public ushort GetPort()
        {
            if (m_Inspector.Data.IsValid)
            {
                if (m_Inspector.Data.HasServer)
                    return m_Inspector.Data.GameServer.port;
                else
                    return 0;
            }
            else
            {
                Debug.LogWarning($"[{nameof(SteamLobbyGameServer)}] No lobby");
                return 0;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SteamLobbyGameServer), true)]
    public class SteamLobbyGameServerEditor : UnityEditor.Editor
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