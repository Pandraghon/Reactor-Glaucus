using System;
using HarmonyLib;

namespace Glaucus
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsImpostor && !Glaucus.ImpostorsKnowEachother.GetValue())
            {
                var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                team.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = team;
                return true;
            }

            if (PlayerControl.LocalPlayer.isPlayerRole("Jester"))
            {
                var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                team.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = team;
                return true;
            }
            return true;
        }
        
        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer.isPlayerRole("Jester"))
            {
                __instance.__this.Title.Text = "Jester";
                __instance.__this.Title.Color = Main.Palette.jesterColor;
                __instance.__this.ImpostorText.Text = "Get voted off of the ship to win";
                __instance.__this.BackgroundBar.material.color = Main.Palette.jesterColor;
            }
            
            if (PlayerControl.LocalPlayer.isPlayerRole("Sheriff"))
            {
                __instance.__this.Title.Text = "Sheriff";
                __instance.__this.Title.Color = Main.Palette.sheriffColor;
                __instance.__this.ImpostorText.Text = "Shoot the [FF0000FF]Impostor";
                __instance.__this.BackgroundBar.material.color = Main.Palette.sheriffColor;
                PlayerControl.LocalPlayer.getModdedControl().LastAbilityTime = DateTime.UtcNow;
            }
        }
    }
}