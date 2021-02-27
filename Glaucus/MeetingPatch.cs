using HarmonyLib;
using System;
using Hazel;

namespace Glaucus
{
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance != null && obj == ExileController.Instance.gameObject)
            {
                if (ExileController.Instance.exiled != null && ExileController.Instance.exiled._object.isPlayerRole("Jester"))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JesterWin, Hazel.SendOption.None, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (!player.isPlayerRole("Jester"))
                        {
                            player.RemoveInfected();
                            player.Die(DeathReason.Exile);
                            player.Data.IsDead = true;
                            player.Data.IsImpostor = false;
                        }
                    }
                    PlayerControl joker = Main.Logic.getRolePlayer("Jester").PlayerControl;
                    joker.Revive();
                    joker.Data.IsDead = false;
                    joker.Data.IsImpostor = true;
                    Main.Logic.WinReason = WinReasons.Jester;
                }
            }
        }
    }
}