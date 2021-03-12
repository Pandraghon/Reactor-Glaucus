using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using Essentials.Options;
using HarmonyLib;
using Reactor;
using Reactor.Patches;
using UnityEngine;
using Random = System.Random;

namespace Glaucus
{
    public enum WinReasons
    {
        Imposters, Crewmates, Jester
    }
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
        public DeathReason DeathReason { get; set; }
    }

    public class Main
    {
        public static ModdedPalette Palette = new ModdedPalette();
        public static ModdedLogic Logic = new ModdedLogic();
    }

    public class ModdedLogic
    {
        public ModPlayerControl getRolePlayer(string roleName)
        {
            return Main.Logic.AllModPlayerControl.Find(x => x.Role == roleName);
        }

        public List<ModPlayerControl> AllModPlayerControl = new List<ModPlayerControl>();

        public WinReasons WinReason = WinReasons.Crewmates;
    }

    public class ModdedPalette
    {
        public Color jesterColor = new Color(191f / 255f, 0f / 255f, 255f / 255f, 1);
        public Color sheriffColor = new Color(255f / 255f, 204f / 255f, 0f / 255f, 1);
    }

    public class ModPlayerControl
    {
        public PlayerControl PlayerControl { get; set; }
        public string Role { get; set; }
        public float reportsLeft { get; set; }
        public DateTime? LastAbilityTime { get; set; }
    }

    public static class Extensions
    {
        public static bool isPlayerRole(this PlayerControl player, string roleName)
        {
            if (player.getModdedControl() != null)
                return player.getModdedControl().Role == roleName;
            return false;
        }
        
        public static ModPlayerControl getModdedControl(this PlayerControl player)
        {
            return Main.Logic.AllModPlayerControl.Find(x => x.PlayerControl == player);
        }

        public static float getCoolDown(this PlayerControl player)
        {
            var lastAbilityTime = player.getModdedControl().LastAbilityTime;
            if (lastAbilityTime == null)
                return BetterLobby.SheriffKillCooldown.GetValue();

            var diff = (TimeSpan) (DateTime.UtcNow - lastAbilityTime);
            var cooldown = BetterLobby.SheriffKillCooldown.GetValue() - (float) diff.TotalSeconds;

            if (cooldown < 0)
                return 0;

            return cooldown;
        }
    }
    
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class BetterLobby : BasePlugin
    {
        public const string Id = "glaucus.pocus.BetterLobby";

        public static ManualLogSource log;

        //Credit to https://github.com/DorCoMaNdO/Reactor-Essentials
        public static CustomNumberOption MaxReportCount = CustomOption.AddNumber("# Kill Reports", 
            8, 0, 100, 1);
        public static CustomStringOption WhoCanVent = CustomOption.AddString("Who Can Vent", 
            new string[] { "Nobody", "Impostors", "Everyone" });
        public static CustomToggleOption ImpostorsKnowEachother = CustomOption.AddToggle("Impostors Know Eachother", true);
        public static CustomNumberOption JesterSpawnChance =
            CustomOption.AddNumber("Jester Spawn Chance", 100, 0, 100, 5);
        public static CustomNumberOption SheriffSpawnChance =
            CustomOption.AddNumber("Sheriff Spawn Chance", 100, 0, 100, 5);
        public static CustomNumberOption SheriffKillCooldown =
            CustomOption.AddNumber("Sheriff Kill Cooldown", 30f, 10f, 45f, 2.5f);
        public Harmony Harmony { get; } = new Harmony(Id);
        
        public static List<DeadPlayer> killedPlayers = new List<DeadPlayer>();
        public static PlayerControl CurrentTarget = null;
        public static PlayerControl localPlayer = null;
        public static List<PlayerControl> localPlayers = new List<PlayerControl>();
        // the kill button in the bottom right
        public static KillButtonManager KillButton;
        // the report button
        public static ReportButtonManager ReportButton;
        // distance between the local player and closest player
        public static double DistLocalClosest;
        // RNG generator for role attribution
        public static Random rng = new Random();

        public override void Load()
        {
            CustomOption.ShamelessPlug = false;

            ReactorVersionShower.TextUpdated += (text) =>
            {
                int index = text.Text.LastIndexOf('\n');
                text.Text = text.Text.Insert(index == -1 ? text.Text.Length - 1 : index, 
                    "\n[00CC66FF]" + typeof(BetterLobby).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description + " " + typeof(BetterLobby).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion + "[] by Pandraghon");
            };
            
            Harmony.PatchAll();
        }
    }
}