using HarmonyLib;
using UnityEngine;

namespace SilksongMod
{
    [HarmonyPatch(typeof(HeroController), "Start")] 
    public static class OnHornetSpawn
    {
        [HarmonyPostfix]
        public static void Postfix(HeroController __instance)
        {
           // if (GlobalHost.HostEnabled)
           // {
                SilksongModPlugin.Log.LogInfo("Hornet Spawned!");
                LobbyManager.SetHostHornet(__instance.gameObject);
                LobbyManager.ActivateHornets(true);
                PrintChildrenAndComponents(__instance.gameObject);
           // }
        }
        
        public static void PrintChildrenAndComponents(GameObject parent)
        {
            if (parent == null)
            {
                SilksongModPlugin.Log.LogInfo("Parent GameObject is null.");
                return;
            }

            PrintChildRecursive(parent, 0);
        }

        private static void PrintChildRecursive(GameObject obj, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2); // indentation for hierarchy
            SilksongModPlugin.Log.LogInfo($"{indent}GameObject: {obj.name}, Tag: {obj.tag}");

            // Print all components on this object
            Component[] components = obj.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp != null) // sometimes null if component missing
                    SilksongModPlugin.Log.LogInfo($"{indent}  Component: {comp.GetType().Name}");
            }

            // Recursively print children
            foreach (Transform child in obj.transform)
            {
                PrintChildRecursive(child.gameObject, indentLevel + 1);
            }
        }
    }
}