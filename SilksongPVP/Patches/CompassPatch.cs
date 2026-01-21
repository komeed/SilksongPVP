using HarmonyLib;
using UnityEngine;

namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(GameMap), "Awake")]
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
            DamagePatch.PrintComponents(compassIcon.transform);
            //DamagePatch.PrintChildrenRecursive(__instance.transform);
        }
    }
    
    [HarmonyPatch(typeof(GameMap), "CloseQuickMap")]
    public static class CloseQuickMapPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameMap __instance)
        {
            SilksongModPlugin.Log.LogInfo("closing quick map!");
            LobbyManager.showingQuickMap = false;
        }
    }
    
    [HarmonyPatch(typeof(InventoryMapManager), "ZoomIn")]
    public static class InventoryMapManagerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InventoryMapManager __instance)
        {
            SilksongModPlugin.Log.LogInfo("zooming in!");
            LobbyManager.showingFullMap = true;
            LobbyManager.showingQuickMap = false;
        }
    }

    [HarmonyPatch(typeof(InventoryMapManager), "ZoomOut")]
    public static class InventoryMapManagerZoomOutPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InventoryMapManager __instance)
        {
            SilksongModPlugin.Log.LogInfo("zooming out!");
            LobbyManager.showingFullMap = false;
            LobbyManager.showingQuickMap = false;
        }
    }
    
    [HarmonyPatch(typeof(GameMap), "TryOpenQuickMap")]
    public static class TryOpenQuickMapPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InventoryMapManager __instance)
        {
            SilksongModPlugin.Log.LogInfo("showing quick map!");
            LobbyManager.showingQuickMap = true;
            LobbyManager.showingFullMap = false;
        }
    }
    [HarmonyPatch(typeof(InventoryWideMap), "UpdatePositions")]
    public static class InventoryWideMapPatch
    {
        [HarmonyPrefix]
        public static void Prefix(InventoryWideMap __instance)
        {
            LobbyManager.inventoryWideMap = __instance;
        }
    }
}