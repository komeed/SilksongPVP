using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using Steamworks;

namespace SilksongMod
{
    public class LobbyDisplay : MonoBehaviour
    {
        // private static GameObject lobbyPanel;
        private List<Text> notificationTexts = new List<Text>();

        // private GameObject leaveButton;
        private GameObject parent;

        private const float TEXT_HEIGHT = 30f;
        private const int PADDING = 10;

        private static Vector2 panelSize = new Vector2(300f, 500f);

        private static Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        private Text lobbyText;
        private static int lobbyTextFontSize = 18;

        private static int textFontSize = 16;
        private Text showPlayersButtonText;
        private bool showingPlayers = true;

        private Button JoinPVPLobbyButton;

        public void Awake()
        {
            CreateLayoutContainer();
            lobbyText = AddText("Private Lobby: ", lobbyTextFontSize);
            showPlayersButtonText = CreateShowPlayersButton(textFontSize);
            CreateLeaveButton(AddText(LobbyManager.CurrName, textFontSize).gameObject, textFontSize);
            JoinPVPLobbyButton = CreateJoinGlobalLobbyButton(textFontSize);
        }

        private void CreateLayoutContainer()
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

// Anchor top-left (still fine for panels)
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(PADDING, -PADDING);
            rectTransform.sizeDelta = panelSize;
            
            VerticalLayoutGroup layout = gameObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 6f;
            layout.padding = new RectOffset(PADDING, PADDING, PADDING, PADDING);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

// 🔹 Optional but VERY common: auto-size the panel
            ContentSizeFitter fitter = gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public void Update()
        {
        }

        public void UpdatePlayerList(Dictionary<CSteamID, SyncedHornetScript> players, int fontSize = 16)
        {
            ClearAll();
            foreach (KeyValuePair<CSteamID, SyncedHornetScript> player in players)
            {
                Text temp = AddText(player.Value.name, fontSize);
                notificationTexts.Add(temp);
            }
            JoinPVPLobbyButton.transform.SetAsLastSibling();
        }

        // Add ae new notification text
        private Text AddText(string message, int fontSize = 14)
        {
            GameObject textObj = new GameObject($"Notification_{notificationTexts.Count}");
            textObj.transform.SetParent(transform, false);

            // --- Text ---
            Text text = textObj.AddComponent<Text>();
            text.text = message;
            text.font = defaultFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // --- RectTransform ---
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);

            // Width controlled by parent, height by preferred size
            rt.sizeDelta = new Vector2(0, 0);
            // (Optional) Explicit layout hint
            LayoutElement layout = textObj.GetComponent<LayoutElement>();
            if (layout == null)
                layout = textObj.AddComponent<LayoutElement>();

            layout.minHeight = fontSize + 6f; // small safety padding
            layout.preferredHeight = -1;      // let Text calculate height
            //notificationTexts.Add(text);
            return text;
        }


        private void CreateLeaveButton(GameObject parentText, int fontSize = 14)
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
            btnText.font = defaultFont;
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

        private Text CreateShowPlayersButton(int fontSize = 14)
        {
            // --- Button GameObject ---
            GameObject buttonObj = new GameObject("ShowPlayers");
            buttonObj.transform.SetParent(transform, false);

            // --- RectTransform ---
            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(120, 26); // height only; width from layout

            // --- Button ---
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnShowPlayersClicked);

            // --- Background Image (required for Button) ---
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.gray;

            // --- Layout Element ---
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.minHeight = 26;
            layout.preferredHeight = 26;
            layout.preferredWidth = 120;

            // --- Text Child ---
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.text = "Hide Players";
            text.font = defaultFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // --- Text RectTransform (fill button) ---
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return text;
        }
        
        private Button CreateJoinGlobalLobbyButton(int fontSize = 14)
        {
            // --- Button GameObject ---
            GameObject buttonObj = new GameObject("JoinGlobalLobbyButton");
            buttonObj.transform.SetParent(transform, false);

            // --- RectTransform ---
            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(120, 32); // height only; width from layout

            // --- Button ---
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnJoinGlobalLobbyClicked);

            // --- Background Image (required for Button) ---
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.gray;

            // --- Layout Element ---
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.minHeight = 32;
            layout.preferredHeight = 40;
            layout.preferredWidth = 150;

            // --- Text Child ---
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.text = "Join Public PVP Lobby";
            text.font = defaultFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // --- Text RectTransform (fill button) ---
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return button;
        }

        private void OnJoinGlobalLobbyClicked()
        {
            if (!LobbyManager.isGlobalLobby)
            {
                LobbyManager.waitingForServerResponse = true;
                LobbyManager.JoinGlobalLobby();
            }
            else
            {
                InviteButtonScript.CreateErrorLayout(transform.parent.gameObject, "You are already in a public lobby! Leave this one first.");
            }
        }

        private void OnShowPlayersClicked()
        {
            SilksongModPlugin.Log.LogInfo("show players clickcedd!!!");
            if (showingPlayers)
            {
                showPlayersButtonText.text = "Show Players";
                showingPlayers = false;
                HideAll();

            }
            else
            {
                showPlayersButtonText.text = "Hide Players";
                showingPlayers = true;
                ShowAll();
            }
        }


        // Clear all notifications
        public void ClearAll()
        {
            foreach (Text text in notificationTexts)
            {
                if (text != null)
                    Object.Destroy(text.gameObject);
            }

            notificationTexts.Clear();
        }

        public void HideAll()
        {
            foreach (Text text in notificationTexts)
            {
                text.gameObject.SetActive(false);
            }
        }

        public void ShowAll()
        {
            foreach (Text text in notificationTexts)
            {
                text.gameObject.SetActive(true);
            }
        }

        public void SetPanelActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetLobbyIDText(int id)
        {
            lobbyText.text = $"Public Lobby: (lobbyID = {id})";
        }

        public void ClearLobbyID()
        {
            lobbyText.text = $"Private Lobby: ";
        }
    }
}