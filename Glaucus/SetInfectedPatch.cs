using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using static Glaucus.Glaucus;

namespace Glaucus
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    public class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            Main.Logic.AllModPlayerControl.Clear();
            Main.Logic.WinReason = WinReasons.Crewmates;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.ResetVariables, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            
            List<PlayerControl> crewmates = PlayerTools.getCrewMates();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray())
                Main.Logic.AllModPlayerControl.Add(new ModPlayerControl { PlayerControl = player, Role = "Impostor"});
            foreach (PlayerControl player in crewmates)
            {
                player.getModdedControl().Role = "Crewmate";
            }
            
            if (crewmates.Count > 0 && (rng.Next(0, 100) <= JesterSpawnChance.GetValue()))
            {
                var idx = rng.Next(0, crewmates.Count);
                crewmates[idx].getModdedControl().Role = "Jester";
                byte JesterId = crewmates[idx].PlayerId;
                crewmates.RemoveAt(idx);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.SetJester, Hazel.SendOption.None, -1);
                writer.Write(JesterId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            
            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor)
                    continue;
                if (player.isPlayerRole("Joker"))
                    continue;
                else
                    localPlayers.Add(player);
            }
            var localPlayerBytes = new List<byte>();
            foreach (PlayerControl player in localPlayers)
            {
                localPlayerBytes.Add(player.PlayerId);
            }
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLocalPlayers, Hazel.SendOption.None, -1);
            writer.WriteBytesAndSize(localPlayerBytes.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}