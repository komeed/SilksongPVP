using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Steamworks;

namespace SilksongMod
{
    public static class InviteButtonScript
    {
        private static HashSet<CSteamID> friendsAdded;
        
        private static Dictionary<CSteamID, GameObject> buttonMap; 
        private static GameObject overlayRoot;
        
        public static void CreateVerticalLayout(GameObject parent)
        {
            buttonMap = new Dictionary<CSteamID, GameObject>();
            friendsAdded = new HashSet<CSteamID>();
            // 1. Create the container GameObject
            overlayRoot = UIHelper.CreateBlank(parent);
            GameObject container = UIHelper.CreateContainer(overlayRoot);
            UIHelper.CreateInviteFriendsText(container);
            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
    
                // Check if friend is playing a game
                if (SteamFriends.GetFriendGamePlayed(friendSteamID, out FriendGameInfo_t gameInfo))
                {
                    // Filter by AppID
                    if (gameInfo.m_gameID.AppID() == new AppId_t(1030300)) // only if they are playing silksoing right now
                    {
                        string friendName = SteamFriends.GetFriendPersonaName(friendSteamID);
                        SilksongModPlugin.Log.LogInfo("Showing friend playing the game: " + friendName);

                        GameObject button = UIHelper.CreateButtonFromParent(container, friendSteamID, friendName);
                        buttonMap.Add(friendSteamID, button);
                    }
                }
            }

            UIHelper.CreateDoneButton(container);
        }
        public static void RemoveOverlay()
        {
            if (overlayRoot != null)
            {
                UnityEngine.Object.Destroy(overlayRoot);
            }
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

        public static void DonePressed()
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
            RemoveOverlay();
        }
    }
}