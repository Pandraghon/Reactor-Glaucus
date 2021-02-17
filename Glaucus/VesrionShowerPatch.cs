using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace Glaucus
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class VersionShowerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch]
        [HarmonyPriority(Priority.First)]
        public static void Postfix(VersionShower __instance)
        {
            var reactorVS = GameObject.Find("ReactorVersion");
            GameObject.Destroy(reactorVS);

            TextRenderer text = __instance.text;
            text.Text += "\nLoaded [F7A700FF]Glaucus " + Glaucus.versionString + "[] by Pandraghon";
        }
    }
}