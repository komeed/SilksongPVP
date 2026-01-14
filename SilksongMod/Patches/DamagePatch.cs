using GlobalEnums;
using HarmonyLib;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;
using UnityEngine;

namespace SilksongMod.Patches
{
/*
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("TakeDamage",
        new[]
        {
            typeof(GameObject), typeof(CollisionSide), typeof(int), typeof(HazardType), typeof(DamagePropertyFlags)
        })]
    public class DamagePatch
    {
        [HarmonyPrefix]
        public static void Prefix(NailAttackBase __instance, GameObject go, CollisionSide damageSide, int damageAmount,
            HazardType hazardType,
            DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None) // function called upon entering
        {
            //SilksongModPlugin.Log.LogInfo($"Player took damage! go: {go.name}, damage side: {damageSide}, damageAmount: {damageAmount}, hazardtype: {hazardType}, damagePropertyFlags: {damagePropertyFlags}");
        }
    }*/
}