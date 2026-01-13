using HarmonyLib;

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
           // }
        }
    }
}