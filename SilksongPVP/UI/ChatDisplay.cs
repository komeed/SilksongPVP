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
        private static RectTransform chatPanelRect;
        public static GameObject messagesContainer;
        private static GameObject ScrollView;
        private static GameObject inputContainer;
        private static GameObject rootLayout;
        public static Font customFont;

        public static CustomInputField textBox;

        private static Vector2 ChatSize = new Vector2(300, 400);
        private static int chatBoxHeight = 30;
        private static int chatHeight = 20;

        public static void Init(GameObject parent, Font font)
        {
            chatPanel = CreateContainer(parent);
            chatPanelRect = chatPanel.GetComponent<RectTransform>();
            rootLayout = CreateRootLayout(chatPanel);
            messagesContainer = CreateMessagesContainer(rootLayout);
            inputContainer = CreateInputContainer(rootLayout);
            textBox = UIHelper.CreateTextBox(inputContainer, new Vector2(ChatSize.x, chatBoxHeight));
            textBox.isChatText = true;
            textBox.transform.SetAsLastSibling();
            textBox.DisplayText.resizeTextForBestFit = false;
            textBox.DisplayText.fontSize = 18;
        }

        public static GameObject CreateContainer(GameObject parent)
        {
            GameObject container = new GameObject(
                "ChatContainer",
                typeof(RectTransform)
            );
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = ChatSize;
            rt.anchoredPosition = Vector2.zero;

            //Image img = container.GetComponent<Image>();
            //img.color = new Color(0f, 0f, 0f, 0.75f);

            return container;
        }

        private static GameObject CreateRootLayout(GameObject parent)
        {
            GameObject root = new GameObject(
                "RootLayout",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup)
            );
            root.transform.SetParent(parent.transform, false);

            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0;
            layout.padding = new RectOffset(10, 10, 10, 10);

            return root;
        }

        private static GameObject CreateInputContainer(GameObject parent)
        {
            GameObject container = new GameObject(
                "InputContainer",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup)
            );
            container.transform.SetParent(parent.transform, false);

            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return container;
        }

        private static GameObject CreateMessagesContainer(GameObject parent)
        {
            GameObject container = new GameObject(
                "MessagesContainer",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter)
            );
            container.transform.SetParent(parent.transform, false);

            RectTransform rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 0f);
            rt.offsetMin = new Vector2(0f, chatBoxHeight); // leave space for textbox
            rt.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return container;
        }


        public static void AddPlayerText(string name, string msg)
        {
            GameObject txt = UIHelper.CreateChatText(
                messagesContainer,
                $"{name}: {msg}",
                new Vector2(ChatSize.x, chatHeight),
                textBox.DisplayText.font,
                Color.white
            );

            // Newest messages at the bottom
            txt.transform.SetAsLastSibling();
            CullOldMessages(messagesContainer, chatPanelRect);
        }

        public static void AddPlayerLeaveText(string name)
        {
            GameObject txt = UIHelper.CreateChatText(
                messagesContainer,
                $"<color=red>Player {name} Left Lobby!</color>",
                new Vector2(ChatSize.x, chatHeight),
                textBox.DisplayText.font,
                Color.red
            );

            // Newest messages at the bottom
            txt.transform.SetAsLastSibling();
            CullOldMessages(messagesContainer, chatPanelRect);
        }

        public static void AddPlayerJoinText(string name)
        {
            GameObject txt = UIHelper.CreateChatText(
                messagesContainer,
                $"<color=green>Player {name} Joined Lobby!</color>",
                new Vector2(ChatSize.x, chatHeight),
                textBox.DisplayText.font,
                Color.green
            );

            // Newest messages at the bottom
            txt.transform.SetAsLastSibling();
            CullOldMessages(messagesContainer, chatPanelRect);
        }
        
        public static void CullOldMessages(GameObject messagesContainer, RectTransform chatContainer)
        {
            if (messagesContainer.transform.childCount == 0)
                return;

            // Get first child (oldest message)
            Transform firstMessage = messagesContainer.transform.GetChild(0);
            RectTransform firstRT = firstMessage.GetComponent<RectTransform>();

            // Get top of the message relative to the container
            Vector3[] containerCorners = new Vector3[4];
            chatContainer.GetWorldCorners(containerCorners);
            float containerTop = containerCorners[1].y; // top-left corner in world space

            Vector3[] messageCorners = new Vector3[4];
            firstRT.GetWorldCorners(messageCorners);
            float messageTop = messageCorners[1].y;

            // If the top of the message is above the container top, delete it
            if (messageTop > containerTop)
            {
                GameObject.Destroy(firstMessage.gameObject);
            }
        }
    }
}