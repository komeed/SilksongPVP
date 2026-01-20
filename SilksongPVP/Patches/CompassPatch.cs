using HarmonyLib;
namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(GameMap), "GetCompassPositionLocalBounds")]
    public static class CompassPatch
    {
        [HarmonyPrefix]
        public static void Prefix(GameMap __instance)
        {
            if (!LobbyManager.foundTraverseMethod)
            {
                Traverse method = Traverse.Create(__instance).Method("PrivateInternalUpdate");
                if (method.MethodExists())
                {
                    
                }
            }
        }
    }
}