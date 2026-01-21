using GlobalEnums;

namespace SilksongMod.Patches
{
    using HarmonyLib;
    using System;
    using System.Reflection;
    using UnityEngine;

    public class PrivateCaller
    {
        private delegate void GetSceneInfoDelegate(
            GameMap instance, 
            string sceneName,
            MapZone mapZone,
            out GameMapScene foundScene,
            out GameObject foundSceneObj,
            out Vector2 foundScenePos
        );
        
        private delegate MapZone GetMapZoneFromSceneNameDelegate(GameMap instance, string sceneName);
        private delegate Vector2 GetMapPositionDelegate(GameMap instance, Vector2 positionInScene, GameMapScene scene, GameObject sceneObj, Vector2 scenePos, Vector2 sceneSize);
        private delegate void PositionIconDelegate(InventoryWideMap instance, Transform icon, Vector2 mapBoundsPos, bool isActive, MapZone currentMapZone);

        private GetSceneInfoDelegate cachedDelegate;
        private GetMapZoneFromSceneNameDelegate getMapZoneDelegate;
        private GetMapPositionDelegate getMapPosDelegate;
        private PositionIconDelegate posIconDelegate;

        public PrivateCaller()
        {
            try
            {
                cachedDelegate = CreateDelegate<GetSceneInfoDelegate>(
                    typeof(GameMap),
                    "GetSceneInfo"
                );

                getMapZoneDelegate = CreateDelegate<GetMapZoneFromSceneNameDelegate>(
                    typeof(GameMap),
                    "GetMapZoneFromSceneName"
                );

                getMapPosDelegate = CreateDelegate<GetMapPositionDelegate>(
                    typeof(GameMap),
                    "GetMapPosition"
                );

                posIconDelegate = CreateDelegate<PositionIconDelegate>(
                    typeof(InventoryWideMap),
                    "PositionIcon"
                );
            }
            catch (Exception e)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"[PrivateCaller] Failed to initialize private method delegates:\n{e}"
                );
                throw; // fail hard after logging
            }
        }
        
        private static T CreateDelegate<T>(Type type, string methodName) where T : Delegate
        {
            MethodInfo mi = AccessTools.Method(type, methodName);

            if (mi == null)
            {
                throw new MissingMethodException(
                    $"{type.FullName}.{methodName} was not found (private method missing or renamed)"
                );
            }

            try
            {
                return (T)Delegate.CreateDelegate(typeof(T), null, mi);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Failed to bind delegate for {type.FullName}.{methodName}. " +
                    $"Signature mismatch or method changed.",
                    e
                );
            }
        }

        public void CallGetSceneInfo(GameMap instance, string sceneName, MapZone mapZone,
            out GameMapScene scene, out GameObject sceneObj, out Vector2 scenePos)
        {
            // Call via delegate
            cachedDelegate(instance, sceneName, mapZone, out scene, out sceneObj, out scenePos);
        }
        
        public MapZone CallGetMapZone(GameMap instance, string sceneName)
        {
            // Call via delegate
            return getMapZoneDelegate(instance, sceneName);
        }
        public Vector2 CallGetMapPosition(GameMap instance, Vector2 positionInScene, GameMapScene scene, GameObject sceneObj, Vector2 scenePos, Vector2 sceneSize)
        {
            // Call via delegate
            return getMapPosDelegate(instance, positionInScene, scene, sceneObj, scenePos, sceneSize);
        }

        public void CallPositionIcon(InventoryWideMap instance, Transform icon, Vector2 mapBoundsPos, bool isActive,
            MapZone currentMapZone)
        {
            posIconDelegate(instance, icon, mapBoundsPos, isActive, currentMapZone);
        }
    }

}