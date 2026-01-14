using HarmonyLib;
using SilksongMod.SteamP2P;

namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(NailAttackBase), "OnSlashStarting")] 
    public class AttackPatch
    {
        //currently doesn't work for great slash!
        [HarmonyPostfix]
        public static void Postfix(NailAttackBase __instance) // function called upon entering
        {
            if (!LobbyManager.NABListIndex.IsNullOrEmpty())
            {
                if (LobbyManager.NABListIndex.TryGetValue(__instance, out int index))
                {
                    SilksongModPlugin.Log.LogInfo(
                        $"StartSlash called! instance name: {__instance.gameObject.name}, parent name: {__instance.gameObject.transform.parent.name}");
                    //send the index over to steam p2p to every person in lobby
                    byte[] data = new byte[2];
                    data[0] = (byte)RPCMethod.NailAttack;
                    data[1] = (byte)index;
                    LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
                }
            }
        }
    }
}