using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;

namespace Glaucus
{
    enum CustomRPC
    {
        SetJester = 50,
        ResetVariables = 51,
        SetLocalPlayers = 56,
        JesterWin = 57
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            byte packetId = HKHMBLJFLMC;
            MessageReader reader = ALMCIJKELCP;
            switch (packetId)
            {
                case (byte)CustomRPC.SetLocalPlayers:
                    Glaucus.localPlayers.Clear();
                    Glaucus.localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = ALMCIJKELCP.ReadBytesAndSize();
                    foreach (byte id in localPlayerBytes)
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                            if (player.PlayerId == id)
                                Glaucus.localPlayers.Add(player);
                    break;
                case (byte)CustomRPC.ResetVariables:
                    Main.Logic.AllModPlayerControl.Clear();
                    List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
                    Main.Logic.WinReason = WinReasons.Crewmates;
                    foreach (PlayerControl plr in crewmates)
                        Main.Logic.AllModPlayerControl.Add(new ModPlayerControl { PlayerControl = plr, Role = "Impostor" });
                    crewmates.RemoveAll(x => x.Data.IsImpostor);
                    foreach (PlayerControl plr in crewmates)
                        plr.getModdedControl().Role = "Crewmate";
                    break;
                case (byte)CustomRPC.SetJester:
                    byte JokerId = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == JokerId)
                            player.getModdedControl().Role = "Jester";
                    break;
                case (byte)CustomRPC.JesterWin:
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
                    PlayerControl jester = Main.Logic.getRolePlayer("Jester").PlayerControl;
                    jester.Revive();
                    jester.Data.IsDead = false;
                    jester.Data.IsImpostor = true;
                    Main.Logic.WinReason = WinReasons.Jester;
                    break;
            }
        }
    }
}