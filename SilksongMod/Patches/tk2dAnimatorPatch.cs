using HarmonyLib;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;

// if this is the real hornet, use this to send the animation directions to fake hornet
namespace SilksongMod.tk2dAnimatorPatch
{
    [HarmonyPatch(typeof(tk2dSpriteAnimator))]
    [HarmonyPatch("Play", new[] { typeof(tk2dSpriteAnimationClip), typeof(float), typeof(float) })]
    public class AnimatorPlayPatch
    {   
        [HarmonyPrefix]
        public static void Prefix(tk2dSpriteAnimator __instance, tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps)
        {
            if (LobbyManager.Players.Count > 1 && LobbyManager.HostHornet == __instance.gameObject)
            {
                // if the current lobby contains more than just you
                string name = clip.name; // serialize by name
                byte[] data = Serializer.SerializePlay(AnimType.Hornet, name, clipStartTime, overrideFps);
                LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
            }
            else if (!LobbyManager.NABListIndex.IsNullOrEmpty())
            {
                if (__instance.gameObject.tag == "Nail Attack") // this works pretty well actually
                {
                    if (__instance.gameObject.TryGetComponent(out NailAttackBase component))
                    {
                        if (LobbyManager.NABListIndex.TryGetValue(component, out int index))
                        {
                            byte[] data = Serializer.SerializeNailPlay(index, clip.name, clipStartTime, overrideFps);
                            SilksongModPlugin.Log.LogInfo($"Sending Nail Attack to lobby! name: {__instance.gameObject.name}");
                            LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
                        }
                        else
                        {
                            SilksongModPlugin.Log.LogError($"Play: NABLIST DOESN'T HAVE component for name: {__instance.gameObject.name}. Something wrong with inheritance here.");
                        }
                    }
                    else
                    {
                        SilksongModPlugin.Log.LogInfo($"Play: this nail attack called name: {__instance.gameObject.name}" +
                                                      $" doesn't have a NailAttackBase component!");
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(tk2dSpriteAnimator), "Stop")]
    public class AnimatorStopPatch
    {
        [HarmonyPrefix]
        public static void Prefix(tk2dSpriteAnimator __instance)
        {
            if (LobbyManager.Players.Count > 1 && LobbyManager.HostHornet == __instance.gameObject)
            {
                byte[] data = Serializer.SerializeStop(AnimType.Hornet);
                LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
            }
            else if (!LobbyManager.NABListIndex.IsNullOrEmpty())
            {
                if (__instance.gameObject.tag == "Nail Attack") // this works pretty well actually
                {
                    if (__instance.gameObject.TryGetComponent(out NailAttackBase component))
                    {
                        if (LobbyManager.NABListIndex.TryGetValue(component, out int index))
                        {
                            byte[] data = Serializer.SerializeNailStop(AnimType.NailAttack, index);
                            LobbyManager.SendDataToLobby(data, P2PChannel.Anim);
                        }
                        else
                        {
                            SilksongModPlugin.Log.LogError($"Stop: NABLIST DOESN'T HAVE component for name: {__instance.gameObject.name}. Something wrong with inheritance here.");
                        }
                    }
                    else
                    {
                        SilksongModPlugin.Log.LogInfo($"Stop: this nail attack called name: {__instance.gameObject.name}" +
                                                      $" doesn't have a NailAttackBase component!");
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(tk2dSpriteAnimator), "StopAndResetFrame")]
    public class AnimatorStopAndResetFramePatch
    {   
        [HarmonyPrefix]
        public static void Prefix(tk2dSpriteAnimator __instance)
        {
            // send stop command
        }
    }

    [HarmonyPatch(typeof(tk2dSpriteAnimator), "Pause")]
    public class AnimatorPausePatch
    {
        [HarmonyPrefix]
        public static void Prefix(tk2dSpriteAnimator __instance)
        {
            // send pause command
        }
    }
    
    [HarmonyPatch(typeof(tk2dSpriteAnimator), "Resume")]
    public class AnimatorResumePatch
    {
        [HarmonyPrefix]
        public static void Prefix(tk2dSpriteAnimator __instance)
        {
            // send resume command
        }
    }
    
    [HarmonyPatch(typeof(tk2dSpriteAnimator))]
    [HarmonyPatch("SetFrame", new[] { typeof(int), typeof(bool) })]
    public class AnimatorSetFramePatch
    {
        [HarmonyPrefix]
        public static void Prefix(tk2dSpriteAnimator __instance)
        {
            // send set frame with int and bool paremeter
        }
    }
    
}