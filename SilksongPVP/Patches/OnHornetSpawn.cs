using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SilksongMod
{
    [HarmonyPatch(typeof(HeroController), "Awake")] 
    public static class OnHornetSpawn
    {
        [HarmonyPostfix]
        public static void Postfix(HeroController __instance)
        {
            // if (GlobalHost.HostEnabled)
            // {
            SilksongModPlugin.Log.LogInfo(
                "Hornet Spawned in Awake!"); // if this doesn't work simply use search component thing
            LobbyManager.SetHostHornet(__instance.gameObject);
            // retrieve every 
            LobbyManager.StoreNailAttackComponents(__instance.gameObject);
        }
    }
}