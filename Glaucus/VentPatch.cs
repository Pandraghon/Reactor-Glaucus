﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glaucus
{
    public static class PlayerVentTimeExtension
    {
        public static IDictionary<byte, DateTime> allVentTimes = new Dictionary<byte, DateTime>() { };

        public static void SetLastVent(byte player)
        {
            if (allVentTimes.ContainsKey(player))
                allVentTimes[player] = DateTime.UtcNow;
            else
                allVentTimes.Add(player, DateTime.UtcNow);
        }

        public static DateTime GetLastVent(byte player)
        {
            if (allVentTimes.ContainsKey(player))
                return allVentTimes[player];
            return new DateTime(0);
        }
    }

    [HarmonyPatch(typeof(Vent), "CanUse")]
    public static class VentPatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            float num = float.MaxValue;
            PlayerControl localPlayer = pc.Object;
            switch (Glaucus.WhoCanVent.GetValue())
            {
                case 0:  // Nobody
                    couldUse = false;
                    break;
                case 1:  // Impostors
                    couldUse = localPlayer.Data.IsImpostor && !localPlayer.Data.IsDead;
                    break;
                case 2:  // Everyone
                    couldUse = !localPlayer.Data.IsDead;
                    break;
                default:
                    couldUse = false;
                    break;
            }
            canUse = couldUse;
            if ((DateTime.UtcNow - PlayerVentTimeExtension.GetLastVent(pc.Object.PlayerId)).TotalMilliseconds > 1000)
            {
                num = Vector2.Distance(localPlayer.GetTruePosition(), __instance.transform.position);
                canUse &= num <= __instance.UsableDistance;
            }
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Vent), "Method_38")]
    public static class VentEnterPatch
    {
        public static void Postfix(PlayerControl NMEAPOJFNKA)
        {
            PlayerVentTimeExtension.SetLastVent(NMEAPOJFNKA.PlayerId);
        }
    }

    [HarmonyPatch(typeof(Vent), "Method_1")]
    public static class VentExitPatch
    {
        public static void Postfix(PlayerControl NMEAPOJFNKA)
        {
            PlayerVentTimeExtension.SetLastVent(NMEAPOJFNKA.PlayerId);
        }
    }
}