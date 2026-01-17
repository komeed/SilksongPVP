using HarmonyLib;
using UnityEngine;

namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(PlayerData))]
    [HarmonyPatch("TakeHealth", new[] { typeof(int), typeof(bool), typeof(bool) })]
    public static class TakeHealthPatch
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerData __instance, int amount, bool hasBlueHealth, bool allowFracturedMaskBreak)
        {
            SilksongModPlugin.Log.LogInfo("I lost a bunch of health here");
            LobbyManager.HitEnemy = true;
        }
    }
}