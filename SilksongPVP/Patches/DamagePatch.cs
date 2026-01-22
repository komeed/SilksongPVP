using GlobalEnums;
using HarmonyLib;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;
using UnityEngine;

namespace SilksongMod.Patches
{

    [HarmonyPatch(typeof(DamageEnemies))]
    [HarmonyPatch("OnTriggerEnter2D", new[] { typeof(Collider2D) })]
    public class DamagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(DamageEnemies __instance, Collider2D collision) 
        {
           //PrintFullHierarchyInfo(__instance.gameObject);
          // SilksongModPlugin.Log.LogInfo("path of " + __instance.name + ": " + GetFullPath(__instance.transform) + "tag:  " + __instance.tag);
          // DamagePatch.PrintComponents(__instance.transform);
          // SilksongModPlugin.Log.LogInfo("attack type: " + __instance.attackType);
          //  SilksongModPlugin.Log.LogInfo($"TriggerEnter2D: hit some object! idk what. Curr Object: trigger: {__instance.gameObject.GetComponent<Collider2D>().isTrigger}");
           // PhysLayers layer = (PhysLayers)((Component)(object)collision).gameObject.layer;
            //SilksongModPlugin.Log.LogInfo($"Layer of object: {layer}");
            // only do it if its not a tool (are silk skills tools?)
            if (collision.gameObject.TryGetComponent<SyncedHornetScript>(out var script))
            {
                if (__instance.attackType == AttackTypes.Nail)
                {
                    SilksongModPlugin.Log.LogInfo("hit synced hornet with nail! sending hit");
                    script.TakeDamage(Attack.Nail);
                }
                else if (__instance.RepresentingTool) // if spell (hopefuly this works, if not use below)
                {
                    if (__instance.RepresentingTool.Type == ToolItemType.Skill)
                    {
                        SilksongModPlugin.Log.LogInfo("hit synced hornet with spell! sending hit");
                        script.TakeDamage(Attack.Spell);
                    }
                    else
                    {
                        SilksongModPlugin.Log.LogInfo("red/white/blue tool: not sending (too broken)");
                    }
                }
            }

            return true;
        }
        
        public static void PrintFullHierarchyInfo(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("GameObject is null!");
            return;
        }

        // Build full path of the original object
        string fullPath = GetFullPath(obj.transform);
       // SilksongModPlugin.Log.LogInfo("Full Path: " + fullPath);

        // Loop through the object and all its parents
        Transform current = obj.transform;
        while (current != null)
        {
            PrintComponents(current);

            // Print all children recursively (but skip if the root is Hero_Hornet(Clone))
            if (current.name != "Hero_Hornet(Clone)" && current.name != "Special Attacks")
            {
                foreach (Transform child in current)
                {
                    PrintChildrenRecursive(child);
                }
            }

            current = current.parent;
        }
    }

    // Print all components on a single GameObject
    public static void PrintComponents(Transform t)
    {
        Component[] components = t.GetComponents<Component>();
        string path = GetFullPath(t);
        SilksongModPlugin.Log.LogInfo("Components on " + path + ":");
        if (components.Length == 0)
        {
            SilksongModPlugin.Log.LogInfo(" - None");
        }
        else
        {
            foreach (Component c in components)
            {
                SilksongModPlugin.Log.LogInfo(" - " + c.GetType().Name);
            }
        }
    }

    // Recursively print all children and their components
    public static void PrintChildrenRecursive(Transform parent)
    {
        PrintComponents(parent);

        foreach (Transform child in parent)
        {
            PrintChildrenRecursive(child);
        }
    }

    // Helper to get the full path of a Transform
    public static string GetFullPath(Transform t)
    {
        string path = t.name;
        Transform current = t.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
    }
    /*
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("GetWillThrowTool", new[] { typeof(bool) })]
    public class CollisionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(HeroController __instance, bool reportFailure) 
        {
            AttackToolBinding usedBinding;
            ToolItem item = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Neutral, ToolEquippedReadSource.Active, out usedBinding);
            SilksongModPlugin.Log.LogInfo($"Silk skill: {item.Description}, {item.DisplayName}, FSM event name: {item.Usage.FsmEventName}");
            GameObject throwPrefab = item.Usage.ThrowPrefab;
            if (throwPrefab != null)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"Throw prefab name: {throwPrefab.name}, path: {DamagePatch.GetFullPath(throwPrefab.transform)}");

            }
            else {
                SilksongModPlugin.Log.LogInfo("no throw prefab!");
            }
        }
    }*/
    /*
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("ThrowTool", new[] { typeof(bool) })]
    public class ThrowToolPatch
    {
        [HarmonyPrefix]
        public static void Prefix(HeroController __instance, bool isAutoThrow) 
        {
            AttackToolBinding usedBinding;
            ToolItem item = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Neutral, ToolEquippedReadSource.Active, out usedBinding);
            if(__instance.CanThrowTool(item, ))
            SilksongModPlugin.Log.LogInfo($"Silk skill: {item.Description}, {item.DisplayName}, FSM event name: {item.Usage.FsmEventName}");
            GameObject throwPrefab = item.Usage.ThrowPrefab;
            if (throwPrefab != null)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"Throw prefab name: {throwPrefab.name}, path: {DamagePatch.GetFullPath(throwPrefab.transform)}");

            }
            else {
                SilksongModPlugin.Log.LogInfo("no throw prefab!");
            }
        }
        }*/
}