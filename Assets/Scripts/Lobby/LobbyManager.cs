using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PurrLobby;
using PurrNet;
using UnityEngine;
using UnityEngine.Events;

namespace Lobby
{
    public class LobbyManager : MonoBehaviour, ILobbyProvider
    {
        public Task InitializeAsync()
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<FriendUser>> GetFriendsAsync(PurrLobby.LobbyManager.FriendFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public Task InviteFriendAsync(FriendUser user)
        {
            throw new System.NotImplementedException();
        }

        public Task<PurrLobby.Lobby> CreateLobbyAsync(int maxPlayers, Dictionary<string, string> lobbyProperties = null)
        {
            throw new System.NotImplementedException();
        }

        public Task LeaveLobbyAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task LeaveLobbyAsync(string lobbyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<PurrLobby.Lobby> JoinLobbyAsync(string lobbyId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<PurrLobby.Lobby>> SearchLobbiesAsync(int maxRoomsToFind = 10, Dictionary<string, string> filters = null)
        {
            throw new System.NotImplementedException();
        }

        public Task SetIsReadyAsync(string userId, bool isReady)
        {
            throw new System.NotImplementedException();
        }

        public Task SetLobbyDataAsync(string key, string value)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetLobbyDataAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<LobbyUser>> GetLobbyMembersAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetLocalUserIdAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task SetAllReadyAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task SetLobbyStartedAsync()
        {
            throw new System.NotImplementedException();
        }

        public event UnityAction<string> OnLobbyJoinFailed;
        public event UnityAction OnLobbyLeft;
        public event UnityAction<PurrLobby.Lobby> OnLobbyUpdated;
        public event UnityAction<List<LobbyUser>> OnLobbyPlayerListUpdated;
        public event UnityAction<List<FriendUser>> OnFriendListPulled;
        public event UnityAction<string> OnError;
    }
}
