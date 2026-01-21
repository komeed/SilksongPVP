using System;
using System.Collections;
using System.IO;
using System.Net;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SilksongMod.SteamP2P;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.Networking;

//"MainMenuButtons" is the parent for the menubuttons

namespace SilksongMod
{
    // Main plugin class BepInEx detects
    [BepInPlugin("com.yourname.silksongpvp", "Silksong PVP", "1.1")]
    public class SilksongModPlugin : BaseUnityPlugin
    {
        private const string VersionURL =
            "https://drive.google.com/uc?export=download&id=18qhf9-JbUj807DnAxDHr5i4n_m701lLq";

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
        private bool showWarning;
        private Harmony harmony;

        private void Awake()
        {
            canvasFound = false;
            Instance = this;
            // Log that the plugin loaded
            Logger.LogInfo("SilksongPVP loaded!");
            // Patch all Harmony patches in this assembly
            harmony = new Harmony("com.yourname.silksongpvp");
            harmony.PatchAll();
            //NetworkManager.PrintAllGameObjects();
            reciever = gameObject.AddComponent<SteamP2PReceiver>();
            CreateFullScreenCanvas();
            CheckVersion();
        }

        private void Start()
        {
            SilksongModPlugin.Log.LogInfo("SilksongModPlugin Start Called!");
            LobbyManager = canvas.gameObject.AddComponent<LobbyManager>();
        }

        public void CreateFullScreenCanvas()
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
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Scaler
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Input
            go.AddComponent<GraphicRaycaster>();

            //LobbyManager lobby = go.AddComponent<LobbyManager>(); // add lobby manager

            //return lobby;
        }

        public void OnDestroy()
        {
            Log.LogInfo("SilksongModPlugin OnDestroy Called!");
        }

        private void CheckVersion()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(VersionURL);
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string content = reader.ReadToEnd();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Logger.LogWarning("Version file is empty or could not be read.");
                                return;
                            }

                            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            bool allowed = false;
                            foreach (var line in lines)
                            {
                                if (Version.TryParse(line.Trim(), out var v))
                                {
                                    if (v == Info.Metadata.Version)
                                    {
                                        allowed = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    Logger.LogWarning($"Invalid version format in list: {line}");
                                }
                            }

                            if (!allowed)
                            {
                                Logger.LogError($"This mod version {Info.Metadata.Version} is not allowed!");
                                ShowVersionWarning($"SilksongPVP version {Info.Metadata.Version} is not allowed! Please update SilksongPVP!");
                                harmony.UnpatchSelf();
                                enabled = false;
                            }
                            else
                            {
                                Logger.LogInfo($"Mod version {Info.Metadata.Version} is allowed.");
                            }
                        }
                    }
                    catch (Exception readEx)
                    {
                        Logger.LogWarning($"Failed to read version file: {readEx.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Failed to fetch version file: {e.Message}");
            }
        }
        
        private void ShowVersionWarning(string message)
    {
        // --- Panel to hold UI elements (optional, for layout) ---
        GameObject panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvas.transform, false);
        RectTransform panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 0);
        panelRT.anchorMax = new Vector2(1, 1);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // --- Red Text ---
        GameObject textGO = new GameObject("WarningText");
        textGO.transform.SetParent(panelGO.transform, false);
        Text uiText = textGO.AddComponent<Text>();
        uiText.text = message;
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = 40;
        uiText.color = Color.red;
        uiText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRT = uiText.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.1f, 0.6f);
        textRT.anchorMax = new Vector2(0.9f, 0.8f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // --- Close Button ---
        GameObject buttonGO = new GameObject("CloseButton");
        buttonGO.transform.SetParent(panelGO.transform, false);
        Button btn = buttonGO.AddComponent<Button>();
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = Color.gray;

        RectTransform btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.4f, 0.4f);
        btnRT.anchorMax = new Vector2(0.6f, 0.5f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        // Button Text
        GameObject btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "Close";
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        btnText.fontSize = 24;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;

        RectTransform btnTextRT = btnText.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        // Close button logic
        btn.onClick.AddListener(() =>
        {
            Destroy(canvas);
        });
    }
    }
}
