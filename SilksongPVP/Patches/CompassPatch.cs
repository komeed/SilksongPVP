using HarmonyLib;
using UnityEngine;

namespace SilksongMod.Patches
{
    /*[HarmonyPatch(typeof(GameMap), "Awake")]
    public static class CompassPatch
    {
        [HarmonyPrefix]
        public static void Prefix(GameMap __instance)
        {
            LobbyManager.gameMap = __instance;
            GameObject compassIcon = __instance.transform.Find("Compass Icon").gameObject;
            if (compassIcon == null)
            {
                SilksongModPlugin.Log.LogInfo("COULDN'T FIND COMPASS ICON! this is bad.");
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("Found Compass Icon!");
                LobbyManager.SetCompassIcon(compassIcon);
            }
            //DamagePatch.PrintChildrenRecursive(__instance.transform);
        }
    }
    [HarmonyPatch(typeof(InventoryWideMap), "UpdatePositions")]
    public static class InventoryWideMapPatch
    {
        [HarmonyPrefix]
        public static void Prefix(InventoryWideMap __instance)
        {
            LobbyManager.inventoryWideMap = __instance;
            SilksongModPlugin.Log.LogInfo("set inventorywidemap!");
        }
    }*/
}