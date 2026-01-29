#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class SteamToolsSettings : ScriptableObject
    {
        [Serializable]
        public struct NameAndID : IEquatable<NameAndID>, IComparable<NameAndID>
        {
            public string name;
            public int id;

            public int CompareTo(NameAndID other)
            {
                return name.CompareTo(other.name);
            }

            public bool Equals(NameAndID other)
            {
                // HashSet uniqueness is based on 'name' only
                return string.Equals(name, other.name);
            }

            public override bool Equals(object obj)
            {
                return obj is NameAndID other && Equals(other);
            }

            public override int GetHashCode()
            {
                // Only use 'name' for hash code so HashSet considers it unique per name
                return name != null ? name.GetHashCode() : 0;
            }
        }

        [Serializable]
        public class AppSettings
        {
            public string editorName;
            public uint applicationId;
            public List<LeaderboardData.GetAllRequest> leaderboards = new();
            public List<StatData> stats = new();
            public List<AchievementData> achievements = new();
            public List<string> actionSets = new();
            public List<string> actionSetLayers = new();
            public List<InputActionData> actions = new();

            public static AppSettings CreateDefault()
            {
                var app = new AppSettings();
                app.editorName = "Main";
                app.applicationId = 480;
                app.leaderboards.Add(new LeaderboardData.GetAllRequest { create = false, name = "Feet Traveled", sort = Steamworks.ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, type = Steamworks.ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric });
                app.stats.Add("AverageSpeed");
                app.stats.Add("FeetTraveled");
                app.stats.Add("MaxFeetTraveled");
                app.stats.Add("NumGames");
                app.stats.Add("NumLosses");
                app.stats.Add("NumWins");
                app.stats.Add("Unused2");
                app.achievements.Add("ACH_TRAVEL_FAR_ACCUM");
                app.achievements.Add("ACH_TRAVEL_FAR_SINGLE");
                app.achievements.Add("ACH_WIN_100_GAMES");
                app.achievements.Add("ACH_WIN_ONE_GAME");
                app.achievements.Add("NEW_ACHIEVEMENT_0_4");
                app.actionSets.Add("menu_controls");
                app.actionSets.Add("ship_controls");
                app.actionSetLayers.Add("thrust_action_layer");
                app.actions.Add(new("analog_controls", InputActionType.Analog));
                app.actions.Add(new("backward_thrust", InputActionType.Digital));
                app.actions.Add(new("fire_lasers", InputActionType.Digital));
                app.actions.Add(new("forward_thrust", InputActionType.Digital));
                app.actions.Add(new("menu_cancel", InputActionType.Digital));
                app.actions.Add(new("menu_down", InputActionType.Digital));
                app.actions.Add(new("menu_left", InputActionType.Digital));
                app.actions.Add(new("menu_right", InputActionType.Digital));
                app.actions.Add(new("menu_select", InputActionType.Digital));
                app.actions.Add(new("menu_up", InputActionType.Digital));
                app.actions.Add(new("pause_menu", InputActionType.Digital));
                app.actions.Add(new("turn_left", InputActionType.Digital));
                app.actions.Add(new("turn_right", InputActionType.Digital));

                return app;
            }
            public static AppSettings CreateEmpty()
            {
                var app = new AppSettings();
                app.editorName = "Empty";

                return app;
            }

            public void Clear()
            {
                leaderboards.Clear();
                stats.Clear();
                achievements.Clear();
                actionSets.Clear();
                actions.Clear();
                actionSetLayers.Clear();
            }

            public void SetDefault()
            {
                leaderboards.Clear();
                stats.Clear();
                achievements.Clear();
                actionSets.Clear();
                actions.Clear();
                actionSetLayers.Clear();

                applicationId = 480;
                leaderboards.Add(new LeaderboardData.GetAllRequest { create = false, name = "Feet Traveled", sort = Steamworks.ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending, type = Steamworks.ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric });
                stats.Add("AverageSpeed");
                stats.Add("FeetTraveled");
                stats.Add("MaxFeetTraveled");
                stats.Add("NumGames");
                stats.Add("NumLosses");
                stats.Add("NumWins");
                stats.Add("Unused2");
                achievements.Add("ACH_TRAVEL_FAR_ACCUM");
                achievements.Add("ACH_TRAVEL_FAR_SINGLE");
                achievements.Add("ACH_WIN_100_GAMES");
                achievements.Add("ACH_WIN_ONE_GAME");
                achievements.Add("NEW_ACHIEVEMENT_0_4");
                actionSets.Add("menu_controls");
                actionSets.Add("ship_controls");
                actionSetLayers.Add("thrust_action_layer");
                actions.Add(new("analog_controls", InputActionType.Analog));
                actions.Add(new("backward_thrust", InputActionType.Digital));
                actions.Add(new("fire_lasers", InputActionType.Digital));
                actions.Add(new("forward_thrust", InputActionType.Digital));
                actions.Add(new("menu_cancel", InputActionType.Digital));
                actions.Add(new("menu_down", InputActionType.Digital));
                actions.Add(new("menu_left", InputActionType.Digital));
                actions.Add(new("menu_right", InputActionType.Digital));
                actions.Add(new("menu_select", InputActionType.Digital));
                actions.Add(new("menu_up", InputActionType.Digital));
                actions.Add(new("pause_menu", InputActionType.Digital));
                actions.Add(new("turn_left", InputActionType.Digital));
                actions.Add(new("turn_right", InputActionType.Digital));
            }
        }

        public uint? ActiveApp
        {
            get
            {
                if (activeAppIndex == -1) return mainAppSettings?.applicationId;
                if (activeAppIndex == -2) return demoAppSettings?.applicationId;
                if (activeAppIndex >= 0 && activeAppIndex < playtestSettings.Count)
                    return playtestSettings[activeAppIndex]?.applicationId;
                return null;
            }
        }

        [HideInInspector]
        public DateTime lastGenerated;
        [HideInInspector]
        public int activeAppIndex = -1;
        [HideInInspector]
        public AppSettings mainAppSettings = AppSettings.CreateDefault();
        [HideInInspector]
        public AppSettings demoAppSettings = null;
        [HideInInspector]
        public List<string> dlcNames = new();
        [HideInInspector]
        public List<uint> dlc = new();
        [HideInInspector]
        public SteamGameServerConfiguration defaultServerSettings;
        [HideInInspector]
        public InventorySettings inventorySettings = new();
        [HideInInspector]
        public List<AppSettings> playtestSettings = new();
        [HideInInspector]
        public List<uint> appIds = new();
        [HideInInspector]
        public List<string> leaderboards = new();
        [HideInInspector]
        public List<string> stats = new();
        [HideInInspector]
        public List<string> achievements = new();
        [HideInInspector]
        public List<string> inputSets = new();
        [HideInInspector]
        public List<string> inputLayers = new();
        [HideInInspector]
        public List<string> inputActions = new();
        [HideInInspector]
        public List<NameAndID> items = new();
        [HideInInspector]
        public bool isDirty = true;

#if UNITY_EDITOR
        //private AppSettings current = null;

        public AppSettings Get(uint appId)
        {
            if (mainAppSettings.applicationId == appId)
                return mainAppSettings;
            else if (demoAppSettings?.applicationId == appId)
                return demoAppSettings;
            else
            {
                foreach (var playtest in playtestSettings)
                {
                    if (playtest?.applicationId == appId)
                        return playtest;
                }
            }

            return null;
        }

        public static SteamToolsSettings GetOrCreate()
        {
            // Search for any existing SteamToolsSettings asset in the project
            var guids = UnityEditor.AssetDatabase.FindAssets("t:SteamToolsSettings");
            SteamToolsSettings asset = null;

            if (guids != null && guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamToolsSettings>(path);
            }

            // If none found, create a new one at a sensible default location
            if (asset == null)
            {
                const string defaultPath = "Assets/Settings/SteamToolsSettings.asset";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(defaultPath));

                asset = ScriptableObject.CreateInstance<SteamToolsSettings>();
                UnityEditor.AssetDatabase.CreateAsset(asset, defaultPath);
                UnityEditor.AssetDatabase.SaveAssets();

                Debug.Log("[SteamToolsSettings] Created new settings asset at " + defaultPath);
            }

            return asset;
        }

        public void CollectUniqueData()
        {
            // Clear existing lists
            appIds.Clear();
            leaderboards.Clear();
            stats.Clear();
            achievements.Clear();
            inputSets.Clear();
            inputLayers.Clear();
            inputActions.Clear();
            items.Clear();

            var uniqueAppIds = new HashSet<uint>();
            var uniqueLeaderboards = new HashSet<string>();
            var uniqueStats = new HashSet<string>();
            var uniqueAchievements = new HashSet<string>();
            var uniqueInputSets = new HashSet<string>();
            var uniqueInputLayers = new HashSet<string>();
            var uniqueInputActions = new HashSet<string>();
            var uniqueItems = new HashSet<NameAndID>();

            if (inventorySettings?.items != null)
            {
                foreach (var item in inventorySettings.items)
                    if (!string.IsNullOrEmpty(item?.item_name.GetSimpleValue()))
                        uniqueItems.Add(new() { name = item.item_name.GetSimpleValue(), id = item.id });
            }

            // Helper to walk one AppSettings
            void CollectFromApp(AppSettings app)
            {
                if (app == null) return;

                uniqueAppIds.Add(app.applicationId);

                if (app.leaderboards != null)
                {
                    foreach (var lb in app.leaderboards)
                        if (!string.IsNullOrEmpty(lb.name))
                            uniqueLeaderboards.Add(lb.name);
                }

                if (app.stats != null)
                {
                    foreach (var s in app.stats)
                        if (!string.IsNullOrEmpty(s))
                            uniqueStats.Add(s);
                }

                if (app.achievements != null)
                {
                    foreach (var a in app.achievements)
                        if (!string.IsNullOrEmpty(a))
                            uniqueAchievements.Add(a);
                }

                if (app.actionSets != null)
                {
                    foreach (var set in app.actionSets)
                        if (!string.IsNullOrEmpty(set))
                            uniqueInputSets.Add(set);
                }

                if (app.actionSetLayers != null)
                {
                    foreach (var layer in app.actionSetLayers)
                        if (!string.IsNullOrEmpty(layer))
                            uniqueInputLayers.Add(layer);
                }

                if (app.actions != null)
                {
                    foreach (var act in app.actions)
                        if (!string.IsNullOrEmpty(act.Name))
                            uniqueInputActions.Add(act.Name);
                }
            }

            // Collect from all apps
            CollectFromApp(mainAppSettings);
            CollectFromApp(demoAppSettings);
            foreach (var app in playtestSettings)
                CollectFromApp(app);

            // Assign to public lists
            appIds.AddRange(uniqueAppIds);
            leaderboards.AddRange(uniqueLeaderboards);
            stats.AddRange(uniqueStats);
            achievements.AddRange(uniqueAchievements);
            inputSets.AddRange(uniqueInputSets);
            inputLayers.AddRange(uniqueInputLayers);
            inputActions.AddRange(uniqueInputActions);
            items.AddRange(uniqueItems);

            // Sort all lists
            leaderboards.Sort();
            stats.Sort();
            achievements.Sort();
            inputSets.Sort();
            inputLayers.Sort();
            inputActions.Sort();
            items.Sort();
            dlcNames.Sort();

            isDirty = true; // mark dirty since lists changed
        }

        public void CreateOrUpdateWrapper()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("SteamTools.Game t:Script");
            string path = null;
            foreach (var guid in guids)
            {
                var p = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var text = System.IO.File.ReadAllText(p);
                if (text.Contains("namespace Heathen.SteamTools"))
                {
                    path = p;
                    break;
                }
            }

            if (string.IsNullOrEmpty(path))
                path = "Assets/Scripts/Generated/SteamTools.Game.cs";

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            CollectUniqueData();
            var code = GenerateWrapperCode();
            System.IO.File.WriteAllText(path, code);
            UnityEditor.AssetDatabase.Refresh();
            lastGenerated = DateTime.Now;
        }

        string MakeValidIdentifier(string name)
        {
            // Replace any non-letter/digit/_ with _
            string identifier = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");

            // If it starts with a digit, prepend _
            if (char.IsDigit(identifier[0]))
                identifier = "_" + identifier;

            return identifier;
        }

        private string GenerateWrapperCode()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("using Heathen.SteamworksIntegration;");
            sb.AppendLine("using Heathen.SteamworksIntegration.API;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Steamworks;");
            sb.AppendLine();
            sb.AppendLine("namespace SteamTools");
            sb.AppendLine("{");
            sb.AppendLine("    public static class Game");
            sb.AppendLine("    {");
            sb.AppendLine("        // Generated by SteamToolsSettings");
            sb.AppendLine();

            //-------------------------------------------------
            // AppId section
            //-------------------------------------------------
            for (int i = 0; i < appIds.Count; i++)
            {
                var id = appIds[i];
                sb.AppendLine(i == 0 ? $"#if APP{id}" : $"#elif APP{id}");
                sb.AppendLine($"        public const uint AppId = {id};");
            }
            sb.AppendLine("#else");
            sb.AppendLine("        public const uint AppId = 0;");
            sb.AppendLine("#endif");

            //-------------------------------------------------
            // DLC section
            //-------------------------------------------------
            if (dlcNames.Count > 0)
            {
                bool wroteDlc = false;

                if (!wroteDlc)
                {
                    sb.AppendLine("        public static class DLC");
                    sb.AppendLine("        {");
                    wroteDlc = true;
                }

                for (int ii = 0; ii < dlcNames.Count; ii++)
                {
                    var foundItem = dlc[ii];
                    string safeName = MakeValidIdentifier(dlcNames[ii]);
                    sb.AppendLine($"            public static DlcData {safeName} = {foundItem};");
                }

                if (wroteDlc)
                {
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
            }

            //-------------------------------------------------
            // Server Config section
            //-------------------------------------------------
            sb.AppendLine("        // Default server configuration");
            sb.AppendLine("        public static SteamGameServerConfiguration ServerConfiguration = new()");
            sb.AppendLine("        {");
            sb.AppendLine($"            autoInitialize = {defaultServerSettings.autoInitialize.ToString().ToLower()},");
            sb.AppendLine($"            autoLogon = {defaultServerSettings.autoLogon.ToString().ToLower()},");
            sb.AppendLine($"            ip = {defaultServerSettings.ip},");
            sb.AppendLine($"            gamePort = {defaultServerSettings.gamePort},");
            sb.AppendLine($"            queryPort = {defaultServerSettings.queryPort},");
            sb.AppendLine($"            spectatorPort = {defaultServerSettings.spectatorPort},");
            sb.AppendLine($"            serverVersion = \"{defaultServerSettings.serverVersion}\",");
            sb.AppendLine($"            usingGameServerAuthApi = {defaultServerSettings.usingGameServerAuthApi.ToString().ToLower()},");
            sb.AppendLine($"            enableHeartbeats = {defaultServerSettings.enableHeartbeats.ToString().ToLower()},");
            sb.AppendLine($"            supportSpectators = {defaultServerSettings.supportSpectators.ToString().ToLower()},");
            sb.AppendLine($"            spectatorServerName = \"{defaultServerSettings.spectatorServerName}\",");
            sb.AppendLine($"            anonymousServerLogin = {defaultServerSettings.anonymousServerLogin.ToString().ToLower()},");
            sb.AppendLine($"            gameServerToken = \"{defaultServerSettings.gameServerToken}\",");
            sb.AppendLine($"            isPasswordProtected = {defaultServerSettings.isPasswordProtected.ToString().ToLower()},");
            sb.AppendLine($"            serverName = \"{defaultServerSettings.serverName}\",");
            sb.AppendLine($"            gameDescription = \"{defaultServerSettings.gameDescription}\",");
            sb.AppendLine($"            gameDirectory = \"{defaultServerSettings.gameDirectory}\",");
            sb.AppendLine($"            isDedicated = {defaultServerSettings.isDedicated.ToString().ToLower()},");
            sb.AppendLine($"            maxPlayerCount = {defaultServerSettings.maxPlayerCount},");
            sb.AppendLine($"            botPlayerCount = {defaultServerSettings.botPlayerCount},");
            sb.AppendLine($"            mapName = \"{defaultServerSettings.mapName}\",");
            sb.AppendLine($"            gameData = \"{defaultServerSettings.gameData}\",");
            sb.AppendLine($"            rulePairs = null");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        public static void ServerConfigFromIni(string iniData) => ServerConfiguration = SteamGameServerConfiguration.ParseIniString(iniData);");
            sb.AppendLine("        public static void ServerConfigFromJson(string jsonData) => ServerConfiguration = JsonUtility.FromJson<SteamGameServerConfiguration>(jsonData);");

            //-------------------------------------------------
            // Game.Initialize section
            //-------------------------------------------------
            sb.AppendLine();
            sb.AppendLine("        public static void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine($"              Debug.Log($\"Initializing for app {{AppId}}\");");
            sb.AppendLine($"             {typeof(Heathen.SteamworksIntegration.API.App).FullName}.{nameof(API.App.onSteamInitialized)}.{nameof(API.App.onSteamInitialized.AddListener)}(HandleInitialized);");
            sb.AppendLine("#if UNITY_SERVER");
            sb.AppendLine($"             {typeof(Heathen.SteamworksIntegration.API.App.Server).FullName.Replace('+', '.')}.{nameof(API.App.Server.Initialize)}(AppId, ServerConfiguration);");
            sb.AppendLine("#else");
            sb.AppendLine("             List<InputActionData> actions = new();");
            sb.AppendLine();
            for (int i = 0; i < appIds.Count; i++)
            {
                var appId = appIds[i];
                var appSettings = Get(appId);

                sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                foreach (var actions in appSettings.actions)
                {
                    string type = "InputActionType.Digital";
                    if (actions.Type == InputActionType.Analog)
                        type = "InputActionType.Analog";
                    sb.AppendLine($"                actions.Add(new(\"{actions.Name}\", {type}));");
                }
            }
            sb.AppendLine($"#endif");
            sb.AppendLine();
            sb.AppendLine($"             {typeof(Heathen.SteamworksIntegration.API.App.Client).FullName.Replace('+', '.')}.{nameof(API.App.Client.Initialize)}(AppId, actions.ToArray());");
            sb.AppendLine($"#endif");
            sb.AppendLine("        }");

            //-------------------------------------------------
            // Game.HandleInitialized section
            //-------------------------------------------------
            sb.AppendLine();
            sb.AppendLine("        private static void HandleInitialized()");
            sb.AppendLine("        {");
            if (inputSets.Count > 0)
                sb.AppendLine("            Inputs.Sets.Initialize();");

            if (leaderboards.Count > 0)
            {
                for (int i = 0; i < appIds.Count; i++)
                {
                    var appId = appIds[i];
                    var appSettings = Get(appId);
                    sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                    if (appSettings.leaderboards.Count > 0)
                    {
                        sb.AppendLine($"            int boardCount = {appSettings.leaderboards.Count};");
                        sb.AppendLine($"            int returnedBoards = 0;");
                        foreach (var leaderboard in appSettings.leaderboards)
                        {
                            string display = "ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric";
                            switch (leaderboard.type)
                            {
                                case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds:
                                    display = "ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds";
                                    break;
                                case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds:
                                    display = "ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds";
                                    break;
                            }

                            string sort = leaderboard.sort == ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending ? "ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending" : "ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending";

                            if (leaderboard.create)
                            {
                                sb.AppendLine($"            LeaderboardData.GetOrCreate(\"{leaderboard.name}\", {display}, {sort}, (result, ioError) =>");
                                sb.AppendLine($"            {{");
                                sb.AppendLine($"                if(!ioError)");
                                sb.AppendLine($"                    Leaderboards.{MakeValidIdentifier(leaderboard.name)} = result;");
                                sb.AppendLine();
                                sb.AppendLine($"                returnedBoards++;");
                                sb.AppendLine($"                if(returnedBoards >= boardCount)");
                                sb.AppendLine($"                {{");
                                if (leaderboards.Count > 0)
                                    sb.AppendLine($"                    Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();");
                                else
                                    sb.AppendLine($"                    Dictionary<string, LeaderboardData> boardMap = new();");

                                if (inputSets.Count > 0)
                                    sb.AppendLine($"                    Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();");
                                else
                                    sb.AppendLine($"                    Dictionary<string, InputActionSetData> setMap = new();");

                                if (inputActions.Count > 0)
                                    sb.AppendLine($"                    Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();");
                                else
                                    sb.AppendLine($"                    Dictionary<string, InputActionData> actionMap = new();");

                                sb.AppendLine($"                    Interface.RaiseOnReady(boardMap, setMap, actionMap);");
                                sb.AppendLine($"                }}");
                                sb.AppendLine($"            }});");

                            }
                            else
                            {
                                sb.AppendLine($"            LeaderboardData.Get(\"{leaderboard.name}\", (result, ioError) =>");
                                sb.AppendLine($"            {{");
                                sb.AppendLine($"                if(!ioError)");
                                sb.AppendLine($"                    Leaderboards.{MakeValidIdentifier(leaderboard.name)} = result;");
                                sb.AppendLine();
                                sb.AppendLine($"                returnedBoards++;");
                                sb.AppendLine($"                if(returnedBoards >= boardCount)");
                                sb.AppendLine($"                {{");
                                if (leaderboards.Count > 0)
                                    sb.AppendLine($"                    Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();");
                                else
                                    sb.AppendLine($"                    Dictionary<string, LeaderboardData> boardMap = new();");

                                if (inputSets.Count > 0)
                                    sb.AppendLine($"                    Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();");
                                else
                                    sb.AppendLine($"                    Dictionary<string, InputActionSetData> setMap = new();");

                                if (inputActions.Count > 0)
                                    sb.AppendLine($"                    Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();");
                                else
                                    sb.AppendLine($"                    Dictionary<string, InputActionData> actionMap = new();");

                                sb.AppendLine("                    Interface.RaiseOnReady(boardMap, setMap, actionMap);");
                                sb.AppendLine($"                }}");
                                sb.AppendLine($"            }});");
                            }
                        }
                    }
                    else
                    {
                        if (leaderboards.Count > 0)
                            sb.AppendLine($"                Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();");
                        else
                            sb.AppendLine($"                Dictionary<string, LeaderboardData> boardMap = new();");

                        if (inputSets.Count > 0)
                            sb.AppendLine($"                Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();");
                        else
                            sb.AppendLine($"                Dictionary<string, InputActionSetData> setMap = new();");

                        if (inputActions.Count > 0)
                            sb.AppendLine($"                Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();");
                        else
                            sb.AppendLine($"                Dictionary<string, InputActionData> actionMap = new();");

                        sb.AppendLine("                Interface.RaiseOnReady(boardMap, setMap, actionMap);");
                    }
                }
                sb.AppendLine($"#endif");
            }
            else
            {
                if (leaderboards.Count > 0)
                    sb.AppendLine($"                Dictionary<string, LeaderboardData> boardMap = Leaderboards.GetMap();");
                else
                    sb.AppendLine($"                Dictionary<string, LeaderboardData> boardMap = new();");

                if (inputSets.Count > 0)
                    sb.AppendLine($"                Dictionary<string, InputActionSetData> setMap = Inputs.Sets.GetMap();");
                else
                    sb.AppendLine($"                Dictionary<string, InputActionSetData> setMap = new();");

                if (inputActions.Count > 0)
                    sb.AppendLine($"                Dictionary<string, InputActionData> actionMap = Inputs.Actions.GetMap();");
                else
                    sb.AppendLine($"                Dictionary<string, InputActionData> actionMap = new();");

                sb.AppendLine("                Interface.RaiseOnReady(boardMap, setMap, actionMap);");
            }

            sb.AppendLine("        }");

            //-------------------------------------------------
            // Stats section
            //-------------------------------------------------
            if (stats.Count > 0)
            {
                bool wroteStats = false;
                for (int i = 0; i < appIds.Count; i++)
                {
                    var appId = appIds[i];
                    var appSettings = Get(appId);

                    if (!wroteStats)
                    {
                        sb.AppendLine("        public static class Stats");
                        sb.AppendLine("        {");
                        wroteStats = true;
                    }

                    sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                    foreach (var statName in stats)
                    {
                        var matchStat = appSettings.stats.Find(s => s.ApiName == statName);
                        if (string.IsNullOrEmpty(matchStat))
                            sb.AppendLine($"            public static StatData {MakeValidIdentifier(statName)};");
                        else
                            sb.AppendLine($"            public static StatData {MakeValidIdentifier(statName)} = \"{matchStat.ApiName}\";");
                    }
                }

                if (wroteStats)
                {
                    sb.AppendLine("#else");
                    foreach (var statName in stats)
                        sb.AppendLine($"            public static StatData {MakeValidIdentifier(statName)};");
                    sb.AppendLine("#endif");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
            }

            //-------------------------------------------------
            // Achievements section
            //-------------------------------------------------
            if (achievements.Count > 0)
            {
                bool wroteAchievements = false;
                for (int i = 0; i < appIds.Count; i++)
                {
                    var appId = appIds[i];
                    var appSettings = Get(appId);

                    if (!wroteAchievements)
                    {
                        sb.AppendLine("        public static class Achievements");
                        sb.AppendLine("        {");
                        wroteAchievements = true;
                    }

                    sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                    foreach (var achievementName in achievements)
                    {
                        var matchApiName = appSettings.achievements.Find(p => p.ApiName == achievementName);
                        if (string.IsNullOrEmpty(matchApiName))
                            sb.AppendLine($"            public static AchievementData {MakeValidIdentifier(achievementName)};");
                        else
                            sb.AppendLine($"            public static AchievementData {MakeValidIdentifier(achievementName)} = \"{matchApiName.ApiName}\";");
                    }
                }

                if (wroteAchievements)
                {
                    sb.AppendLine("#else");
                    foreach (var achievementName in achievements)
                        sb.AppendLine($"            public static AchievementData {MakeValidIdentifier(achievementName)};");
                    sb.AppendLine("#endif");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
            }

            //-------------------------------------------------
            // Leaderboards section
            //-------------------------------------------------
            if (leaderboards.Count > 0)
            {
                bool wroteLeaderboards = false;
                if (leaderboards.Count > 0)
                {
                    sb.AppendLine("        public static class Leaderboards");
                    sb.AppendLine("        {");
                    wroteLeaderboards = true;
                    foreach (var leaderboardName in leaderboards)
                    {
                        sb.AppendLine($"            public static LeaderboardData {MakeValidIdentifier(leaderboardName)};");
                    }
                    sb.AppendLine();
                    sb.AppendLine("            public static Dictionary<string, LeaderboardData> GetMap()");
                    sb.AppendLine("            {");
                    sb.AppendLine("                var map = new Dictionary<string, LeaderboardData>();");
                    foreach (var boardName in leaderboards)
                    {
                        sb.AppendLine($"                map.Add(\"{boardName}\", {boardName.Replace(' ', '_')});");
                    }
                    sb.AppendLine("                return map;");
                    sb.AppendLine("            }");
                    sb.AppendLine();

                    if (wroteLeaderboards)
                    {
                        sb.AppendLine("        }");
                        sb.AppendLine();
                    }
                }
            }

            //-------------------------------------------------
            // Inputs section
            //-------------------------------------------------
            if (inputSets.Count > 0)
            {
                sb.AppendLine("        public static class Inputs");
                sb.AppendLine("        {");                

                // ---- Sets ----
                if (inputSets.Count > 0)
                {
                    sb.AppendLine("            public static class Sets");
                    sb.AppendLine("            {");
                    sb.AppendLine();
                    sb.AppendLine("            public static Dictionary<string, InputActionSetData> GetMap()");
                    sb.AppendLine("            {");
                    sb.AppendLine("                var map = new Dictionary<string, InputActionSetData>();");
                    foreach (var setName in inputSets)
                    {
                        sb.AppendLine($"                map.Add(\"{setName}\", {MakeValidIdentifier(setName)});");
                    }
                    sb.AppendLine("                return map;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            public static void Initialize()");
                    sb.AppendLine("            {");
                    sb.AppendLine("#if UNITY_SERVER\r\n                    return;\r\n#endif");

                    for (int i = 0; i < appIds.Count; i++)
                    {
                        var appId = appIds[i];
                        var appSettings = Get(appId);

                        sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                        foreach (var setName in appSettings.actionSets)
                        {
                            sb.AppendLine($"                {setName.Replace(' ', '_')} = InputActionSetData.Get(\"{setName}\");");
                        }
                    }
                    sb.AppendLine("#endif");
                    sb.AppendLine("            }");
                    foreach (var setName in inputSets)
                    {
                        sb.AppendLine($"                public static InputActionSetData {setName.Replace(' ', '_')};");
                    }
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                // ---- Layers ----
                if (inputLayers.Count > 0)
                {
                    sb.AppendLine("            public static class Layers");
                    sb.AppendLine("            {");
                    for (int i = 0; i < appIds.Count; i++)
                    {
                        var appId = appIds[i];
                        var appSettings = Get(appId);
                        sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                        foreach (var layerName in inputLayers)
                        {
                            bool exists = appSettings.actionSetLayers.Contains(layerName);
                            sb.AppendLine(exists
                                ? $"                public static InputActionSetLayerData {MakeValidIdentifier(layerName)} = new(){{ layerName = \"{layerName}\" }};"
                                : $"                public static InputActionSetLayerData {MakeValidIdentifier(layerName)};");
                        }
                    }
                    sb.AppendLine("#else");
                    foreach (var layerName in inputLayers)
                        sb.AppendLine($"                public static InputActionSetLayerData {MakeValidIdentifier(layerName)};");
                    sb.AppendLine("#endif");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                // ---- Actions ----
                if (inputActions.Count > 0)
                {
                    sb.AppendLine("            public static class Actions");
                    sb.AppendLine("            {");
                    sb.AppendLine();
                    for (int i = 0; i < appIds.Count; i++)
                    {
                        var appId = appIds[i];
                        var appSettings = Get(appId);
                        sb.AppendLine(i == 0 ? $"#if APP{appId}" : $"#elif APP{appId}");
                        foreach (var actionName in inputActions)
                        {
                            var match = appSettings.actions.Find(a => a.Name == actionName);
                            sb.AppendLine(!string.IsNullOrEmpty(match.Name)
                                ? $"                public static InputActionData {actionName.Replace(' ', '_')} = new InputActionData(\"{match.Name}\", InputActionType.{match.Type});"
                                : $"                public static InputActionData {actionName.Replace(' ', '_')};");
                        }
                    }
                    sb.AppendLine("#else");
                    foreach (var actionName in inputActions)
                        sb.AppendLine($"                public static InputActionData {actionName.Replace(' ', '_')};");
                    sb.AppendLine("#endif");
                    sb.AppendLine();
                    sb.AppendLine("                public static Dictionary<string, InputActionData> GetMap()");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    var map = new Dictionary<string, InputActionData>();");
                    foreach (var actionName in inputActions)
                    {
                        sb.AppendLine($"                    map.Add(\"{actionName}\", {actionName.Replace(' ', '_')});");
                    }
                    sb.AppendLine("                return map;");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                sb.AppendLine("        }");
                sb.AppendLine();
            }

            //-------------------------------------------------
            // Inventory section
            //-------------------------------------------------
            if (items.Count > 0)
            {

                sb.AppendLine("        public static class Inventory");
                sb.AppendLine("        {");

                foreach (var itemNameAndId in items)
                {
                    //var foundItem = inventorySettings.items.Find(p => p.Name == itemName);
                    //if (foundItem != null && foundItem.id <= 0)
                    //    sb.AppendLine($"            public static ItemData {itemName.Replace(' ', '_')};");
                    //else
                        sb.AppendLine($"            public static ItemData {MakeValidIdentifier(itemNameAndId.name)} = {itemNameAndId.id};");
                }

                sb.AppendLine("        }");
                sb.AppendLine();
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
#endif
    }
}
#endif