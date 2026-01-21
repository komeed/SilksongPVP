using UnityEngine;
using UnityEngine.UI;

namespace SilksongMod
{
    public static class LobbySwitch
    {
        private static Canvas canvas;
        private static Vector2 position = new Vector2(0, 0); // position on canvas
        private static Vector2 size = new Vector2(100, 40); // size of toggle

        private static GameObject toggleGO;
        private static GameObject textGO;

        public static void CreateSwitchWithLabel(Canvas canvas)
        {
            float offsetX = 10;
            float offsetY = 10;

            // --- Text Label ---
            textGO = new GameObject("SwitchLabel");
            textGO.transform.SetParent(canvas.transform, false);
            textGO.SetActive(false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            Text label = textGO.AddComponent<Text>();
            label.text = "Enable Global PVP Lobby?";
            label.font = LobbyManager.DefaultFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 20;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleLeft;

            textRT.pivot = new Vector2(0.5f, 0);
            textRT.anchorMin = new Vector2(0.5f, 0);
            textRT.anchorMax = new Vector2(0.5f, 0);
            textRT.anchoredPosition = new Vector2(offsetX, offsetY);
            textRT.sizeDelta = new Vector2(240, 40);

            // --- Toggle ---
            toggleGO = new GameObject("SwitchToggle");
            toggleGO.transform.SetParent(canvas.transform, false);
            toggleGO.SetActive(false);

            RectTransform toggleRT = toggleGO.AddComponent<RectTransform>();
            toggleRT.sizeDelta = new Vector2(100, 40);
            toggleRT.pivot = new Vector2(0.5f, 0);
            toggleRT.anchorMin = new Vector2(0.5f, 0);
            toggleRT.anchorMax = new Vector2(0.5f, 0);
            toggleRT.anchoredPosition = new Vector2(offsetX + textRT.sizeDelta.x + 10, offsetY);

            Toggle toggle = toggleGO.AddComponent<Toggle>();

            // Background
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(toggleGO.transform, false);
            RectTransform bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.gray;
            toggle.targetGraphic = bgImage;

            // Handle
            GameObject handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(bgGO.transform, false);
            RectTransform handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(32, 32);
            handleRT.anchoredPosition = new Vector2(-50 / 4, 0);
            Image handleImage = handleGO.AddComponent<Image>();
            handleImage.color = Color.white;

            toggle.isOn = false;
            toggle.onValueChanged.AddListener((isOn) =>
            {
                bool action = OnToggleChanged(isOn);
                if (action) // if action is valid, do the correct switching
                {
                    handleRT.anchoredPosition = new Vector2(isOn ? 50 / 4 : -50 / 4, 0);
                    bgImage.color = isOn ? Color.green : Color.gray;
                }
                else
                {
                    toggle.isOn = !isOn; // if action is invalid, go back to previous
                }
            });
        }

        private static bool OnToggleChanged(bool isOn)
        {
            if (isOn)
            {
                if (!LobbyManager.LobbyPlayers.IsNullOrEmpty()) // temporary
                {
                    SilksongModPlugin.Log.LogInfo("Error! need to have empty lobby!");
                    //InviteButtonScript.CreateFriendLayout(canvas.gameObject);
                    InviteButtonScript.CreateErrorLayout(textGO.transform.parent.gameObject, "Please leave your current lobby first!");
                    return false;
                }
            }
            SilksongModPlugin.Log.LogInfo("changing toggle!");
            LobbyManager.isGlobalLobby = isOn;
            return true;
        }
        
        public static void SetActive(bool active)
        {
            toggleGO.SetActive(active);
            //toggleGO.GetComponent<Toggle>().isOn = false;
            textGO.SetActive(active);
        }

        public static void SetToggleOff()
        {
            toggleGO.GetComponent<Toggle>().isOn = false;
        }
    }
}