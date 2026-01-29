#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace Heathen.SteamworksIntegration.Editors
{
    public class SteamworksSettingsProvider : SettingsProvider
    {
        private SteamToolsSettings settings;
        private bool needRefresh = false;
        private Dictionary<string, bool> toggles = new();
        private string newSettingName = string.Empty;

        private bool GetToggle(string name)
        {
            if(toggles.TryGetValue(name, out var value))
                return value;
            else
                return false;
        }

        private void SetToggle(string name, bool value)
        {
            if(!toggles.TryAdd(name, value))
                toggles[name] = value;
        }

        private bool this[string name]
        {
            get => GetToggle(name);
            set => SetToggle(name, value);
        }

        class Styles
        {
            public static GUIContent appId = new("Application ID");
            public static GUIContent callbackFrequency = new("Tick (Milliseconds)");
        }

        public SteamworksSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            settings = SteamToolsSettings.GetOrCreate();
        }

        public void UpdateAppDefine()
        {
            if (!settings.ActiveApp.HasValue)
                return;

            string activeDefine = $"APP{settings.ActiveApp.Value}";

            var targets = new NamedBuildTarget[]
            {
                NamedBuildTarget.Standalone,
                NamedBuildTarget.Server,
            };

            foreach (var buildTarget in targets)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
                var defineList = new HashSet<string>(defines.Split(';', System.StringSplitOptions.RemoveEmptyEntries));

                // Remove all APP#### defines except active
                var toRemove = new List<string>();
                bool hasActive = false;

                foreach (var d in defineList)
                {
                    if (d == activeDefine)
                    {
                        hasActive = true;
                        continue;
                    }

                    if (d.StartsWith("APP") &&
                        d.Length > 3 &&
                        uint.TryParse(d.Substring(3), out _) &&
                        d != activeDefine)
                    {
                        toRemove.Add(d);
                    }
                }

                if (hasActive && toRemove.Count == 0)
                    continue;

                foreach (var d in toRemove)
                    defineList.Remove(d);

                // Add active define
                defineList.Add(activeDefine);

                PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", defineList));
            }
        }

        public override void OnGUI(string searchContext)
        {
            if(!settings.ActiveApp.HasValue)
            {
                settings.activeAppIndex = -1;
                settings.mainAppSettings ??= SteamToolsSettings.AppSettings.CreateDefault();
            }

            var options = new List<string>();
            var indices = new List<int>();

            // Always add main
            options.Add($"Main ({settings.mainAppSettings.applicationId})");
            indices.Add(-1);

            // Optional demo
            if (settings.demoAppSettings != null)
            {
                options.Add($"Demo ({settings.demoAppSettings.applicationId})");
                indices.Add(-2);
            }

            // Optional playtests
            if (settings.playtestSettings != null)
            {
                for (int i = 0; i < settings.playtestSettings.Count; i++)
                {
                    var app = settings.playtestSettings[i];
                    options.Add($"{app.editorName} ({app.applicationId})");
                    indices.Add(i);
                }
            }

            // Find current dropdown index
            int currentIndex = indices.IndexOf(settings.activeAppIndex);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.indentLevel++;
            // Show dropdown
            int newIndex = EditorGUILayout.Popup("Active Application", currentIndex, options.ToArray());
            EditorGUI.indentLevel--;

            // Update activeAppIndex
            int nIndex = indices[newIndex];
            bool needRestart = false;
            if (nIndex != settings.activeAppIndex)
            {
                EditorUtility.SetDirty(this.settings);
                settings.activeAppIndex = nIndex;
                needRestart = true;
            }
            
            UpdateAppDefine();

            if (needRestart)
            {
                bool restartNow = EditorUtility.DisplayDialog(
                    "Restart Required",
                    "Changing the active App ID requires restarting Unity. Do you want to restart now?",
                    "Restart Now",
                    "Restart Later"
                );

                if (restartNow)
                {
                    EditorApplication.OpenProject(Environment.CurrentDirectory);
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIStyle nStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 18
            };
            EditorGUILayout.LabelField(" Global", nStyle);
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Knowledge Base"))
            {
                Application.OpenURL("https://kb.heathen.group/steamworks");
            }
            if (EditorGUILayout.LinkButton("Support"))
            {
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            }
            if (EditorGUILayout.LinkButton("Leave a Review"))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/5836");
                Application.OpenURL("https://ie.trustpilot.com/review/heathen.group");
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Generate Wrapper", GUILayout.Height(24)))
            {
                settings.CreateOrUpdateWrapper();
            }
            EditorGUILayout.Space(6);
            EditorGUI.indentLevel++;
            DrawDLCList();
            DrawInventoryArea();
            DrawServerSettings();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(" Main", nStyle);
            if (EditorGUILayout.LinkButton("Steamworks Portal"))
            {
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.mainAppSettings.applicationId.ToString());
            }
                        
            DrawCommonSettings(settings.mainAppSettings);
            DemoArea();
            PlaytestArea();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateSteamworksSettingsProvider()
        {
            var provider = new SteamworksSettingsProvider("Project/Player/Steamworks", SettingsScope.Project)
            {
                // Automatically extract all keywords from the Styles.
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }

        private void DemoArea()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIStyle nStyle = new GUIStyle(EditorStyles.boldLabel);
            nStyle.fontSize = 18;
            EditorGUILayout.LabelField(" Demo", nStyle);
            EditorGUILayout.Space();
            if (settings.demoAppSettings != null)
            {
                if (EditorGUILayout.LinkButton("Steamworks Portal"))
                {
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.demoAppSettings.applicationId.ToString());
                }
                
                DrawCommonSettings(settings.demoAppSettings);
            }
            else
            {
                if (GUILayout.Button("Create Demo Settings"))
                {
                    GUI.FocusControl(null);

                    settings.demoAppSettings = new() { editorName = "Demo" };
                }
            }
        }

        private void PlaytestArea()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUIStyle nStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 18
            };
            EditorGUILayout.LabelField(" Playtests", nStyle);

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            newSettingName = EditorGUILayout.TextField("Playtest Name", newSettingName);
            if (GUILayout.Button("Create Playtest Settings") && !string.IsNullOrEmpty(newSettingName))
            {
                GUI.FocusControl(null);

                settings.playtestSettings.Add(new()
                {
                    editorName = newSettingName,
                    applicationId = 0
                });

                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);

                newSettingName = string.Empty;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            SteamToolsSettings.AppSettings settingsToRemove = null;
            foreach (var playtest in settings.playtestSettings)
            {
                if (!PlaytestArea(playtest))
                {
                    settingsToRemove = playtest;
                    break;
                }
            }

            if(settingsToRemove != null)
                settings.playtestSettings.Remove(settingsToRemove);
        }

        private bool PlaytestArea(SteamToolsSettings.AppSettings settings)
        {
            EditorGUILayout.Space();
            GUIStyle nStyle = new GUIStyle(EditorStyles.boldLabel);
            nStyle.fontSize = 16;
            EditorGUILayout.LabelField(" " + settings.editorName, nStyle);
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Steamworks Portal"))
            {
                Application.OpenURL("https://partner.steamgames.com/apps/landing/" + settings.applicationId.ToString());
            }
            if (EditorGUILayout.LinkButton("Remove"))
            {
                return false;
            }
            EditorGUILayout.EndHorizontal();

            DrawCommonSettings(settings);
            return true;
        }

        private void DrawCommonSettings(SteamToolsSettings.AppSettings settings)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Debug Window"))
            {
                GUI.FocusControl(null);

                SteamInspector_Code.ShowExample();
            }

            if (GUILayout.Button("Clear All"))
            {
                GUI.FocusControl(null);

                settings.Clear();
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
            }

            if (GUILayout.Button("Set Test Values"))
            {
                GUI.FocusControl(null);
                needRefresh = true;
                settings.SetDefault();
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            var id = EditorGUILayout.TextField("Application Id", settings.applicationId.ToString());
            uint buffer = 0;
            if (uint.TryParse(id, out buffer))
            {
                if (buffer != settings.applicationId)
                {
                    Undo.RecordObject(this.settings, "editor");
                    settings.applicationId = buffer;
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
            }
            
            this[settings.editorName + "artifactFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "artifactFoldout"], "Artifacts");

            if (this[settings.editorName + "artifactFoldout"])
            {
                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                DrawInputArea(settings);
                DrawStatsList(settings);
                DrawLeaderboardList(settings);
                DrawAchievementList(settings);

                EditorGUI.indentLevel = il;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawServerSettings()
        {
            this["sgsFoldout"] = EditorGUILayout.Foldout(this["sgsFoldout"], "Steam Game Server Configuration");

            if (this["sgsFoldout"])
            {
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.LinkButton("Default"))
                {
                    settings.defaultServerSettings = SteamGameServerConfiguration.Default;
                }
                if (EditorGUILayout.LinkButton("Clear"))
                {
                    settings.defaultServerSettings = new();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                DrawServerToggleSettings();
                EditorGUILayout.Space();
                DrawConnectionSettings();
                EditorGUILayout.Space();
                DrawServerGeneralSettings();
            }
        }

        private void DrawServerGeneralSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("General", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            if (!settings.defaultServerSettings.anonymousServerLogin)
            {
                EditorGUILayout.HelpBox("If anonymous server login is not enabled then you must provide a game server token.", MessageType.Info);

                var token = EditorGUILayout.TextField("Token", settings.defaultServerSettings.gameServerToken);

                if (token != settings.defaultServerSettings.gameServerToken)
                {
                    Undo.RecordObject(settings, "editor");
                    settings.defaultServerSettings.gameServerToken = token;
                    settings.isDirty = true;
                    EditorUtility.SetDirty(settings);
                }
            }

            var serverName = EditorGUILayout.TextField("Server Name", settings.defaultServerSettings.serverName);

            if (serverName != settings.defaultServerSettings.serverName)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.serverName = serverName;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            var serverVersion = EditorGUILayout.TextField("Server Version", settings.defaultServerSettings.serverVersion);

            if (serverVersion != settings.defaultServerSettings.serverVersion)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.serverVersion = serverVersion;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (settings.defaultServerSettings.supportSpectators)
            {
                serverName = EditorGUILayout.TextField("Spectator Name", settings.defaultServerSettings.spectatorServerName);

                if (serverName != settings.defaultServerSettings.spectatorServerName)
                {
                    Undo.RecordObject(settings, "editor");
                    settings.defaultServerSettings.spectatorServerName = serverName;
                    settings.isDirty = true;
                    EditorUtility.SetDirty(settings);
                }
            }

            serverName = EditorGUILayout.TextField("Description", settings.defaultServerSettings.gameDescription);

            if (serverName != settings.defaultServerSettings.gameDescription)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.gameDescription = serverName;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            serverName = EditorGUILayout.TextField("Directory", settings.defaultServerSettings.gameDirectory);

            if (serverName != settings.defaultServerSettings.gameDirectory)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.gameDirectory = serverName;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            serverName = EditorGUILayout.TextField("Map Name", settings.defaultServerSettings.mapName);

            if (serverName != settings.defaultServerSettings.mapName)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.mapName = serverName;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            serverName = EditorGUILayout.TextField("Game Metadata", settings.defaultServerSettings.gameData);

            if (serverName != settings.defaultServerSettings.gameData)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.gameData = serverName;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            var count = EditorGUILayout.TextField("Max Player Count", settings.defaultServerSettings.maxPlayerCount.ToString());
            int buffer;
            if (int.TryParse(count, out buffer) && buffer != settings.defaultServerSettings.maxPlayerCount)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.maxPlayerCount = buffer;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            count = EditorGUILayout.TextField("Bot Player Count", settings.defaultServerSettings.botPlayerCount.ToString());

            if (int.TryParse(count, out buffer) && buffer != settings.defaultServerSettings.botPlayerCount)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.botPlayerCount = buffer;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }
        }

        private void DrawConnectionSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Connection", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var address = API.Utilities.IPUintToString(settings.defaultServerSettings.ip);
            var nAddress = EditorGUILayout.TextField("IP Address", address);

            if (address != nAddress)
            {
                try
                {
                    var nip = API.Utilities.IPStringToUint(nAddress);
                    if (nip != settings.defaultServerSettings.ip)
                    {
                        Undo.RecordObject(settings, "editor");
                        settings.defaultServerSettings.ip = nip;
                        settings.isDirty = true;
                        EditorUtility.SetDirty(settings);
                    }
                }
                catch { }
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Ports ");
            EditorGUILayout.EndHorizontal();

            var port = EditorGUILayout.TextField(new GUIContent("Game", "The port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port."), settings.defaultServerSettings.gamePort.ToString());
            ushort nPort;

            if (ushort.TryParse(port, out nPort) && nPort != settings.defaultServerSettings.gamePort)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.gamePort = nPort;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            port = EditorGUILayout.TextField(new GUIContent("Query", "The port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE (65535) for QueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master server updater. See references to GameSocketShare in isteamgameserver.h"), settings.defaultServerSettings.queryPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != settings.defaultServerSettings.queryPort)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.queryPort = nPort;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            port = EditorGUILayout.TextField("Spectator", settings.defaultServerSettings.spectatorPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != settings.defaultServerSettings.spectatorPort)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.spectatorPort = nPort;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }
        }

        private void DrawServerToggleSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Features", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var autoInt = GUILayout.Toggle(settings.defaultServerSettings.autoInitialize, (settings.defaultServerSettings.autoInitialize ? "Disable" : "Enable") + " Auto-Initialize", EditorStyles.toolbarButton);
            var autoLog = GUILayout.Toggle(settings.defaultServerSettings.autoLogon, (settings.defaultServerSettings.autoLogon ? "Disable" : "Enable") + " Auto-Logon", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var heart = GUILayout.Toggle(settings.defaultServerSettings.enableHeartbeats, (settings.defaultServerSettings.enableHeartbeats ? "Disable" : "Enable") + " Server Heartbeat", EditorStyles.toolbarButton);
            var anon = GUILayout.Toggle(settings.defaultServerSettings.anonymousServerLogin, (settings.defaultServerSettings.anonymousServerLogin ? "Disable" : "Enable") + " Anonymous Server Login", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var gsAuth = GUILayout.Toggle(settings.defaultServerSettings.usingGameServerAuthApi, (settings.defaultServerSettings.usingGameServerAuthApi ? "Disable" : "Enable") + " Game Server Auth API", EditorStyles.toolbarButton);
            var pass = GUILayout.Toggle(settings.defaultServerSettings.isPasswordProtected, (settings.defaultServerSettings.isPasswordProtected ? "Disable" : "Enable") + " Password Protected", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var dedicated = GUILayout.Toggle(settings.defaultServerSettings.isDedicated, (settings.defaultServerSettings.isDedicated ? "Disable" : "Enable") + " Dedicated Server", EditorStyles.toolbarButton);
            var spec = GUILayout.Toggle(settings.defaultServerSettings.supportSpectators, (settings.defaultServerSettings.supportSpectators ? "Disable" : "Enable") + " Spectator Support", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            
            if (autoInt != settings.defaultServerSettings.autoInitialize)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.autoInitialize = autoInt;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (heart != settings.defaultServerSettings.enableHeartbeats)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.enableHeartbeats = heart;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (spec != settings.defaultServerSettings.supportSpectators)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.supportSpectators = spec;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (anon != settings.defaultServerSettings.anonymousServerLogin)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.anonymousServerLogin = anon;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (gsAuth != settings.defaultServerSettings.usingGameServerAuthApi)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.usingGameServerAuthApi = gsAuth;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (pass != settings.defaultServerSettings.isPasswordProtected)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.isPasswordProtected = pass;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (dedicated != settings.defaultServerSettings.isDedicated)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.isDedicated = dedicated;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }

            if (autoLog != settings.defaultServerSettings.autoLogon)
            {
                Undo.RecordObject(settings, "editor");
                settings.defaultServerSettings.autoLogon = autoLog;
                settings.isDirty = true;
                EditorUtility.SetDirty(settings);
            }
        }

        private void DrawStatsList(SteamToolsSettings.AppSettings settings)
        {
            this[settings.editorName + "statsFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "statsFoldout"], "Stats: " + settings.stats.Count);

            if (this[settings.editorName + "statsFoldout"])
            {
                int mil = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamTools.Colors.BrightGreen;
                if (GUILayout.Button("+ Int", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    StatData nStat = "New_Int_Stat";
                    settings.stats.Add(nStat);
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
                if (GUILayout.Button("+ Float", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    StatData nStat = "New_Float_Stat";
                    settings.stats.Add(nStat);
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
                if (GUILayout.Button("+ Avg Rate", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    GUI.FocusControl(null);

                    StatData nStat = "New_AvgRat_Stat";
                    settings.stats.Add(nStat);
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.stats.Count; i++)
                {
                    var target = settings.stats[i];

                    UnityEngine.Color sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var newName = EditorGUILayout.TextField(target);
                    if (!string.IsNullOrEmpty(newName) && newName != target)
                    {
                        Undo.RecordObject(this.settings, "name change");
                        settings.stats[i] = newName;
                        this.settings.isDirty = true;
                        EditorUtility.SetDirty(this.settings);
                    }

                    var terminate = false;
                    GUI.contentColor = SteamTools.Colors.ErrorRed;
                    if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        settings.stats.RemoveAt(i);
                        terminate = true;
                        EditorUtility.SetDirty(this.settings);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    if (terminate)
                    {
                        break;
                    }
                }
                EditorGUI.indentLevel = il;

                EditorGUI.indentLevel = mil;
            }
        }

        private void DrawAchievementList(SteamToolsSettings.AppSettings settings)
        {
            this[settings.editorName + "achievements"] = EditorGUILayout.Foldout(this[settings.editorName + "achievements"], "Achievements: " + settings.achievements.Count);

            if (this[settings.editorName + "achievements"])
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = color;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialized)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!Steamworks.SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    try
                    {
                        GUI.FocusControl(null);

                        var names = API.StatsAndAchievements.Client.GetAchievementNames();
                        settings.achievements.Clear();
                        foreach (var name in names)
                            settings.achievements.Add(name);
                        EditorUtility.SetDirty(this.settings);
                    }
                    catch
                    {
                        Debug.LogWarning("Achievements can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.achievements.Count; i++)
                {
                    var target = settings.achievements[i];

                    UnityEngine.Color sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                    EditorGUILayout.LabelField(target.ApiName);
                    if (UnityEngine.Application.isPlaying && API.App.Initialized)
                        EditorGUILayout.LabelField(target.IsAchieved ? "Unlocked" : "Locked");

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel = il;
            }
        }

        private void DrawLeaderboardList(SteamToolsSettings.AppSettings settings)
        {
            this[settings.editorName + "leaderboardFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "leaderboardFoldout"], "Leaderboards: " + settings.leaderboards.Count);

            if (this[settings.editorName + "leaderboardFoldout"])
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamTools.Colors.BrightGreen;
                if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);
                    Undo.RecordObject(this.settings, "editor");
                    LeaderboardData.GetAllRequest nStat = new()
                    {
                        name = "New Leaderboard",
                        create = false,
                        sort = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending,
                        type = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric,
                    };

                    settings.leaderboards.Add(nStat);
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                var bgColor = GUI.backgroundColor;

                for (int i = 0; i < settings.leaderboards.Count; i++)
                {
                    var item = settings.leaderboards[i];

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button(new GUIContent(item.create ? "✓" : "-", "Create if missing?"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        item.create = !item.create;
                        settings.leaderboards[i] = item;
                        
                        EditorUtility.SetDirty(this.settings);
                    }

                    var nVal = EditorGUILayout.TextField(item.name);
                    if (nVal != item.name)
                    {
                        item.name = nVal;
                        settings.leaderboards[i] = item;
                        var boards = settings.leaderboards.ToArray();
                        settings.leaderboards.Clear();
                        settings.leaderboards.AddRange(boards);
                        EditorUtility.SetDirty(this.settings);
                    }

                    GUI.contentColor = SteamTools.Colors.ErrorRed;
                    if (GUILayout.Button(new GUIContent("X", "Remove the object"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        Undo.RecordObject(this.settings, "editor");
                        settings.leaderboards.RemoveAt(i);
                        this.settings.isDirty = true;
                        EditorUtility.SetDirty(this.settings);
                        return;
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();
                }
                
                GUI.backgroundColor = bgColor;
            }
        }

        private void DrawDLCList()
        {
            this["dlcFoldout"] = EditorGUILayout.Foldout(this["dlcFoldout"], "Downloadable Content: " + settings.dlc.Count);

            if (this["dlcFoldout"])
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialized)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!Steamworks.SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    GUI.FocusControl(null);
                    try
                    {
                        var dlc = API.App.Client.Dlc;
                        settings.dlcNames.Clear();
                        settings.dlc.Clear();

                        foreach(var data in dlc)
                        {
                            settings.dlc.Add(data);
                            settings.dlcNames.Add(data.Name);
                        }

                        this.settings.isDirty = true;
                        EditorUtility.SetDirty(this.settings);
                    }
                    catch
                    {
                        Debug.LogWarning("DLC can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                var bgColor = GUI.backgroundColor;

                for (int i = 0; i < settings.dlcNames.Count; i++)
                {
                    var item = settings.dlcNames[i];

                    EditorGUILayout.LabelField(item);
                }

                GUI.backgroundColor = bgColor;
            }
        }

        private void DrawInventoryArea()
        {
            settings.inventorySettings.items.RemoveAll(p => p == null);

            this["inventoryFoldout"] = EditorGUILayout.Foldout(this["inventoryFoldout"], "Inventory: " + settings.inventorySettings.items.Count);

            if (this["inventoryFoldout"])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                var color = GUI.contentColor;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (!API.App.Initialized)
                    {
                        Debug.Log("You cannot import data before initializing Steam API.");
                        return;
                    }

                    if (!Steamworks.SteamUser.BLoggedOn())
                    {
                        Debug.Log("You cannot import data when the Steam client is in offline mode.");
                        return;
                    }

                    GUI.FocusControl(null);

                    try
                    {
                        Debug.Log("Processing inventory item definition cashe!");
                        settings.inventorySettings.items.Clear();
                        settings.inventorySettings.UpdateItemDefinitions(true);
                        Debug.Log("Requesting Refresh of Steam Inventory Item Definitions");

                        API.Inventory.Client.OnSteamInventoryDefinitionUpdate.RemoveListener(settings.inventorySettings.HandleSettingsInventoryDefinitionUpdate);
                        API.Inventory.Client.OnSteamInventoryDefinitionUpdate.AddListener(settings.inventorySettings.HandleSettingsInventoryDefinitionUpdate);
                        API.Inventory.Client.LoadItemDefinitions();
                    }
                    catch
                    {
                        Debug.LogWarning("Failed to import data from Steam, make sure you have simulated/ran at least once in order to engage the Steam API.");
                    }

                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                    GUI.FocusControl(null);
                }
                GUI.contentColor = color;


                EditorGUILayout.EndHorizontal();

                this["itemsFoldout"] = EditorGUILayout.Foldout(this["itemsFoldout"], "Items: " + settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.item).Count());

                if (this["itemsFoldout"])
                {
                    settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.inventorySettings.items.Count; i++)
                    {
                        var item = settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.item)
                        {
                            if (DrawItem(settings, item))
                                break;
                        }
                    }
                }

                this["bundlesFoldout"] = EditorGUILayout.Foldout(this["bundlesFoldout"], "Bundles: " + settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.bundle).Count());

                if (this["bundlesFoldout"])
                {
                    settings.inventorySettings.items.RemoveAll(p => p == null);
                    settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.inventorySettings.items.Count; i++)
                    {
                        var item = settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.bundle)
                        {
                            if (DrawItem(settings, item))
                                break;
                        }
                    }
                }

                this["generatorFoldout"] = EditorGUILayout.Foldout(this["generatorFoldout"], "Generators: " + settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.generator).Count());

                if (this["generatorFoldout"])
                {
                    settings.inventorySettings.items.RemoveAll(p => p == null);
                    settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.inventorySettings.items.Count; i++)
                    {
                        var item = settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.generator)
                        {
                            if (DrawItem(settings, item))
                                break;
                        }
                    }
                }

                this["playtimegeneratorFoldout"] = EditorGUILayout.Foldout(this["playtimegeneratorFoldout"], "Playtime Generators: " + settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.playtimegenerator).Count());

                if (this["playtimegeneratorFoldout"])
                {
                    settings.inventorySettings.items.RemoveAll(p => p == null);
                    settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.inventorySettings.items.Count; i++)
                    {
                        var item = settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.playtimegenerator)
                        {
                            if (DrawItem(settings, item))
                                break;
                        }
                    }
                }

                this["taggeneratorFoldout"] = EditorGUILayout.Foldout(this["taggeneratorFoldout"], "Tag Generators: " + settings.inventorySettings.items.Where(p => p.Type == InventoryItemType.tag_generator).Count());

                if (this["taggeneratorFoldout"])
                {
                    settings.inventorySettings.items.RemoveAll(p => p == null);
                    settings.inventorySettings.items.Sort((a, b) => a.Id.m_SteamItemDef.CompareTo(b.Id.m_SteamItemDef));

                    for (int i = 0; i < settings.inventorySettings.items.Count; i++)
                    {
                        var item = settings.inventorySettings.items[i];

                        if (item.Type == InventoryItemType.tag_generator)
                        {
                            if (DrawItem(settings, item))
                                break;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawInputArea(SteamToolsSettings.AppSettings settings)
        {
            //this[settings.name + "inputFoldout"]
            this[settings.editorName + "inputFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputFoldout"], "Input: " + (settings.actions.Count + settings.actionSets.Count + settings.actionSetLayers.Count).ToString());

            if (this[settings.editorName + "inputFoldout"])
            {

                EditorGUI.indentLevel++;

                this[settings.editorName + "inputActionSetFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputActionSetFoldout"], "Action Sets: " + settings.actionSets.Count.ToString());

                if (this[settings.editorName + "inputActionSetFoldout"])
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamTools.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        var nItem = "action_set";

                        settings.actionSets.Add(nItem);
                        this.settings.isDirty = true;
                        EditorUtility.SetDirty(this.settings);

                        GUI.FocusControl(null);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.actionSets.Count; i++)
                    {
                        if (DrawActionSet(settings, i))
                            break;
                    }
                }

                this[settings.editorName + "inputActionSetLayerFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputActionSetLayerFoldout"], "Action Set Layers: " + settings.actionSetLayers.Count.ToString());

                if (this[settings.editorName + "inputActionSetLayerFoldout"])
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamTools.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        string nItem = "action_set_layer";

                        settings.actionSetLayers.Add(nItem);
                        this.settings.isDirty = true;
                        EditorUtility.SetDirty(this.settings);

                        GUI.FocusControl(null);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.actionSetLayers.Count; i++)
                    {
                        if (DrawActionSetLayer(settings, i))
                            break;
                    }
                }

                this[settings.editorName + "inputActionFoldout"] = EditorGUILayout.Foldout(this[settings.editorName + "inputActionFoldout"], "Actions: " + settings.actions.Count.ToString());

                if (this[settings.editorName + "inputActionFoldout"])
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    var color = GUI.contentColor;
                    GUI.contentColor = SteamTools.Colors.BrightGreen;
                    if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);

                        InputActionData nItem = new("action", InputActionType.Digital);

                        settings.actions.Add(nItem);
                        this.settings.isDirty = true;
                        EditorUtility.SetDirty(this.settings);

                        GUI.FocusControl(null);
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < settings.actions.Count; i++)
                    {
                        if (DrawAction(settings, i))
                            break;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private bool DrawItem(SteamToolsSettings settings, ItemDefinitionSettings item)
        {
            var color = GUI.contentColor;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            // Draw the item name as a label
            EditorGUILayout.LabelField(item.Name, EditorStyles.boldLabel);

            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawActionSet(SteamToolsSettings.AppSettings settings, int setIndex)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var result = EditorGUILayout.TextField(settings.actionSets[setIndex]);

            if (result != settings.actionSets[setIndex])
            {
                settings.actionSets[setIndex] = result;
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
            }

            GUI.contentColor = SteamTools.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                settings.actionSets.RemoveAt(setIndex);
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawActionSetLayer(SteamToolsSettings.AppSettings settings, int index)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var result = EditorGUILayout.TextField(settings.actionSetLayers[index]);

            if (result != settings.actionSetLayers[index])
            {
                settings.actionSetLayers[index] = result;
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
            }

            GUI.contentColor = SteamTools.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                settings.actionSetLayers.RemoveAt(index);
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }

        private bool DrawAction(SteamToolsSettings.AppSettings settings, int index)
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var item = settings.actions[index];
            if (item.Type == InputActionType.Digital)
            {
                if (GUILayout.Button(new GUIContent("DI", "Click to make this an analog action."), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    item = new(item.Name, InputActionType.Analog);
                    settings.actions[index] = item;
                    GUI.FocusControl(null);
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("AI", "Click to make this a digital action."), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    item = new(item.Name, InputActionType.Digital);
                    settings.actions[index] = item;
                    GUI.FocusControl(null);
                    this.settings.isDirty = true;
                    EditorUtility.SetDirty(this.settings);
                }
            }

            var result = EditorGUILayout.TextField(item.Name);

            if (result != item.Name)
            {
                item = new(result, item.Type);
                settings.actions[index] = item;
                GUI.FocusControl(null);
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
            }

            GUI.contentColor = SteamTools.Colors.ErrorRed;
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                GUI.FocusControl(null);
                settings.actions.RemoveAt(index);
                this.settings.isDirty = true;
                EditorUtility.SetDirty(this.settings);
                return true;
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();
            return false;
        }
    }
}
#endif