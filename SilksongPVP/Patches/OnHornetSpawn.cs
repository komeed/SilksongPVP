using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SilksongMod
{
    [HarmonyPatch(typeof(HeroController), "Awake")] 
    public static class OnHornetSpawn
    {
        [HarmonyPostfix]
        public static void Postfix(HeroController __instance)
        {
            // if (GlobalHost.HostEnabled)
            // {
            SilksongModPlugin.Log.LogInfo(
                "Hornet Spawned in Awake!"); // if this doesn't work simply use search component thing
            LobbyManager.SetHostHornet(__instance.gameObject);
            // retrieve every 
            LobbyManager.StoreNailAttackComponents(__instance.gameObject);
            LobbySwitch.SetActive(false);
            if (LobbyManager.isGlobalLobby)
            {
                SilksongModPlugin.Log.LogInfo("Spawned in global lobby!");
                // first disable the lobby thing so that we don't invite/add/leave anyone
                LobbyDisplay.SetPanelActive(false);
                LobbyManager.server.JoinGlobalLobby(LobbyManager.CurrSteamID, LobbyManager.CurrName);
            }
        }
        
        public static List<ComponentObjectInfo> FindAllWithTagIncludingInactive(string tag)
        {
            var result = new List<ComponentObjectInfo>();
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.CompareTag(tag) && go.scene.IsValid())
                {
                    result.Add(new ComponentObjectInfo
                    {
                        Name = go.name,
                        Tag = go.tag,
                        ParentPath = ComponentFinder.GetParentPath(go.transform)
                    });
                }
            }
            return result;
        }
    }
    
    public struct ComponentObjectInfo
    {
        public string Name;
        public string Tag;
        public string ParentPath;

        public override string ToString()
        {
            return $"Name: {Name}, Tag: {Tag}, Path: {ParentPath}";
        }
    }
    
    public static class ComponentFinder
    {
        public static List<ComponentObjectInfo> FindObjectsWithComponent<T>(bool includeInactive = false)
            where T : MonoBehaviour
        {
            var results = new List<ComponentObjectInfo>();

            T[] components = Object.FindObjectsOfType<T>(includeInactive);

            foreach (T comp in components)
            {
                GameObject go = comp.gameObject;

                results.Add(new ComponentObjectInfo
                {
                    Name = go.name,
                    Tag = go.tag,
                    ParentPath = GetParentPath(go.transform)
                });
            }

            return results;
        }

        public static string GetParentPath(Transform t)
        {
            if (t == null)
                return string.Empty;

            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }

            return string.Join("/", stack);
        }
    }
}