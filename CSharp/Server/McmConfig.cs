using System;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;
using Barotrauma.Networking;

namespace MultiplayerCrewManager
{
    public enum McmLoggingLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Trace = 4
    }
    public class McmConfig
    {
        public McmLoggingLevel LoggingLevel = McmLoggingLevel.Info;

        public int ServerUpdateFrequency = 15;
        public bool AllowSpawnNewClients = false;
        public bool SecureEnabled = false;
        //public float RespawnDelay = 5;


        public McmConfig()
        {
        }

        public float MaxTransportTime
        {
            get => GameMain.NetworkMember.ServerSettings.MaxTransportTime;
        }

        public float RespawnInterval
        {
            get => GameMain.NetworkMember.ServerSettings.RespawnInterval;
        }

        public bool AllowRespawn
        {
            get
            {
                var allowMissionRespawn = GameMain.GameSession.GameMode is not MissionMode missionMode || false == missionMode.Missions.Any(m => false == m.AllowRespawn);
                return allowMissionRespawn;
            }
        }

        public float SkillLossPercentageOnDeath
        {
            get => GameMain.NetworkMember.ServerSettings.SkillLossPercentageOnDeath;
        }

        public override string ToString()
        {
            return
                $"{nameof(AllowRespawn)}: {AllowRespawn}\n" +
                $"{nameof(AllowSpawnNewClients)}: {AllowSpawnNewClients}\n" +
                $"{nameof(LoggingLevel)}: {(int)LoggingLevel} - {LoggingLevel}\n" +
                $"{nameof(MaxTransportTime)}: {MaxTransportTime}s\n" +
                $"{nameof(RespawnInterval)}: {RespawnInterval}s\n" +
                $"{nameof(SecureEnabled)}: {SecureEnabled},\n" +
                $"{nameof(ServerUpdateFrequency)}: {ServerUpdateFrequency}s\n" +
                $"{nameof(SkillLossPercentageOnDeath)}: {SkillLossPercentageOnDeath}%";
        }
    }

    partial class McmMod
    {

        public static McmConfig Config { get; private set; }
        public static McmConfig GetConfig
        {
            get
            {
                if (Config == null)
                    LoadConfig();

                return Config;
            }
        }

        public static void LoadConfig()
        {
            var cfgFilePath = System.IO.Path.Combine(McmUtils.GetModStoreDirectory(), "McmConfig.xml");
            try
            {
                if (System.IO.File.Exists(cfgFilePath))
                {
                    McmUtils.Raw("Loading config file...");
                    var serializer = new XmlSerializer(typeof(McmConfig));
                    using (var fstream = System.IO.File.OpenRead(cfgFilePath))
                    {
                        Config = (McmConfig)serializer.Deserialize(fstream);
                        //McmUtils.Raw($"Loaded config file:\n   [{Config.ToString()}]");
                    }
                }
                else
                {
                    Config = new McmConfig();
                    McmUtils.Raw("Generating new MCM config file");
                    SaveConfig();
                }
            }
            catch (System.Exception e)
            {
                Config = new McmConfig();
                McmUtils.Raw(e, "Error while loading config");
                SaveConfig();
            }
        }

        public static void SaveConfig()
        {
            var cfgFilePath = System.IO.Path.Combine(McmUtils.GetModStoreDirectory(), "McmConfig.xml");
            if (false == string.IsNullOrWhiteSpace(cfgFilePath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(McmConfig));
                    using (var fstream = new System.IO.StreamWriter(cfgFilePath))
                    {
                        serializer.Serialize(fstream, Config);
                        McmUtils.Raw("Saved config file");
                    }
                }
                catch (Exception e)
                {
                    McmUtils.Error(e, "Error while saving");
                }
            }
        }
    }
}