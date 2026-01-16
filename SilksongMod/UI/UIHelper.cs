using Steamworks;
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
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
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

        public static GameObject CreateInviteFriendsText(GameObject parent)
        {
            GameObject textGO = new GameObject(
                "InviteFriendsText",
                typeof(RectTransform),
                typeof(Text),
                typeof(LayoutElement)
            );

            textGO.transform.SetParent(parent.transform, false);

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 140);

            LayoutElement le = textGO.GetComponent<LayoutElement>();
            le.preferredWidth = 600;
            le.preferredHeight = 140;

            Text txt = textGO.GetComponent<Text>();

            txt.supportRichText = true;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = LobbyManager.DefaultFont;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;

            txt.text =
                "<size=48>Invite Friends</size>\n" +
                "<size=18><color=#FFFFFF99>Only friends currently playing the game can be invited.</color></size>";

            txt.color = Color.white;

            return textGO;
        }


         public static GameObject CreateButtonFromParent(GameObject parent, CSteamID steamID, string name)
         {
             // Create the button GameObject
             GameObject buttonGO = new GameObject("MyButton", typeof(RectTransform), typeof(Button), typeof(Image));
             buttonGO.transform.SetParent(parent.transform, false); // parent to canvas

             // Set button size and position
             RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
             buttonRect.sizeDelta = new Vector2(160, 40); // width x height
             buttonRect.anchoredPosition = Vector2.zero;  // center of canvas
             
             LayoutElement le = buttonGO.AddComponent<LayoutElement>();
             le.preferredWidth = 160;
             le.preferredHeight = 40;

             // Set button background color
             Image buttonImage = buttonGO.GetComponent<Image>();
             buttonImage.color = new Color(0.7f, 0.7f, 0.9f, 1f); // light purple

             // Add Button functionality
             Button button = buttonGO.GetComponent<Button>();
             button.onClick.AddListener(() => InviteButtonScript.FriendButtonPressed(steamID));
             button.interactable = true;
             
             // add outline
             Outline outline = buttonGO.AddComponent<Outline>();
             outline.effectColor = Color.white;       // border color
             outline.effectDistance = new Vector2(5, 5); // thickness in pixels
             outline.useGraphicAlpha = true;          // respects image alpha
             outline.enabled = false; // disable bcs not pressed yet

             // Create Text child
             GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
             textGO.transform.SetParent(buttonGO.transform, false);

             Text text = textGO.GetComponent<Text>();
             text.text = $"Name: {name} ({steamID})";
             text.font = LobbyManager.DefaultFont; // default built-in font
             text.fontSize = 16;
             text.alignment = TextAnchor.MiddleCenter;
             text.color = Color.black;

             // Stretch text to fill button
             RectTransform textRect = textGO.GetComponent<RectTransform>();
             textRect.anchorMin = Vector2.zero;
             textRect.anchorMax = Vector2.one;
             textRect.offsetMin = textRect.offsetMax = Vector2.zero;
             return buttonGO;
         }

         public static GameObject CreateDoneButton(GameObject parent)
         {
             // Create the button GameObject
             GameObject buttonGO = new GameObject("MyButton", typeof(RectTransform), typeof(Button), typeof(Image));
             buttonGO.transform.SetParent(parent.transform, false); // parent to canvas

             // Set button size and position
             RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
             buttonRect.sizeDelta = new Vector2(160, 60); // width x height
             buttonRect.anchoredPosition = Vector2.zero;  // center of canvas
             
             LayoutElement le = buttonGO.AddComponent<LayoutElement>();
             le.preferredWidth = 160;
             le.preferredHeight = 60;

             // Set button background color
             Image buttonImage = buttonGO.GetComponent<Image>();
             buttonImage.color = new Color(0.0f, 1.0f, 0.0f, 1.0f); // light purple

             // Add Button functionality
             Button button = buttonGO.GetComponent<Button>();
             button.onClick.AddListener(InviteButtonScript.DonePressed);
             button.interactable = true;

             // Create Text child
             GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
             textGO.transform.SetParent(buttonGO.transform, false);

             Text text = textGO.GetComponent<Text>();
             text.text = "Done";
             text.font = LobbyManager.DefaultFont; // default built-in font
             text.fontSize = 20;
             text.alignment = TextAnchor.MiddleCenter;
             text.color = Color.black;

             // Stretch text to fill button
             RectTransform textRect = textGO.GetComponent<RectTransform>();
             textRect.anchorMin = Vector2.zero;
             textRect.anchorMax = Vector2.one;
             textRect.offsetMin = textRect.offsetMax = Vector2.zero;
             return buttonGO;
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