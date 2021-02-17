using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                Glaucus.KillButton = __instance.KillButton;
                PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(PlayerControl.LocalPlayer);
                Glaucus.DistLocalClosest = PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    player.nameText.Color = Color.white;
                if (PlayerControl.LocalPlayer.Data.IsImpostor)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.Data.IsImpostor && (Glaucus.ImpostorsKnowEachother.GetValue() ||
                                                       PlayerControl.LocalPlayer.PlayerId == player.PlayerId))
                            player.nameText.Color = Color.red;
                    if (!Glaucus.ImpostorsKnowEachother.GetValue() && Glaucus.DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance])
                    {
                        Glaucus.KillButton.SetTarget(PlayerTools.closestPlayer);
                        Glaucus.CurrentTarget = PlayerTools.closestPlayer;
                    }
                }
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea playerArea in MeetingHud.Instance.playerStates)
                    {
                        if (playerArea.NameText == null) continue;
                        playerArea.NameText.Color = Color.white;
                        PlayerControl player = PlayerControl.AllPlayerControls[playerArea.TargetPlayerId];
                        
                        if (player.Data.IsImpostor && (Glaucus.ImpostorsKnowEachother.GetValue() ||
                                                                             PlayerControl.LocalPlayer.PlayerId == player.PlayerId))
                            playerArea.NameText.Color = Color.red;
                    }
            }
        }
    }
}