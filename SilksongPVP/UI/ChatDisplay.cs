using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SilksongMod
{

    public class ChatDisplay : MonoBehaviour
    {
        //public static GameObject chatPanel; this monobehaviour is on the chatpanel object
        private static RectTransform chatPanelRect;
        public static GameObject messagesContainer;
        private static GameObject ScrollView;
        private static GameObject inputContainer;
        private static GameObject rootLayout;

        public static ChatInputField textBox;

        private static Vector2 ChatSize = new Vector2(300, 400);
        private static int chatBoxHeight = 40;
        private static int chatHeight = 20;

        private static Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        public void Awake()
        {
            CreateContainer();
            chatPanelRect = GetComponent<RectTransform>();
            rootLayout = CreateRootLayout(gameObject);
            messagesContainer = CreateMessagesContainer(rootLayout);
            inputContainer = CreateInputContainer(rootLayout);
            textBox = UIHelper.CreateChatTextBox(inputContainer, new Vector2(ChatSize.x, chatBoxHeight));
            textBox.transform.SetAsLastSibling();
        }

        public void CreateContainer()
        {
            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = gameObject.AddComponent<RectTransform>();
            }
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = ChatSize;
            rt.anchoredPosition = Vector2.zero;
        }

        private GameObject CreateRootLayout(GameObject parent)
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

        private GameObject CreateInputContainer(GameObject parent)
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

        private GameObject CreateMessagesContainer(GameObject parent)
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


        public void AddPlayerText(string name, string msg)
        {
            GameObject txt = UIHelper.CreateChatText(
                messagesContainer,
                $"{name}: {msg}",
                new Vector2(ChatSize.x, chatHeight),
                defaultFont,
                Color.white
            );

            // Newest messages at the bottom
            txt.transform.SetAsLastSibling();
            CullOldMessages(messagesContainer, chatPanelRect);
        }

        public void AddPlayerLeaveText(string name)
        {
            GameObject txt = UIHelper.CreateChatText(
                messagesContainer,
                $"<color=red>Player {name} Left Lobby!</color>",
                new Vector2(ChatSize.x, chatHeight),
                defaultFont,
                Color.red
            );

            // Newest messages at the bottom
            txt.transform.SetAsLastSibling();
            CullOldMessages(messagesContainer, chatPanelRect);
        }

        public void AddPlayerJoinText(string name)
        {
            GameObject txt = UIHelper.CreateChatText(
                messagesContainer,
                $"<color=green>Player {name} Joined Lobby!</color>",
                new Vector2(ChatSize.x, chatHeight),
                defaultFont,
                Color.green
            );

            // Newest messages at the bottom
            txt.transform.SetAsLastSibling();
            CullOldMessages(messagesContainer, chatPanelRect);
        }
        
        private void CullOldMessages(GameObject messagesContainer, RectTransform chatContainer)
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

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}