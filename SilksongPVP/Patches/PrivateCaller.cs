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
            object instance, 
            string sceneName,
            MapZone mapZone,
            out GameMapScene foundScene,
            out GameObject foundSceneObj,
            out Vector2 foundScenePos
        );
        
        private delegate MapZone GetMapZoneFromSceneNameDelegate(object instance, string sceneName);
        private delegate Vector2 GetMapPositionDelegate(object instance, Vector2 positionInScene, GameMapScene scene, GameObject sceneObj, Vector2 scenePos, Vector2 sceneSize);
        private delegate void PositionIconDelegate(object instance, Transform icon, Vector2 mapBoundsPos, bool isActive, MapZone currentMapZone);

        private GetSceneInfoDelegate cachedDelegate;
        private GetMapZoneFromSceneNameDelegate getMapZoneDelegate;
        private GetMapPositionDelegate getMapPosDelegate;
        private PositionIconDelegate posIconDelegate;

        public PrivateCaller()
        {
            // Grab the MethodInfo
            MethodInfo mi = AccessTools.Method(typeof(GameMap), "GetSceneInfo");

            // Create an open instance delegate
            cachedDelegate = (GetSceneInfoDelegate)Delegate.CreateDelegate(
                typeof(GetSceneInfoDelegate), 
                null, // null because open instance delegate
                mi
            );
            
            // New GetMapZoneFromSceneName delegate setup
            MethodInfo mi2 = AccessTools.Method(typeof(GameMap), "GetMapZoneFromSceneName");
            getMapZoneDelegate = (GetMapZoneFromSceneNameDelegate)Delegate.CreateDelegate(
                typeof(GetMapZoneFromSceneNameDelegate),
                null,
                mi2
            );
            
            MethodInfo mi3 = AccessTools.Method(typeof(GameMap), "GetMapPosition");
            getMapPosDelegate = (GetMapPositionDelegate)Delegate.CreateDelegate(
                typeof(GetMapPositionDelegate),
                null,
                mi3
            );
            
            MethodInfo mi4 = AccessTools.Method(typeof(InventoryWideMap), "PositionIcon");
            posIconDelegate = (PositionIconDelegate)Delegate.CreateDelegate(
                typeof(PositionIconDelegate),
                null,
                mi4
            );
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