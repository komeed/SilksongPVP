using HarmonyLib;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;

namespace SilksongMod.Patches
{
    [HarmonyPatch(typeof(NailAttackBase), "OnSlashStarting")] 
    public class AttackPatch
    {
        //currently doesn't work for great slash!
        [HarmonyPrefix]
        public static void Prefix(NailAttackBase __instance) // function called upon entering
        {
            if (!LobbyManager.NABListIndex.IsNullOrEmpty())
            {
                if (LobbyManager.NABListIndex.TryGetValue(__instance, out int index))
                {
                    SilksongModPlugin.Log.LogInfo(
                        $"StartSlash called! instance name: {__instance.gameObject.name}, parent name: {__instance.gameObject.transform.parent.name}");
                    //send the index over to steam p2p to every person in lobby
                    byte[] data = new byte[4];
                    data[0] = (byte)AnimType.NailAttack;
                    data[1] = (byte)NailAttackType.NailAttackEnable;
                    data[2] = (byte)index;
                    data[3] = 1; // 1 is turn on, 0 is turn off
                    LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(NailAttackBase), "OnCancelAttack")] 
    public class CancelAttackPatch
    {
        //currently doesn't work for great slash!
        [HarmonyPrefix]
        public static void Prefix(NailAttackBase __instance) // function called upon entering
        {
            if (!LobbyManager.NABListIndex.IsNullOrEmpty())
            {
                if (LobbyManager.NABListIndex.TryGetValue(__instance, out int index))
                {
                    SilksongModPlugin.Log.LogInfo(
                        $"StartSlash called! instance name: {__instance.gameObject.name}, parent name: {__instance.gameObject.transform.parent.name}");
                    //send the index over to steam p2p to every person in lobby
                    byte[] data = new byte[4];
                    data[0] = (byte)AnimType.NailAttack;
                    data[1] = (byte)NailAttackType.NailAttackEnable;
                    data[2] = (byte)index;
                    data[3] = 1; // 1 is turn on, 0 is turn off
                    LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
                }
            }
        }
    }
}