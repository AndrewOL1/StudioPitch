#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SteamTools
{
    public static class Interface
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            IsReady = false;
            boards = new();
            sets = new();
            actions = new();
            whenReadyCalls = new();
        }

        public static bool IsInitialized => App.Initialized;
        public static bool IsDebugging
        {
            get => App.isDebugging;
            set => App.isDebugging = true;
        }
        public static bool IsReady { get; private set; }

        public static event Action OnReady;
        public static event Action<string> OnInitializationError;

        private static Dictionary<string, LeaderboardData> boards = new();
        private static Dictionary<string, InputActionSetData> sets = new();
        private static Dictionary<string, InputActionData> actions = new();
        private static List<Action> whenReadyCalls = new();

        public static void Initialize()
        {
            App.onSteamInitializationError.AddListener(HandleInitializedError);
            try
            {
                // Try to reflect Game.App.AppId
                Type gameType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    gameType = asm.GetType("SteamTools.Game");
                    if (gameType != null)
                    {
                        Debug.Log($"Found SteamTools.Game in assembly: {asm.GetName().Name}");
                        break;
                    }
                }

                if (gameType != null)
                {

                    var initMethod = gameType.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    initMethod?.Invoke(null, null);
                }
                else
                {
                    Debug.LogError("Unable to locate SteamTools.Game class make sure your generate wrapper before attempting to initialize.");
                    OnInitializationError?.Invoke("Unable to locate SteamTools.Game class");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reflecting Game.AppId: {e.Message}");
                OnInitializationError?.Invoke($"Error reflecting Game.AppId: {e.Message}");
                return;
            }
        }

        public static void WhenReady(Action callback)
        {
            if (callback == null)
                return;

            if (IsReady)
                callback.Invoke();
            else
                whenReadyCalls.Add(callback);
        }

        private static void HandleInitializedError(string arg0)
        {
            OnInitializationError?.Invoke(arg0);
        }

        public static void RaiseOnReady(Dictionary<string, LeaderboardData> boardMap,
                                        Dictionary<string, InputActionSetData> setMap,
                                        Dictionary<string, InputActionData> actionMap)
        {
            boards = boardMap;
            sets = setMap;
            actions = actionMap;
            IsReady = true;
            OnReady?.Invoke();
            foreach (var call in whenReadyCalls)
            {
                try
                {
                    call?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            whenReadyCalls.Clear();
        }

        public static void AddBoard(LeaderboardData board)
        {
            boards.TryAdd(board.apiName, board);
        }
        public static LeaderboardData GetBoard(string name)
        {
            if (boards.TryGetValue(name, out var board))
                return board;
            else
                return default;
        }
        public static LeaderboardData[] GetBoards() => boards.Values.ToArray();
        public static InputActionSetData GetSet(string name)
        {
            if (sets.TryGetValue(name, out var set))
                return set;
            else
                return default;
        }
        public static InputActionData GetAction(string name)
        {
            if (actions.TryGetValue(name, out var action))
                return action;
            else
                return default;
        }
    }

    public static class Colors
    {
        public static Color SteamBlue = new(0.2f, 0.60f, 0.93f, 1f);
        public static Color SteamGreen = new(0.2f, 0.42f, 0.2f, 1f);
        public static Color BrightGreen = new(0.4f, 0.84f, 0.4f, 1f);
        public static Color HalfAlpha = new(1f, 1f, 1f, 0.5f);
        public static Color ErrorRed = new(1, 0.5f, 0.5f, 1);
    }

    public static class Authenticate
    {
        public static void BeginSession(UserData user, byte[] ticket, BeginSessionResult callback)
        {
            var result = Authentication.BeginAuthSession(ticket, user, session =>
            {
                callback?.Invoke(EBeginAuthSessionResult.k_EBeginAuthSessionResultOK, session);
            });

            if (result != Steamworks.EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                callback?.Invoke(result, null);
        }

        public static void EndSession(UserData user) => Authentication.EndAuthSession(user);

        public static void EndAllSessions() => Authentication.EndAllSessions();

        public static void SendToRpcWhenReady(ulong serverId, SendGameServerAuthentication serverRpcCallback, Action<EResult> errorCallback)
        {
            Authentication.GetAuthSessionTicket(new CSteamID(serverId), (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                    errorCallback?.Invoke(ticket.Result);
                else
                    serverRpcCallback?.Invoke(UserData.Me, ticket.Data);
            });
        }

        public static void SendToLobbyOwnerWhenReady(LobbyData lobby, Action<EResult> onResult)
        {
            Authentication.GetAuthSessionTicket(lobby.Owner.user, (ticket, ioError) =>
            {
                if (ioError || ticket.Result != EResult.k_EResultOK)
                    onResult?.Invoke(ticket.Result);
                else
                {
                    lobby.Authenticate(new LobbyMessagePayload()
                    {
                        id = UserData.Me,
                        data = ticket.Data,
                        inventory = null
                    });

                    onResult?.Invoke(ticket.Result);
                }
            });
        }

        public static void DiscordConnectProvisional()
        {
#if DISCORD //|| true
            Heathen.DiscordSocialIntegration.API.DiscordSocialApp.GetSteamProvisionalToken(
                (result, token, refreshToken, tokenType, expiresIn, scope) =>
                {
                    if (result.Successful())
                        Heathen.DiscordSocialIntegration.API.DiscordSocialApp.Connect(token, DateTime.UtcNow.AddSeconds(expiresIn), refreshToken);
                });
#else
            Debug.LogError($"[{nameof(DiscordConnectProvisional)}]: Heathen.DiscordSocialIntegration not found.");
#endif
        }

    }

    public delegate void SendGameServerAuthentication(ulong userId, byte[] ticket);
    public delegate void BeginSessionResult(EBeginAuthSessionResult requestResult, AuthenticationSession session);
}
#endif