using HarmonyLib;

namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(GameManager), "OnNextLevelReady")] 
    public class OnNextLevelReadyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameManager __instance)
        {
            if (__instance.IsGameplayScene())
            {
                string scene = __instance.sceneName;
                SilksongModPlugin.Log.LogInfo("LOADING SCENE: " + scene);
                LobbyManager.UpdateCurrSceneAndSend(scene);
            }
        }
    }
}