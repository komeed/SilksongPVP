using HarmonyLib;

namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("UnloadScene", new[] { typeof(string), typeof(SceneLoad)})]
    public class OnSceneLoad
    {
        [HarmonyPrefix]
        public static void Prefix(GameManager __instance, string unloadingSceneName, SceneLoad unloadingSceneLoad)
        {
            
        }
    }
}