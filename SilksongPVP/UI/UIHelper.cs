using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SilksongMod
{
    public static class UIHelper
    {
        public static GameObject CreateBlank(GameObject parent)
        {
            GameObject overlayRoot = new GameObject(
                "OverlayRoot",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup)
            );

            overlayRoot.transform.SetParent(parent.transform, false);

// Stretch to full canvas
            RectTransform overlayRootRect = overlayRoot.GetComponent<RectTransform>();
            overlayRootRect.anchorMin = Vector2.zero;
            overlayRootRect.anchorMax = Vector2.one;
            overlayRootRect.offsetMin = Vector2.zero;
            overlayRootRect.offsetMax = Vector2.zero;

// Fully transparent image
            Image overlayRootImage = overlayRoot.GetComponent<Image>();
            overlayRootImage.color = new Color(0f, 0f, 0f, 0.4f);
            return overlayRoot;
        }

        public static GameObject CreateContainer(GameObject parent)
        {
            // 1. Create the container GameObject
            GameObject container = new GameObject("VerticalLayoutContainer", typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            container.transform.SetParent(parent.transform, false);


            // 2. Configure RectTransform
            RectTransform rt = container.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 2000); // width x height
            rt.anchorMin = new Vector2(0.5f, 0.75f);
            rt.anchorMax = new Vector2(0.5f, 0.75f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Image containerImage = container.AddComponent<Image>();
            containerImage.color = Color.black; // semi-transparent black

            // 3. Configure VerticalLayoutGroup
            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter; // top alignment
            layout.childForceExpandWidth = true; // stretch children horizontally
            layout.childForceExpandHeight = false; // don't stretch height
            layout.spacing = 10; // space between children

            // 4. Configure ContentSizeFitter
            ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            return container;
        }

        public static GameObject CreateText(GameObject parent, string text, Vector2 size, Color bg, Color textColor)
        {
            GameObject textGO = new GameObject(
                "InviteFriendsText",
                typeof(RectTransform),
                typeof(Text),
                typeof(LayoutElement)
            );

            textGO.transform.SetParent(parent.transform, false);

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            LayoutElement le = textGO.GetComponent<LayoutElement>();
            if (le == null)
            {
                SilksongModPlugin.Log.LogError("this is somehow null?");
                le = textGO.AddComponent<LayoutElement>();
            }

            le.preferredWidth = size.x;
            le.preferredHeight = size.y;

            //Image image = textGO.AddComponent<Image>();
            // image.color = bg;

            Text txt = textGO.GetComponent<Text>();

            txt.supportRichText = true;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = LobbyManager.DefaultFont;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;

            txt.text = text;

            txt.color = textColor;

            return textGO;
        }

        public static GameObject CreateChatText(GameObject parent, string text, Vector2 size, Font font, Color textColor)
        {
            GameObject textGO = new GameObject(
                "ChatText",
                typeof(RectTransform),
                typeof(LayoutElement)
            );
            SilksongModPlugin.Log.LogInfo("called createchattext with text " + text);

            textGO.transform.SetParent(parent.transform, false);
            
            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            Text txt = textGO.AddComponent<Text>();

            //txt.supportRichText = true;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.font = font;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.supportRichText = true;

            txt.text = text;

            txt.color = textColor;
            float neededHeight = txt.preferredHeight; // Unity calculates height for wrapped text
            LayoutElement le = textGO.GetComponent<LayoutElement>();
            if (le == null)
            {
                SilksongModPlugin.Log.LogError("this is somehow null?");
                le = textGO.AddComponent<LayoutElement>();
            }

            le.preferredWidth = size.x;
            le.preferredHeight = neededHeight;
            
            rt.sizeDelta = new Vector2(size.x, neededHeight);

            return textGO;
        }

        public static CustomInputField CreateTextBox(
            GameObject parentCanvas, Vector2 size,
            int maxLength = 100
        )
        {
            // Root GameObject
            GameObject go = new GameObject("CustomTextBox", typeof(RectTransform));
            go.transform.SetParent(parentCanvas.transform, false);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 600;
            le.preferredHeight = 40;
            CustomInputField inputField = go.AddComponent<CustomInputField>();
            inputField.MaxLength = maxLength;
            return inputField;
        }
        
        public static ChatInputField CreateChatTextBox(
            GameObject parentCanvas, Vector2 size
        )
        {
            // Root GameObject
            GameObject go = new GameObject("CustomChatTextBox", typeof(RectTransform));
            go.transform.SetParent(parentCanvas.transform, false);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;
            ChatInputField inputField = go.AddComponent<ChatInputField>();
            return inputField;
        }

        public static GameObject CreateFriendButton(GameObject parent, CSteamID steamID, string name)
        {
            // Create the button GameObject
            GameObject go =
                CreateButtonFromParent(parent, $"Name: {name} ({steamID})", new Color(0.7f, 0.7f, 0.9f, 1f), new Vector2(160, 40));
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => InviteButtonScript.FriendButtonPressed(steamID));
            return go;
        }

public static GameObject CreateButtonFromParent(GameObject parent, string txt, Color buttonColor, Vector2 size, int fontSize = 16)
        {
            GameObject buttonGO = new GameObject("MyButton", typeof(RectTransform), typeof(Button), typeof(Image));
            buttonGO.transform.SetParent(parent.transform, false); // parent to canvas

            // Set button size and position
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = size; // width x height
            buttonRect.anchoredPosition = Vector2.zero; // center of canvas

            LayoutElement le = buttonGO.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;

            // Set button background color
            Image buttonImage = buttonGO.GetComponent<Image>();
            buttonImage.color = buttonColor; // light purple

            // Add Button functionality
            Button button = buttonGO.GetComponent<Button>();
            button.interactable = true;
            // add outline
            Outline outline = buttonGO.AddComponent<Outline>();
            outline.effectColor = Color.white; // border color
            outline.effectDistance = new Vector2(5, 5); // thickness in pixels
            outline.useGraphicAlpha = true; // respects image alpha
            outline.enabled = false; // disable bcs not pressed yet

            // Create Text child
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(buttonGO.transform, false);

            Text text = textGO.GetComponent<Text>();
            text.text = txt;
            text.font = LobbyManager.DefaultFont; // default built-in font
            if (fontSize == 16)
            {
                text.resizeTextForBestFit = true;
            }
            else
            {
                text.fontSize = fontSize;
            }

            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;

            // Stretch text to fill button
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            return buttonGO;
        }

        public static GameObject CreateFriendDoneButton(GameObject parent)
        {
            GameObject go = CreateButtonFromParent(parent, "Done", Color.green, new Vector2(160, 60), 20);
            return go;
        }

        public static void PrintButtonInfo(GameObject buttonGO)
        {
            if (buttonGO == null)
            {
                SilksongModPlugin.Log.LogInfo("Button GameObject is null!");
                return;
            }

            // RectTransform info
            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"Button '{buttonGO.name}' RectTransform:\n" +
                    $"- Anchors: min={rt.anchorMin}, max={rt.anchorMax}\n" +
                    $"- Pivot: {rt.pivot}\n" +
                    $"- Anchored Position: {rt.anchoredPosition}\n" +
                    $"- Size Delta: {rt.sizeDelta}\n" +
                    $"- World Position: {rt.position}"
                );
            }

            // Button info
            Button btn = buttonGO.GetComponent<Button>();
            if (btn != null)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"- Interactable: {btn.interactable}\n" +
                    $"- OnClick listeners: {btn.onClick.GetPersistentEventCount()}"
                );
            }

            // Image info
            Image img = buttonGO.GetComponent<Image>();
            if (img != null)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"- Image color: {img.color}\n" +
                    $"- RaycastTarget: {img.raycastTarget}"
                );
            }

            // Optional: Text info if button has child text
            Text text = buttonGO.GetComponentInChildren<Text>();
            if (text != null)
            {
                SilksongModPlugin.Log.LogInfo(
                    $"- Text: '{text.text}'\n" +
                    $"- Font: {text.font?.name}\n" +
                    $"- Alignment: {text.alignment}\n" +
                    $"- Color: {text.color}"
                );
            }
        }
    }
}