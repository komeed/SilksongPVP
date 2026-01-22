using System;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongMod
{
    public class ChatInputField : MonoBehaviour
    {
        public string Text = "";
        private int MaxLength = 100;

        [Header("UI")] 
        public Image Background;
        public Text DisplayText;
        public Text pressToChatTextComp;

        private bool isFocused = false;
        private int caretIndex = 0;
        private int fontSize = 16;
        
        private float keyHoldTime = 0f;
        private float repeatDelay = 1f; // 1 second before fast move
        private float repeatRate = 0.05f; // move every 0.05s after delay
        private KeyCode heldKey = KeyCode.None;
        private float lastMoveTime = 0f;
        
        private string enterKeyString = "";
        private KeyCode enterKey;
        private Font defaultFont;

        void Awake()
        {
            defaultFont = Font.CreateDynamicFontFromOSFont("Courier New", 18);
            if (defaultFont == null)
            {
                SilksongModPlugin.Log.LogInfo("still couldn't find COURIER NEW");
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (Background == null)
            {
                Background = gameObject.AddComponent<Image>();
                Background.color = Color.white;
            }

            if (DisplayText == null)
            {
                GameObject textGO = new GameObject("Text", typeof(RectTransform));
                textGO.transform.SetParent(transform, false);

                DisplayText = textGO.AddComponent<Text>();
                DisplayText.color = Color.black;
                DisplayText.font = defaultFont;
                DisplayText.fontSize = fontSize;
                DisplayText.alignment = TextAnchor.MiddleLeft;
                DisplayText.horizontalOverflow = HorizontalWrapMode.Overflow;
                DisplayText.fontStyle = FontStyle.Bold;

                RectTransform rt = textGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(5, 5);
                rt.offsetMax = new Vector2(-5, -5);

                GameObject pressToChatText = new GameObject("Placeholder", typeof(RectTransform));
                pressToChatText.transform.SetParent(textGO.transform, false);

                RectTransform rttxt = pressToChatText.GetComponent<RectTransform>();
                rttxt.anchorMin = Vector2.zero;
                rttxt.anchorMax = Vector2.one;
                rttxt.offsetMin = rttxt.offsetMax = Vector2.zero;
                rttxt.pivot = new Vector2(0.5f, 0.5f);

                pressToChatTextComp = pressToChatText.AddComponent<Text>();
                pressToChatTextComp.color = Color.gray;
                pressToChatTextComp.font = defaultFont;
                pressToChatTextComp.alignment = TextAnchor.MiddleLeft;
               // pressToChatTextComp.text = "Press 'Enter' To Chat";
            }
        }

        void Update()
        {
            if (enterKeyString != SilksongModPlugin.ChatKeyString.Value)
            {
                enterKeyString = SilksongModPlugin.ChatKeyString.Value;
                if (!Enum.TryParse(SilksongModPlugin.ChatKeyString.Value, true, out enterKey))
                {
                    // Fallback if parsing fails
                    enterKey = KeyCode.Return;
                    SilksongModPlugin.Log.LogError(
                        $"Invalid chat key in config: {SilksongModPlugin.ChatKeyString.Value}, defaulting to Enter");
                }

                pressToChatTextComp.text = $"Press '{GetKeyName(enterKey)}' to Chat";
            }

            pressToChatTextComp.gameObject.SetActive(string.IsNullOrEmpty(Text));
            // Focus toggle
            if (!isFocused)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    isFocused = true;
                    LobbyManager.FreezeGame();
                }

                return;
            }
            else
            {
                if (Text.Length == 0)
                {
                    pressToChatTextComp.text = $"Type a message (max 100c)";
                }
            }

            // --- Arrow key hold handling ---
            if (Input.GetKeyDown(KeyCode.LeftArrow)) StartHold(KeyCode.LeftArrow);
            if (Input.GetKeyDown(KeyCode.RightArrow)) StartHold(KeyCode.RightArrow);
            if (Input.GetKeyUp(KeyCode.LeftArrow) && heldKey == KeyCode.LeftArrow) EndHold();
            if (Input.GetKeyUp(KeyCode.RightArrow) && heldKey == KeyCode.RightArrow) EndHold();

            if (heldKey != KeyCode.None)
            {
                keyHoldTime += Time.deltaTime;
                if (keyHoldTime > repeatDelay && Time.time - lastMoveTime > repeatRate)
                {
                    MoveCaret(heldKey);
                    lastMoveTime = Time.time;
                }
            }

            // --- Character typing ---
            foreach (char c in Input.inputString)
            {
                if (c == '\b' && caretIndex > 0)
                    Text = Text.Remove(--caretIndex, 1);
                else if (c == '\n' || c == '\r')
                {
                    if (Text.Length > 0) LobbyManager.SendMessage(Text);
                    Text = "";
                    caretIndex = 0;
                    isFocused = false;
                    LobbyManager.UnfreezeGame();
                    pressToChatTextComp.text = $"Press '{GetKeyName(enterKey)}' to Chat";
                }
                else if (char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || char.IsPunctuation(c))
                {
                    if (Text.Length < MaxLength)
                        Text = Text.Insert(caretIndex++, c.ToString());
                }
            }

            if (isFocused)
            {
                caretIndex = Mathf.Clamp(caretIndex, 0, Text.Length);

                // --- Determine visible text ---
                float width = DisplayText.rectTransform.rect.width;

                // Rough approximation of character width (mono font)
                float charWidth = DisplayText.fontSize * 0.6f;
                int maxVisibleChars = Mathf.FloorToInt(width / charWidth);

                int start = Mathf.Max(0, caretIndex - maxVisibleChars + 1);
                int end = Mathf.Min(Text.Length, start + maxVisibleChars);

                string visibleText = Text.Substring(start, end - start);

                // Insert caret
                int caretPosInVisible = caretIndex - start;
                DisplayText.text = visibleText.Insert(caretPosInVisible, "|");
            }
            else
            {
                DisplayText.text = "";
            }

        }


        void StartHold(KeyCode key)
        {
            heldKey = key;
            keyHoldTime = 0f;
            lastMoveTime = Time.time;
            MoveCaret(key); // move immediately on press
        }

        void EndHold()
        {
            heldKey = KeyCode.None;
            keyHoldTime = 0f;
        }

        void MoveCaret(KeyCode key)
        {
            if (key == KeyCode.LeftArrow) caretIndex = Mathf.Max(0, caretIndex - 1);
            if (key == KeyCode.RightArrow) caretIndex = Mathf.Min(Text.Length, caretIndex + 1);
        }
        
        string GetKeyName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Return: return "Enter";
                case KeyCode.Escape: return "Escape";
                case KeyCode.Space: return "Space";
                case KeyCode.LeftArrow: return "Left Arrow";
                case KeyCode.RightArrow: return "Right Arrow";
                case KeyCode.UpArrow: return "Up Arrow";
                case KeyCode.DownArrow: return "Down Arrow";
                default: return key.ToString(); // letters and numbers are fine
            }
        }
    }
}
