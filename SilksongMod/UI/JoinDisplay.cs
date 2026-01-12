using UnityEngine;
using UnityEngine.UI;
using Steamworks;

namespace SilksongMod
{
    public static class JoinDisplay
    {
        private static GameObject displayPanel;
        private static Image backgroundImage;
        private static Text displayText;
        private static Button joinButton;
        private static Button cancelButton;
        private static Font customFont;

        private const float PANEL_WIDTH = 300f;
        private const float PANEL_HEIGHT = 200f;
        private const float PADDING = 20f;
        private const float BUTTON_HEIGHT = 40f;
        private const float BUTTON_SPACING = 10f;

        public static SteamPlayer player;

        // Initialize the join display
        public static void Init(GameObject parent, Font font)
        {
            if (displayPanel != null) return;

            customFont = font;

            // Create the main panel
            displayPanel = new GameObject("JoinDisplayPanel");
            displayPanel.transform.SetParent(parent.transform, false);

            // Add RectTransform and position in top-right
            RectTransform panelRect = displayPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.anchoredPosition = new Vector2(-PADDING, -PADDING);
            panelRect.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);

            // Add purple background image
            backgroundImage = displayPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0.5f, 0f, 0.5f, 1f); // Purple

            // Create text in top half
            CreateDisplayText();

            // Create buttons in bottom half
            CreateButtons();
        }

        private static void CreateDisplayText()
        {
            GameObject textObj = new GameObject("DisplayText");
            textObj.transform.SetParent(displayPanel.transform, false);

            displayText = textObj.AddComponent<Text>();
            displayText.font = customFont;
            displayText.fontSize = 18;
            displayText.color = Color.white;
            displayText.alignment = TextAnchor.MiddleCenter;
            displayText.text = "temp";

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.5f);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, -10);
        }

        private static void CreateButtons()
        {
            // Left button
            joinButton = CreateButton("joinButton", new Vector2(0, 0), new Vector2(0.5f, 0.5f),
                new Vector2(-BUTTON_SPACING / 2, BUTTON_SPACING), "Join", Color.green);

            // Right button
            cancelButton = CreateButton("cancelButton", new Vector2(0.5f, 0), new Vector2(1, 0.5f),
                new Vector2(BUTTON_SPACING / 2, BUTTON_SPACING), "Cancel", Color.gray);
        }

        private static Button CreateButton(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offset, string buttonText, Color color)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(displayPanel.transform, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = offset;
            buttonRect.offsetMin = new Vector2(10, 10);
            buttonRect.offsetMax = new Vector2(-10, -10);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color;

            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => LobbyManager.JoinButtonPressed(buttonText));
            
            // Create button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.font = customFont;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = buttonText;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        // Public accessors
        public static GameObject GetPanel() => displayPanel;
        public static Text GetText() => displayText;
        public static Button GetLeftButton() => joinButton;
        public static Button GetRightButton() => cancelButton;

        // Utility methods
        public static void SetText(SteamPlayer p)
        {
            if (displayText != null)
                displayText.text = $"Join {p.Name}'s Lobby?";
            player = p;
        }

        public static void SetVisible(bool visible)
        {
            if (displayPanel != null)
                displayPanel.SetActive(visible);
        }

        public static void SetLeftButtonText(string text)
        {
            if (joinButton != null)
                joinButton.GetComponentInChildren<Text>().text = text;
        }

        public static void SetRightButtonText(string text)
        {
            if (cancelButton != null)
                cancelButton.GetComponentInChildren<Text>().text = text;
        }

        public static bool IsActive()
        {
            if (displayPanel != null)
            {
                return displayPanel.activeSelf;
            }

            return false;
        }
    }
}