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
            SilksongModPlugin.Log.LogInfo($"TriggerEnter2D: hit some object! idk what. Curr Object: trigger: {__instance.gameObject.GetComponent<Collider2D>().isTrigger}");
            PhysLayers layer = (PhysLayers)((Component)(object)collision).gameObject.layer;
            SilksongModPlugin.Log.LogInfo($"Layer of object: {layer}");
            if (collision.gameObject.TryGetComponent<SyncedHornetScript>(out var script))
            {
                SilksongModPlugin.Log.LogInfo("hit synced hornet! sending hit");
                script.TakeDamage();
                //return false;
            }

            return true;
        }
    }
   /* 
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("TakeDamage", new[] { typeof(GameObject), typeof(CollisionSide),  })]
    public class CollisionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(HeroController __instance, GameObject go, CollisionSide damageSide, int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None) 
        {
            SilksongModPlugin.Log.LogInfo($"CollisionEnter2D: hit some object! idk what. Curr Object: trigger: {__instance.gameObject.GetComponent<Collider2D>().isTrigger}");
            if (collision.gameObject.TryGetComponent<SyncedHornetScript>(out var script))
            {
                SilksongModPlugin.Log.LogInfo("hit synced hornet! sending hit");
                script.TakeDamage();
            }
        }
    }*/
}