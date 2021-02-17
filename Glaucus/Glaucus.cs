using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using Essentials.CustomOptions;
using HarmonyLib;
using Reactor;

namespace Glaucus
{
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
        public DeathReason DeathReason { get; set; }
    }
    
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class Glaucus : BasePlugin
    {
        public const string Id = "glaucus.pocus.Glaucus";
        public static string versionString = "v1.0.0";

        public static ManualLogSource log;

        //Credit to https://github.com/DorCoMaNdO/Reactor-Essentials
        public static CustomStringOption WhoCanVent = CustomOption.AddString("Who Can Vent", 
            new string[] { "Nobody", "Impostors", "Everyone" });
        public static CustomToggleOption ImpostorsKnowEachother = CustomOption.AddToggle("Impostors Know Eachother", true);
        
        public Harmony Harmony { get; } = new Harmony(Id);

        
        public ConfigEntry<string> ServerName { get; private set; }
        public ConfigEntry<string> ServerHost { get; private set; }
        public ConfigEntry<ushort> ServerPort { get; private set; }
        
        
        public static List<DeadPlayer> killedPlayers = new List<DeadPlayer>();
        public static PlayerControl CurrentTarget = null;
        public static PlayerControl localPlayer = null;
        public static List<PlayerControl> localPlayers = new List<PlayerControl>();
        //the kill button in the bottom right
        public static KillButtonManager KillButton;
        //distance between the local player and closest player
        public static double DistLocalClosest;

        public override void Load()
        {
            ServerName = Config.Bind("Server", "Name", "Glaucus Pocus");
            ServerHost = Config.Bind("Server", "Hostname", "127.0.0.1");
            ServerPort = Config.Bind("Server", "Port", (ushort) 22023);

            var defaultRegions = ServerManager.DefaultRegions.ToList();
            var ip = ServerHost.Value;
            if (Uri.CheckSchemeName(ServerHost.Value).ToString() == "Dns")
            {
                try
                {
                    foreach (var address in Dns.GetHostAddresses(ServerHost.Value))
                    {
                        if (address.AddressFamily != AddressFamily.InterNetwork) continue;
                        ip = address.ToString();
                        break;
                    }
                }
                catch
                {
                    log.LogMessage("Hostname could not be resolved" + ip);
                }
                log.LogMessage("IP is " + ip);
            }

            defaultRegions.Insert(0, new RegionInfo(ServerName.Value, ip, new[]
            {
                new ServerInfo($"{ServerName.Value}-Master-1", ip, ServerPort.Value)
            }));

            ServerManager.DefaultRegions = defaultRegions.ToArray();


            Harmony.PatchAll();
        }
    }
}