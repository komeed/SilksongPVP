using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Steamworks;
using TMPro;

namespace SilksongMod
{
    public static class InviteButtonScript
    {
        private static HashSet<CSteamID> friendsAdded;
        private static GameObject parentCanvas;
        
        private static Dictionary<CSteamID, GameObject> buttonMap; 
        private static GameObject overlayRoot;
        private static CustomInputField inputField;

        private static GameObject hostLobbyButton;
        private static GameObject joinLobbyButton;
        
        public static void CreateFriendLayout(GameObject parent)
        {
            RemoveOverlay(); // ensure the overlay is already removed
            parentCanvas = parent;
            buttonMap = new Dictionary<CSteamID, GameObject>();
            friendsAdded = new HashSet<CSteamID>();
            // 1. Create the container GameObject
            overlayRoot = UIHelper.CreateBlank(parent);
            GameObject container = UIHelper.CreateContainer(overlayRoot);
            UIHelper.CreateText(container, "<size=48>Invite Friends</size>\n" +
                                                        "<size=18><color=#FFFFFF99>Only friends currently playing the game can be invited.</color></size>",
                new Vector2(600, 140), Color.black, Color.white);
            if (!SteamUser.BLoggedOn()) // steam user is offline, show offline message
            {
                UIHelper.CreateText(container, "<size=24>Error fetching friends. Make sure you are connected to the internet!</size>",
                    new Vector2(600, 70), new Color(1f, 0.4f, 0.0f, 1f), Color.white);
            }
            else
            {
                int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

                for (int i = 0; i < friendCount; i++)
                {
                    CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

                    // Check if friend is playing a game
                    if (SteamFriends.GetFriendGamePlayed(friendSteamID, out FriendGameInfo_t gameInfo))
                    {
                        // Filter by AppID
                        if (gameInfo.m_gameID.AppID() ==
                            new AppId_t(1030300)) // only if they are playing silksoing right now
                        {
                            string friendName = SteamFriends.GetFriendPersonaName(friendSteamID);
                            SilksongModPlugin.Log.LogInfo("Showing friend playing the game: " + friendName);

                            GameObject button = UIHelper.CreateFriendButton(container, friendSteamID, friendName);
                            buttonMap.Add(friendSteamID, button);
                        }
                    }
                }

                UIHelper.CreateText(container, "<size=24>OR: (for nonsteam users)</size>" ,
                    new Vector2(600, 40),
                    Color.black, Color.white);
                hostLobbyButton = UIHelper.CreateButtonFromParent(container, "Create Lobby", Color.gray, new Vector2(160, 40));
                hostLobbyButton.GetComponent<Button>().onClick.AddListener(HostButtonPressed);
                joinLobbyButton = UIHelper.CreateButtonFromParent(container, "Join Lobby", Color.gray, new Vector2(160, 40));
                joinLobbyButton.GetComponent<Button>().onClick.AddListener(JoinButtonPressed);
                
               // inputField = UIHelper.CreateTextBox(container);
            }

            GameObject go = UIHelper.CreateFriendDoneButton(container);
            go.GetComponent<Button>().onClick.AddListener(FriendDoneButtonPressed);
        }
        
        private static void CreateHostLobbyLayout(GameObject parent)
        {
            // 1. Create the container GameObject
            overlayRoot = UIHelper.CreateBlank(parent);
            GameObject container = UIHelper.CreateContainer(overlayRoot);
            UIHelper.CreateText(container, "<size=48>Host Lobby Name:</size>\n" +
                                           "<size=18><color=#FFFFFF99>WARNING: can be buggy, only use if lobby contains non-steam users.</color></size>",
                new Vector2(600, 140), Color.black, Color.white);
            if (!SteamUser.BLoggedOn()) // steam user is offline, show offline message
            {
                UIHelper.CreateText(container, "<size=24>Error. Make sure you are connected to the internet!</size>",
                    new Vector2(600, 70), new Color(1f, 0.4f, 0.0f, 1f), Color.white);
            }
            else
            {
                inputField = UIHelper.CreateTextBox(container);
            }

            GameObject doneButton = UIHelper.CreateFriendDoneButton(container);
            doneButton.GetComponent<Button>().onClick.AddListener(HostDoneButtonPressed);
            GameObject closeButton = UIHelper.CreateButtonFromParent(container, "Close", Color.gray, new Vector2(160, 40));
            closeButton.GetComponent<Button>().onClick.AddListener(RemoveOverlay);
        }

        private static void CreateJoinLobbyLayout(GameObject parent)
        {
            overlayRoot = UIHelper.CreateBlank(parent);
            GameObject container = UIHelper.CreateContainer(overlayRoot);
            UIHelper.CreateText(container, "<size=48>Join Lobby Name:</size>\n" +
                                           "<size=18><color=#FFFFFF99>WARNING: can be buggy, only use if lobby contains non-steam users.</color></size>",
                new Vector2(600, 140), Color.black, Color.white);
            if (!SteamUser.BLoggedOn()) // steam user is offline, show offline message
            {
                UIHelper.CreateText(container, "<size=24>Error. Make sure you are connected to the internet!</size>",
                    new Vector2(600, 70), new Color(1f, 0.4f, 0.0f, 1f), Color.white);
            }
            else
            {
                inputField = UIHelper.CreateTextBox(container);
            }

            GameObject doneButton = UIHelper.CreateFriendDoneButton(container);
            doneButton.GetComponent<Button>().onClick.AddListener(JoinDoneButtonPressed);
            GameObject closeButton = UIHelper.CreateButtonFromParent(container, "Close", Color.gray, new Vector2(160, 40));
            closeButton.GetComponent<Button>().onClick.AddListener(RemoveOverlay);
        }

        public static void CreateErrorLayout(GameObject parent, string errorMsg)
        {
            if (parent == null)
            {
                SilksongModPlugin.Log.LogError("This shouldn't hpapen LOL");
            }
            //SilksongModPlugin.Log.LogInfo("Started here with msg: " + errorMsg);
            RemoveOverlay(); // incase overlay exists
            overlayRoot = UIHelper.CreateBlank(parent);
            GameObject container = UIHelper.CreateContainer(overlayRoot);
            UIHelper.CreateText(container, "<size=48>ERROR</size>\n" + 
                                           "<size=18>" + errorMsg + "</size>", 
                new Vector2(600, 140), Color.black, Color.white);
            GameObject closeButton = UIHelper.CreateButtonFromParent(container, "Close", Color.gray, new Vector2(160, 40));
            closeButton.GetComponent<Button>().onClick.AddListener(RemoveOverlay);
            //SilksongModPlugin.Log.LogInfo("ended here");
        }
        
        private static void RemoveOverlay()
        {
            Object.Destroy(overlayRoot);
        }

        public static void FriendButtonPressed(CSteamID player)
        {
            GameObject button = buttonMap[player];
            if (button == null)
            {
                SilksongModPlugin.Log.LogError($"BUTTON FOR {player} NOT FOUND; deleted somehow?");
                return;
            }

            if (friendsAdded.Contains(player))
            {
                button.GetComponent<Outline>().enabled = false;
                friendsAdded.Remove(player);
            }
            else
            {
                button.GetComponent<Outline>().enabled = true;
                friendsAdded.Add(player);
            }
        }

        private static void FriendDoneButtonPressed()
        {
            foreach (CSteamID player in friendsAdded)
            { 
                if (!LobbyManager.LobbyPlayers.ContainsKey(player)) // if current lobby doesn't have this friend
                {
                    //send the current lobby info (players hashmap)
                    LobbyManager.SendLobbyToPlayerWithJoin(player);
                    SilksongModPlugin.Log.LogInfo($"SENT JOIN REQUEST TO FRIEND: {player})");
                    //LobbyManager.AddPlayerToLobby(steamID, friendName);
                  //  LobbyManager.PendingPlayer.Add(steamID); // first, add player to "pending" (we don't know if he accepts yet)
                }
            }
            SilksongModPlugin.Log.LogInfo($"text from textbox: {inputField.Text}");
            RemoveOverlay();
        }

        private static void HostDoneButtonPressed()
        {
            if (inputField != null)
            {
                if (inputField.Text != "")
                {
                    SilksongModPlugin.Log.LogInfo("HOST: User inputted lobby name: " + inputField.Text);
                    LobbyManager.SendHostLobby(inputField.Text);
                }
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("HOST: ERROR: inputfield doesn't exist! we did something wrong here");
            }
            RemoveOverlay();
        }

        private static void JoinDoneButtonPressed()
        {
            if (inputField != null)
            {
                if (inputField.Text != "")
                {
                    SilksongModPlugin.Log.LogInfo("JOIN: User inputted lobby name: " + inputField.Text);
                    LobbyManager.SendJoinLobby(inputField.Text);
                }
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("JOIN: ERROR: inputfield doesn't exist! we did something wrong here");
            }
            RemoveOverlay();
        }

        public static void HostButtonPressed()
        {
            SilksongModPlugin.Log.LogInfo("host button pressed! showing lobby name enter.");
            RemoveOverlay();
            CreateHostLobbyLayout(parentCanvas);
        }

        private static void JoinButtonPressed()
        {
            SilksongModPlugin.Log.LogInfo("join button pressed! showing lobby name join.");
            RemoveOverlay();
            CreateJoinLobbyLayout(parentCanvas);
        }
    }
}