using UnityEngine;
using UnityEngine.UI;

namespace SilksongMod
{
    public class CustomInputField : MonoBehaviour
{
    public string Text = ""; // Stores the typed text
    public int MaxLength = 100;

    [Header("UI")]
    public Image Background;
    public Text DisplayText; // Use UnityEngine.UI.Text

    private bool isFocused = false;

    void Awake()
    {
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
            DisplayText.font = LobbyManager.DefaultFont;
            DisplayText.resizeTextForBestFit = true;
            DisplayText.alignment = TextAnchor.MiddleLeft;

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);
        }
    }

    void Update()
    {
        // Focus logic: click to focus
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransform rt = GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
            {
                isFocused = true;
            }
            else
            {
                isFocused = false;
            }
        }

        if (!isFocused)
        {
            DisplayText.text = Text; // remove carret
        }
        else
        {
            // Handle typing
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // Backspace
                {
                    if (Text.Length > 0)
                        Text = Text.Substring(0, Text.Length - 1);
                }
                else if (c == '\n' || c == '\r') // Enter
                {
                    // Could trigger submit event here
                }
                else
                {
                    if (Text.Length < MaxLength)
                        Text += c;
                }
            }

            DisplayText.text = Text + "|"; // caret
        }
    }
}
}