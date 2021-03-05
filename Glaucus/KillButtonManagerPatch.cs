using HarmonyLib;
using System;
using Hazel;
using static Glaucus.Glaucus;

namespace Glaucus
{
    [HarmonyPatch(typeof(KillButtonManager))]
    class KillButtonManagerPatch
    {
        [HarmonyPatch(nameof(KillButtonManager.PerformKill))]
        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;
            
            MessageWriter writer;
            if (CurrentTarget != null)
            {
                var target = CurrentTarget;
                //code that handles the ability button presses
                if (PlayerControl.LocalPlayer.isPlayerRole("Sheriff"))
                {
                    if (PlayerControl.LocalPlayer.getCoolDown() != 0)
                        return false;
                    
                    if (!target.Data.IsImpostor)
                        target = PlayerControl.LocalPlayer;

                    writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte) CustomRPC.SheriffKill, Hazel.SendOption.None, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    PlayerControl.LocalPlayer.MurderPlayer(target);
                    PlayerControl.LocalPlayer.getModdedControl().LastAbilityTime = DateTime.UtcNow;
                    
                    return false;
                }
            }
            
            return true;
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
             public static bool Prefix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
             {
                 if (__instance.isPlayerRole("Sheriff"))
                 {
                     __instance.Data.IsImpostor = true;
                 }
                 return true;
             }
             
             public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
             {
                 if (__instance.isPlayerRole("Sheriff"))
                 {
                     __instance.Data.IsImpostor = false;
                 }
             }
        }
    }
}