using GlobalEnums;
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
            //SilksongModPlugin.Log.LogInfo("I lost a bunch of health here");
            LobbyManager.HitEnemy = true; // verifies that the hit is registered (from the 
        }
    }
    /*
    //remove the freeze frame and reset the invultime
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("StartRecoil", new[] { typeof(CollisionSide), typeof(int) })]
    public static class StartRecoilPrePatch
    {
        [HarmonyPrefix]
        public static void Prefix(HeroController __instance, CollisionSide impactSide, int damageAmount)
        {
            if (LobbyManager.HitEnemy)
            {
                SilksongModPlugin.Log.LogInfo("recoiling from syncedhornet attack! ensuring zero freeze frame.");
                __instance.INVUL_TIME /= 2;
            }
            else
            {
                LobbyManager.HitEnemy = false;
            }
            SilksongModPlugin.Log.LogInfo("I lost a bunch of health here");
            LobbyManager.HitEnemy = true;
        }
    }
    */
    /*[HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("StartRecoil", new[] { typeof(CollisionSide), typeof(int) })]
    public static class StartRecoilPostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(HeroController __instance, CollisionSide impactSide, int damageAmount)
        {
            if (LobbyManager.HitEnemy)
            {
                SilksongModPlugin.Log.LogInfo("end of recoiling from syncedhornet attack. setting back ");
            }
            else
            {
                LobbyManager.HitEnemy = false;
            }
        }
    }*/
}