﻿using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static Glaucus.Glaucus;

namespace Glaucus
{
    [HarmonyPatch(typeof(EndGameManager), "SetEverythingUp")]
    public static class EndGamePatch
    {
        public static bool Prefix()
        {
            if (TempData.winners.Count > 1 && TempData.DidHumansWin(TempData.EndReason))
            {
                TempData.winners.Clear();
                List<PlayerControl> orderLocalPlayers = new List<PlayerControl>();
                foreach (PlayerControl player in localPlayers)
                    if (player.PlayerId == localPlayer.PlayerId)
                        orderLocalPlayers.Add(player);
                foreach (PlayerControl player in localPlayers)
                    if (player.PlayerId != localPlayer.PlayerId)
                        orderLocalPlayers.Add(player);
                foreach (PlayerControl winner in orderLocalPlayers)
                    TempData.winners.Add(new WinningPlayerData(winner.Data));
            }
            return true;
        }

        public static void Postfix(EndGameManager __instance)
        {
            if (TempData.DidHumansWin(TempData.EndReason))
            {
                bool flag = true;
                foreach (PlayerControl player in localPlayers)
                {
                    if (player.PlayerId == localPlayer.PlayerId)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    __instance.WinText.Text = "Defeat";
                    __instance.WinText.Color = Palette.ImpostorRed;
                    __instance.BackgroundBar.material.color = new Color(1, 0, 0);
                }
            }
                
            if (Main.Logic.WinReason == WinReasons.Jester)
            {
                foreach (PoolablePlayer player in Object.FindObjectsOfType<PoolablePlayer>())
                {
                    player.NameText.Color = Main.Palette.jesterColor;
                }
            }
        }
    }
}