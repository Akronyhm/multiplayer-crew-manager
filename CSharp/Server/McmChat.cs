using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;
using Barotrauma.Networking;

namespace MultiplayerCrewManager {
    class McmChat {
        public McmMod Mod { get; private set; }

        public McmChat(McmMod mod)
        {
            Mod = mod;
        }

        private static readonly Regex rMaskGlobal = new Regex(@"^mcm.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex rMaskHelp1 = new Regex(@"^mcm\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskHelp2 = new Regex(@"^mcm\s+help\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex rMaskList = new Regex(@"^mcm\s+list\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskControl = new Regex(@"^mcm\s+control\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskReleaseGeneral = new Regex(@"^mcm\s+release\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskReleaseId = new Regex(@"^mcm\s+release\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskDelete = new Regex(@"^mcm\s+delete\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex rMaskClientList = new Regex(@"^mcm\s+clientlist\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskSpawn = new Regex(@"^mcm\s+spawn\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex rMaskClientAutospawn = new Regex(@"^mcm\s+client\s+autospawn\s+(true|false)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskRespawn = new Regex(@"^mcm\s+respawn\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskRespawnSet = new Regex(@"^mcm\s+respawn\s+set\s+(true|false)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskRespawnPenalty = new Regex(@"^mcm\s+respawn\s+penalty\s+(true|false)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskRespawnDelay = new Regex(@"^mcm\s+respawn\s+delay\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskRespawnTime = new Regex(@"^mcm\s+respawn\s+time\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskSecure = new Regex(@"^mcm\s+secure\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskSecureEnabled = new Regex(@"^mcm\s+secure\s+(true|false)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskReserve = new Regex(@"^mcm\s+reserve\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskReservePut = new Regex(@"^mcm\s+reserve\s+put\s+\d+\s?(force)?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskReserveGet = new Regex(@"^mcm\s+reserve\s+get\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskLoggingLevelSet = new Regex(@"^mcm\slogging\s\d$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskLoggingLevelGet = new Regex(@"^mcm\slogging$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex rMaskIntValue = new Regex(@"\s+\d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex rMaskBoolValue = new Regex(@"\s+true\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public bool? OnChatMessage(string message, Client sender) {
            if (!McmMod.IsCampaign || !rMaskGlobal.IsMatch(message)) return null;

            string response = null;
            ChatMessageType messageType = ChatMessageType.ServerMessageBox;
            bool isInGame = McmMod.IsRunning;

            Action setInGameError = () => {
                response = "[MCM] Command can be used only in game and multiplayer campaign";
                messageType = ChatMessageType.Error;
            };
            Action setPrivilegeError = () => {
                response = "[MCM] Not enough privileges to use this command";
                messageType = ChatMessageType.Error;
            };

            if (rMaskHelp1.IsMatch(message) || rMaskHelp2.IsMatch(message))
            { // mcm [help]
                response = @"
mcm [function] [args] - mcm Function syntax

— mcm [help] - show help for mcm function (help keyword is optional)
— mcm list - list all controllable characters
— mcm control <ID> - try gain control of character with provided ID (only for admins/moderators if SM enabled)
— mcm release [ID] - release currently controlled character or character with ID, if provided (later only for admins/moderators)
";
                if (sender.HasPermission(ClientPermissions.ConsoleCommands)) response += @"

admin/moderator only commands

— mcm delete <ID> - delete character (with inventory) with provided ID
— mcm clientlist - list all the clients
— mcm spawn <ID> - spawn unique client character (his cosmetic presets)
— mcm client autospawn <true/false> - trun automatic spawning for new connected clients on/off

— mcm respawn - list respawn config
— mcm respawn set <true/false> - turn respawning on/off
— mcm respawn delay <number> - time to wait before respawning

— mcm secure - show the current secure mode status
— mcm secure <true/false> - secure mode to allow only admins/moderators to gain control on/off

- mcm reserve - show characters stocked in reserve
- mcm reserve put <ID> - put character in reserve with provided ID
- mcm reserve get <ID> - get character from reserve with provided ID
- mcm logging - Gets the current logging level
- mcm logging <0-4> sets the current logging level (0 - None, 1 - Error, 2 - Warn, 3 - Info, 4 - Trace)
";
            }
            else if (rMaskList.IsMatch(message))
            { // mcm list
                if (isInGame)
                {
                    response = "Controllable characters";
                    foreach (var character in Character.CharacterList.Where(c => c.TeamID == CharacterTeamType.Team1 && !c.IsDead))
                    {
                        // Team1, meaning default crew
                        response += $"\n  {character.ID} / {character.TeamID.ToString("G")} / {character.SpeciesName} | {character.Name} ({character.DisplayName})";
                    }
                }
                else setInGameError();
            }
            else if (rMaskClientList.IsMatch(message))
            { // mcm clientlist
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    response = "Current clients";
                    foreach (var client in Client.ClientList)
                        response += $"\n  {client.CharacterID} | {client.SteamID} - {client.Name}";
                }
                else setPrivilegeError();
            }
            else if (rMaskControl.IsMatch(message))
            { // mcm control <ID>
                if (isInGame)
                {
                    Int32.TryParse(rMaskIntValue.Match(message).Value, out int id);
                    Mod.Control.TryGiveControl(sender, id, McmMod.Config.SecureEnabled);
                }
                else setInGameError();
            }
            else if (rMaskReleaseGeneral.IsMatch(message))
            { // mcm release
                if (isInGame)
                {
                    if (sender.Character == null)
                    {
                        response = "[MCM] Not currently controlling any characters";
                        messageType = ChatMessageType.Error;
                    }
                    else
                    {
                        Mod.Manager.Set(sender, null);
                        response = "[MCM] Current character released";
                        messageType = ChatMessageType.Server;
                    }
                }
                else setInGameError();
            }
            else if (rMaskReleaseId.IsMatch(message))
            { // mcm release <ID>
                if (isInGame)
                {
                    Int32.TryParse(rMaskIntValue.Match(message).Value, out int id);
                    if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                    {
                        var client = Client.ClientList.FirstOrDefault(c => c.Character != null && c.Character.ID == id);
                        if (client != null)
                        {
                            response = $"[MCM] Character ID - {id} ({client.Character.Name}) released";
                            messageType = ChatMessageType.Server;
                            Mod.Manager.Set(client, null);
                        }
                        else
                        {
                            response = $"[MCM] Clinet with the character {id} not found";
                            messageType = ChatMessageType.Error;
                        }
                    }
                    else setPrivilegeError();
                }
                else setInGameError();
            }
            else if (rMaskDelete.IsMatch(message))
            { // mcm delete <ID>
                if (isInGame)
                {
                    Int32.TryParse(rMaskIntValue.Match(message).Value, out int id);
                    if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                    {
                        var character = Character.CharacterList.FirstOrDefault(c => c.TeamID == CharacterTeamType.Team1 && c.ID == id);
                        if (character != null)
                        {
                            Entity.Spawner.AddEntityToRemoveQueue(character);
                            GameMain.GameSession.CrewManager.RemoveCharacter(character, true, true);
                            response = $"[MCM] Character ID - {id} ({character.Name}) was removed";
                            messageType = ChatMessageType.Server;
                        }
                        else
                        {
                            response = $"[MCM] Character with ID {id} not found";
                            messageType = ChatMessageType.Error;
                        }
                    }
                    else setPrivilegeError();
                }
                else setInGameError();
            }
            else if (rMaskSpawn.IsMatch(message))
            { // mcm spawn <ID>
                if (isInGame)
                {
                    if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                    {
                        Int32.TryParse(rMaskIntValue.Match(message).Value, out int clientId);

                        messageType = ChatMessageType.Error;
                        var client = Client.ClientList.FirstOrDefault(c => clientId == c.Character.ID);
                        if (client != null)
                        {
                            if (Mod.TryCeateClientCharacter(client))
                            {
                                response = $"[MCM] Character spawned for client ID - {clientId}";
                                messageType = ChatMessageType.Server;
                            }
                            else
                            {
                                response = $"[MCM] Could not spawn character for client ID - {clientId} , spawn waypoint not found";
                            }
                        }
                        else response = $"[MCM] Client with ID {clientId} not found";
                    }
                    else setPrivilegeError();
                }
                else setInGameError();
            }
            else if (rMaskClientAutospawn.IsMatch(message))
            { // mcm client autospawn
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    Boolean.TryParse(rMaskBoolValue.Match(message).Value, out bool value);
                    if (value) response = "[MCM] Automatic new client character spawn is turned ON";
                    else response = "[MCM] Automatic new client character spawn is turned OFF";
                    McmMod.Config.AllowSpawnNewClients = value;
                    McmMod.SaveConfig();
                }
                else setPrivilegeError();
            }
            else if (rMaskRespawnSet.IsMatch(message))
            { // mcm respawn set <true/false> 
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    Boolean.TryParse(rMaskBoolValue.Match(message).Value, out bool value);
                    if (value) response = "[MCM] Respawning is turned ON";
                    else response = "[MCM] Respawning is turned OFF";
                    GameMain.Server.ServerSettings.AllowRespawn = value;
                    McmMod.SaveConfig();
                }
                else setPrivilegeError();
            }
            else if (rMaskRespawnDelay.IsMatch(message))
            { // mcm respawn delay <number>
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    Int32.TryParse(rMaskIntValue.Match(message).Value, out int delay);
                    response = $"[MCM] Respawn delay is set to {delay} seconds";
                    McmMod.Config.RespawnDelay = (float)delay;
                    McmMod.SaveConfig();
                }
                else setPrivilegeError();
            }
            else if (rMaskRespawn.IsMatch(message))
            { // mcm respawn time <number>
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    var confStr = new[]{
                        ("Client Autospawn", $"{McmMod.Config.AllowSpawnNewClients}"),
                        ("Allow Respawns", $"{GameMain.Server.ServerSettings.AllowRespawn}"),
                        //("Penalty", $"{McmMod.Config.RespawnPenalty}"),
                        ("Delay", $"{McmMod.Config.RespawnDelay}"),
                        ("Time", $"{GameMain.Server.ServerSettings.RespawnInterval}"),
                    }.Select(s => $"\n— {s.Item1}:    {s.Item2}").Aggregate((s1, s2) => $"{s1}{s2}");
                    response = $"Respawn Config:{confStr}";
                }
                else setPrivilegeError();
            }
            else if (rMaskSecure.IsMatch(message))
            { // mcm secure
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    var confStr = new[]{
                        ("Secure Mode", $"{McmMod.Config.SecureEnabled}")
                    }.Select(s => $"\n— {s.Item1}:    {s.Item2}").Aggregate((s1, s2) => $"{s1}{s2}");
                    response = $"Secure Mode:{confStr}";
                }
                else setPrivilegeError();
            }
            else if (rMaskSecureEnabled.IsMatch(message))
            { // mcm secure <true/false>
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    Boolean.TryParse(rMaskBoolValue.Match(message).Value, out bool value);
                    if (value) response = "[MCM] Secure mode is turned ON";
                    else response = "[MCM] Secure mode is turned OFF";
                    McmMod.Config.SecureEnabled = value;
                    McmMod.SaveConfig();
                }
                else setPrivilegeError();
            }
            else if (rMaskReserve.IsMatch(message))
            { // mcm reserve
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    McmReserve.showReserveList(client: sender);
                }
                else setPrivilegeError();
            }
            else if (rMaskReservePut.IsMatch(message))
            { // mcm reserve put <ID>
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    string digits = new string(message.Where(d => char.IsDigit(d)).ToArray());
                    Int32.TryParse(digits, out int value);
                    bool isForce = message.Contains("force");
                    McmReserve.putCharacterToReserve(charId: value, client: sender, isForce: isForce);
                }
                else setPrivilegeError();
            }
            else if (rMaskReserveGet.IsMatch(message))
            { // mcm reserve get <ID>
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    Int32.TryParse(rMaskIntValue.Match(message).Value, out int value);
                    McmReserve.getCharacterFromReserve(ordinal: value, client: sender);
                }
                else setPrivilegeError();
            }
            else if (rMaskLoggingLevelGet.IsMatch(message))
            {
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    var loggingLevel = McmMod.Config.LoggingLevel;
                    string correspondingLoggingLevel = "";
                    switch (loggingLevel)
                    {
                        case 0:
                            correspondingLoggingLevel = "None";
                            break;
                        case 1:
                            correspondingLoggingLevel = "Error";
                            break;
                        case 2:
                            correspondingLoggingLevel = "Warn";
                            break;
                        case 3:
                            correspondingLoggingLevel = "Info";
                            break;
                        default:
                            correspondingLoggingLevel = "Trace";
                            break;
                    }
                    response = $"[MCM] - Current logging level set to [{correspondingLoggingLevel}]";
                }
                else setPrivilegeError();
            }
            else if (rMaskLoggingLevelSet.IsMatch(message))
            {
                if (sender.HasPermission(ClientPermissions.ConsoleCommands))
                {
                    messageType = ChatMessageType.Server;
                    if (Int32.TryParse(rMaskIntValue.Match(message).Value, out int value))
                    {
                        //Clamp value between 0-4
                        if (value > 4)
                            value = 4;
                        if (value < 0)
                            value = 0;
                        McmMod.Config.LoggingLevel = value;
                        McmMod.SaveConfig();
                        string correspondingLoggingLevel = "";
                        switch (value)
                        {
                            case 0:
                                correspondingLoggingLevel = "None";
                                break;
                            case 1:
                                correspondingLoggingLevel = "Error";
                                break;
                            case 2:
                                correspondingLoggingLevel = "Warn";
                                break;
                            case 3:
                                correspondingLoggingLevel = "Info";
                                break;
                            default:
                                correspondingLoggingLevel = "Trace";
                                break;
                        }
                        response = $"[MCM] - Set logging level to [{correspondingLoggingLevel}]";
                    }
                    else
                    {
                        response = $"ERROR - Could not parse integer from message [{message}]";
                        McmUtils.Warn($"Could not parse integer from message [{message}]");
                    }
                }
                else setPrivilegeError();
            }
            else
            {
                response = "[MCM] error: Incorrect function or argument.";
                messageType = ChatMessageType.Error;
            }

            // send message
            if (response != null) {
                var cm = ChatMessage.Create("[Server]", response, messageType, null, sender);
                cm.IconStyle = "StoreShoppingCrateIcon";
                GameMain.Server.SendDirectChatMessage(cm, sender);
            }

            // no chat display
            return true;
        }
    }
}