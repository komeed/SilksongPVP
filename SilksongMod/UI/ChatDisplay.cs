using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilksongMod
{
    using UnityEngine;
    using UnityEngine.UI;

    public static class ChatDisplay
    {
        public static GameObject chatPanel;
        public static Font customFont;
        
        public static TMP_InputField inputField;

        public static void Init(GameObject parent, Font font)
        {
            customFont = font;

            // Create panel
            chatPanel = new GameObject("ChatPanel", typeof(RectTransform), typeof(Image));
            chatPanel.transform.SetParent(parent.transform, false);

            RectTransform rt = chatPanel.GetComponent<RectTransform>();

            // Anchor bottom-left
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot     = new Vector2(0f, 0f);

            // Position & size
            rt.anchoredPosition = new Vector2(20f, 20f);
            rt.sizeDelta = new Vector2(400f, 200f);

            // Background styling
            Image bg = chatPanel.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);

            // Optional: add a layout group for messages
            VerticalLayoutGroup layout = chatPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            CreateTextBox(chatPanel);
        }

        public static void CreateTextBox(GameObject parent)
        {
            GameObject inputGO = new GameObject("ChatInput", typeof(RectTransform));
            inputGO.transform.SetParent(parent.transform, false);

            RectTransform inputRT = inputGO.GetComponent<RectTransform>();
            inputRT.anchorMin = new Vector2(0f, 0f);
            inputRT.anchorMax = new Vector2(1f, 0f);
            inputRT.pivot = new Vector2(0.5f, 0f);
            inputRT.sizeDelta = new Vector2(-20f, 40f);
            inputRT.anchoredPosition = new Vector2(0f, 10f);

            // ======================
            // Background
            // ======================
            Image inputBG = inputGO.AddComponent<Image>();
            inputBG.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            // ======================
            // TMP Input Field
            // ======================
            inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            // ======================
            // Text Component
            // ======================
            GameObject textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(inputGO.transform, false);

            TMP_Text text = textGO.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
            text.color = Color.white;
            text.enableWordWrapping = false;

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10f, 6f);
            textRT.offsetMax = new Vector2(-10f, -6f);

            inputField.textComponent = text;

            // ======================
            // Placeholder
            // ======================
            GameObject placeholderGO = new GameObject("Placeholder", typeof(RectTransform));
            placeholderGO.transform.SetParent(inputGO.transform, false);

            TMP_Text placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholder.text = "Press Enter to chat...";
            placeholder.fontSize = 18;
            placeholder.color = new Color(1f, 1f, 1f, 0.4f);

            RectTransform phRT = placeholderGO.GetComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = new Vector2(10f, 6f);
            phRT.offsetMax = new Vector2(-10f, -6f);

            inputField.placeholder = placeholder;

            // ======================
            // Submit Behavior
            // ======================
            inputField.onSubmit.AddListener(printResponse);
        }

        public static void printResponse(string msg)
        {
            SilksongModPlugin.Log.LogInfo("Sent message " + msg);
        }
    }

}