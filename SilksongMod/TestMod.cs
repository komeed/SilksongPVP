using System;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilksongMod.SteamP2P;
using UnityEngine;
using Object = UnityEngine.Object;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//"MainMenuButtons" is the parent for the menubuttons

namespace SilksongMod
{
    // Main plugin class BepInEx detects
    [BepInPlugin("com.yourname.silksongmod", "Silksong Mod", "1.0.0")]
    public class SilksongModPlugin : BaseUnityPlugin
    {
        
        private float timer = 0f;

        private const float interval = 0.1f;
        // Static instance to allow static patches to access Logger
        public static SilksongModPlugin Instance { get; private set; }

        // Public accessor for the protected Logger
        public static ManualLogSource Log => Instance.Logger;

        private bool canvasFound;

        public static Canvas canvas;
        
        private static SteamP2PReceiver reciever;
        
        public static LobbyManager LobbyManager;
        
        private void Awake()
        {
            canvasFound = false;
            Instance = this;
            // Log that the plugin loaded
            Logger.LogInfo("SilksongMod loaded!");
            // Patch all Harmony patches in this assembly
            var harmony = new Harmony("com.yourname.silksongmod");
            harmony.PatchAll();
            //NetworkManager.PrintAllGameObjects();
            reciever = gameObject.AddComponent<SteamP2PReceiver>();
        }

        private void Start()
        {
            SilksongModPlugin.Log.LogInfo("SilksongModPlugin Start Called!");
            LobbyManager = CreateFullScreenCanvas();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LobbyManager.CreateJoin(LobbyManager.CurrPlayer);
            }
        }

        public LobbyManager CreateFullScreenCanvas()
        {
            var go = new GameObject("BepinexCanvas", typeof(RectTransform));
            DontDestroyOnLoad(go);
            go.transform.SetParent(null);
            // RectTransform setup
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Canvas
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Scaler
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Input
            go.AddComponent<GraphicRaycaster>();
            
            LobbyManager lobby = go.AddComponent<LobbyManager>(); // add lobby manager

            return lobby;
        }

        public void OnDestroy()
        {
            Log.LogInfo("SilksongModPlugin OnDestroy Called!");
        }
    }
}
