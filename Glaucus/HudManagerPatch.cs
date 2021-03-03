using HarmonyLib;
using System.Linq;
using UnityEngine;
using static Glaucus.Glaucus;

namespace Glaucus
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                var infected = (from x in GameData.Instance.AllPlayers.ToArray().Where(x => x.IsImpostor).ToList() select x.PlayerId).ToList();
                KillButton = __instance.KillButton;
                ReportButton = __instance.ReportButton;
                PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(PlayerControl.LocalPlayer);
                DistLocalClosest = PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
                PlayerControl jester = null;
                
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    player.nameText.Color = Color.white;
                if (PlayerControl.LocalPlayer.Data.IsImpostor)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.Data.IsImpostor && (ImpostorsKnowEachother.GetValue() ||
                                                       PlayerControl.LocalPlayer.PlayerId == player.PlayerId))
                            player.nameText.Color = Color.red;
                    if (!ImpostorsKnowEachother.GetValue() && DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance])
                    {
                        KillButton.SetTarget(PlayerTools.closestPlayer);
                        CurrentTarget = PlayerTools.closestPlayer;
                    }
                }
                if (Main.Logic.getRolePlayer("Jester") != null && PlayerControl.LocalPlayer.isPlayerRole("Jester"))
                {
                    jester = Main.Logic.getRolePlayer("Jester").PlayerControl;
                    jester.nameText.Color = Main.Palette.jesterColor;
                }
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea playerArea in MeetingHud.Instance.playerStates)
                    {
                        if (playerArea.NameText == null) continue;
                        playerArea.NameText.Color = Color.white;
                        PlayerControl player = PlayerTools.getPlayerById((byte)playerArea.TargetPlayerId);
                        
                        // Impostor
                        if (player.Data.IsImpostor && (ImpostorsKnowEachother.GetValue() || PlayerControl.LocalPlayer.PlayerId == player.PlayerId))
                            playerArea.NameText.Color = Color.red;
                        
                        // Jester
                        if (jester != null && jester.PlayerId == playerArea.TargetPlayerId)
                            playerArea.NameText.Color = Main.Palette.jesterColor;
                    }

                ReportButton.enabled = localPlayer.getModdedControl().reportsLeft > 0;
            }
        }
    }
}