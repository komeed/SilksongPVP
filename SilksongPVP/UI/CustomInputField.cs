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
    public int fontSize = 12;

    public bool isChatText = false;

    public GameObject pressToChatText;

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
            pressToChatText = new GameObject("Text", typeof(RectTransform));
            pressToChatText.transform.SetParent(textGO.transform, false);
            
            RectTransform rttxt = pressToChatText.GetComponent<RectTransform>(); // Match parent size
            rttxt.anchorMin = new Vector2(0f, 0f); // bottom-left
            rttxt.anchorMax = new Vector2(1f, 1f); // top-right
            rttxt.offsetMin = Vector2.zero; // no extra offset
            rttxt.offsetMax = Vector2.zero;
            rttxt.pivot = new Vector2(0.5f, 0.5f); // center pivot (optional)
            
            Text txt = pressToChatText.AddComponent<Text>();
            txt.color = Color.gray;
            txt.font = LobbyManager.DefaultFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            //pressToChatText.resizeTextForBestFit = true;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.text = "Press 'Enter' To Chat";
            //pressToChatText.supportRichText = true;

            DisplayText = textGO.AddComponent<Text>();
            DisplayText.color = Color.black;
            DisplayText.font = LobbyManager.DefaultFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            DisplayText.resizeTextForBestFit = true;
            DisplayText.alignment = TextAnchor.MiddleLeft;
           // DisplayText.supportRichText = true;

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);
        }
    }

    void Update()
    {

        if (isChatText)
        {
            if (!isFocused)
            {
                foreach (char c in Input.inputString)
                {
                    if (c == '\n' || c == '\r') // Enter
                    {
                        SilksongModPlugin.Log.LogInfo("enter pressed if chat text!");
                        isFocused = true;
                        LobbyManager.FreezeGame();
                        return;
                    }
                }
            }
            if (DisplayText.text.Length != 0)
            {
                pressToChatText.SetActive(false);
            }
            else
            {
                pressToChatText.SetActive(true);
            }
        }
        else
        {
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
                    if (isChatText)
                    {
                        //chat sent, call send message
                        LobbyManager.UnfreezeGame();
                        if (Text.Length > 0)
                        {
                            LobbyManager.SendMessage(Text);
                        }

                        Text = "";
                        DisplayText.text = "";
                        isFocused = false;
                    }
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