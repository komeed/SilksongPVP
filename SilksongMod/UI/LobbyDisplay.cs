using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using Steamworks;

namespace SilksongMod
{
    public class LobbyDisplay
    {
        private static GameObject lobbyPanel;
        private static List<Text> notificationTexts = new List<Text>();
        private static GameObject leaveButton;
        private static GameObject parent;

        private const float TEXT_HEIGHT = 30f;
        private const float PADDING = 10f;
        
        private static Vector2 panelSize = new Vector2(300f, 500f);

        private static Font font;

        // Initialize the notification panel
        public static void Init(GameObject p, Font f)
        {
            if (lobbyPanel != null) return;

            parent = p;

            // Create the notification panel
            lobbyPanel = new GameObject("NotificationPanel");
            lobbyPanel.transform.SetParent(parent.transform, false);

            // Add RectTransform and position in top-left
            RectTransform rectTransform = lobbyPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(PADDING, -PADDING);
            rectTransform.sizeDelta = panelSize;
            font = f;
        }

        public static void UpdatePlayerList(Dictionary<CSteamID, SyncedHornetScript> players, int fontSize = 16)
        { 
            ClearAll();
            AddText("Lobby:", fontSize + 2);
            CreateLeaveButton(AddText(LobbyManager.CurrName, fontSize).gameObject, fontSize);
            foreach (KeyValuePair<CSteamID, SyncedHornetScript> player in players)
            {
                AddText(player.Value.name, fontSize);
            }
        }
        // Add a new notification text
        private static Text AddText(string message, int fontSize = 14)
        {
            if (lobbyPanel == null)
            {
                SilksongModPlugin.Log.LogError("NotificationManager not initialized! Call Initialize() first.");
                return null;
            }

            GameObject textObj = new GameObject($"Notification_{notificationTexts.Count}");
            textObj.transform.SetParent(lobbyPanel.transform, false);

            // Add Text component
            Text text = textObj.AddComponent<Text>();
            text.text = message;
            text.font = font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;

            // Position the text vertically
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = new Vector2(0, -notificationTexts.Count * TEXT_HEIGHT);
            textRect.sizeDelta = new Vector2(0, TEXT_HEIGHT);

            notificationTexts.Add(text);
            return text;
        }

        public static void CreateLeaveButton(GameObject parentText, int fontSize = 14)
        {
            GameObject buttonGO = new GameObject("LeaveButton");
            buttonGO.transform.SetParent(parentText.transform, false);

            Button button = buttonGO.AddComponent<Button>();

            // Add Image component for visuals
            Image img = buttonGO.AddComponent<Image>();
            img.color = Color.red;

            // RectTransform for positioning
            RectTransform btnRect = buttonGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 0.5f);
            btnRect.anchorMax = new Vector2(0, 0.5f);
            btnRect.pivot = new Vector2(0, 0.5f);
            btnRect.sizeDelta = new Vector2(60f, TEXT_HEIGHT - 2); // slightly smaller height
            Text pText = parentText.GetComponent<Text>();
            float textWidth = pText.cachedTextGeneratorForLayout.GetPreferredWidth(
                pText.text,
                pText.GetGenerationSettings(parentText.GetComponent<RectTransform>().rect.size)
            ) / pText.pixelsPerUnit;
            btnRect.anchoredPosition = new Vector2(textWidth + 10f, 0);

            // Add button text
            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(buttonGO.transform, false);
            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.text = "Leave";
            btnText.font = font;
            btnText.fontSize = fontSize;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;

            RectTransform txtRect = btnTextGO.GetComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0, 0);
            txtRect.anchorMax = new Vector2(1, 1);
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            // Assign click behavior
            button.onClick.AddListener(LobbyManager.LeaveButtonPressed);
        }

        // Remove a notification by index
        public static void RemoveNotification(int index)
        {
            if (index < 0 || index >= notificationTexts.Count) return;

            Object.Destroy(notificationTexts[index].gameObject);
            notificationTexts.RemoveAt(index);

            // Reposition remaining texts
            for (int i = 0; i < notificationTexts.Count; i++)
            {
                RectTransform rect = notificationTexts[i].GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -i * TEXT_HEIGHT);
            }
        }

        // Clear all notifications
        public static void ClearAll()
        {
            foreach (Text text in notificationTexts)
            {
                if (text != null)
                    Object.Destroy(text.gameObject);
            }
            Object.Destroy(leaveButton);

            notificationTexts.Clear();
        }

        // Get all notification texts
        public static List<Text> GetNotifications()
        {
            return new List<Text>(notificationTexts);
        }

        // Get the notification panel GameObject
        public static GameObject GetPanel()
        {
            return lobbyPanel;
        }
    }
}