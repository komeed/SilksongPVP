using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SilksongMod
{ // if host button pressed, enable host
    [HarmonyPatch(typeof(GameManager), "ReadyForRespawn")] 
    public static class OnEnterWorld {
        [HarmonyPostfix]
        public static void Postfix(GameManager __instance, bool isFirstLevelForPlayer) // function called upon entering
        {
            SilksongModPlugin.Log.LogInfo("Player Respawned");
            if (isFirstLevelForPlayer)
            {
                SilksongModPlugin.Log.LogInfo("PRINTING GAME DATA ON GAME LOAD");
               // GameObject Hornet = GameObject.Find("Hero_Hornet(Clone)");
                //LogHierarchyAndComponents(Hornet);
                //SyncedHornetScript.CreateHornet();
            }
        }
        
        private static void LogAllGameObjectsOneShot()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            StringBuilder sb = new StringBuilder(8192);

            sb.AppendLine("===== GAMEOBJECT DUMP =====");
            sb.AppendLine();

            foreach (var obj in allObjects)
            {
                // Skip editor-only / hidden objects
                if (obj.hideFlags != HideFlags.None)
                    continue;

                string state = obj.activeInHierarchy ? "ACTIVE" : "INACTIVE";
                string selfState = obj.activeSelf ? "Self: ON" : "Self: OFF";

                sb.AppendLine(
                    $"[{state}] {obj.name} | {selfState}"
                );
            }

            sb.AppendLine();
            sb.AppendLine("===== END DUMP =====");

            SilksongModPlugin.Log.LogInfo(sb.ToString());
        }
        private static void LogHierarchyAndComponents(GameObject root)
        {
            if (root == null)
            {
                SilksongModPlugin.Log.LogInfo("GameObjectInspector: root is null");
                return;
            }

            StringBuilder sb = new StringBuilder(4096);

            sb.AppendLine("===== GAMEOBJECT INSPECT =====");
            sb.AppendLine($"Root: {root.name}");
            sb.AppendLine($"Parent: {root.transform.parent?.name}");
            sb.AppendLine();

            // Components on THIS object only
            sb.AppendLine("Components on root:");
            foreach (var comp in root.GetComponents<Component>())
            {
                if (comp == null)
                    sb.AppendLine(" - <Missing Script>");
                else
                    sb.AppendLine($" - {comp.GetType().Name}");
            }

            sb.AppendLine();
            sb.AppendLine("Children:");
            AppendChildrenRecursive(root.transform, sb, 1);

            sb.AppendLine();
            sb.AppendLine("===== END INSPECT =====");

            SilksongModPlugin.Log.LogInfo(sb.ToString());
        }

        private static void AppendChildrenRecursive(Transform parent, StringBuilder sb, int depth)
        {
            foreach (Transform child in parent)
            {
                sb.AppendLine($"{new string(' ', depth * 2)}- {child.name}");
                AppendChildrenRecursive(child, sb, depth + 1);
            }
        }
    }
}