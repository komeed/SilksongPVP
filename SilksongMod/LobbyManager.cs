using System;
using System.Collections.Generic;
using System.Text;
using SilksongMod.SteamP2P;
using UnityEngine;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ALERT: LOBBY INCLUDES YOURSELF

namespace SilksongMod // canvas is transform.parent
{
    public class LobbyManager : MonoBehaviour
    {
        public static Font DefaultFont = Font.CreateDynamicFontFromOSFont("Arial", 12);
        
        private bool _apiRunning;

        public static SteamPlayer CurrPlayer;
        
        public static string CurrScene;
        
        public static Dictionary<SteamPlayer, string> Players = new Dictionary<SteamPlayer, string>();
        
        //temporary buffer that stores the lobby sent until the user presses join (which lets him join this lobby)
        public static Dictionary<SteamPlayer, string> PendingLobbyBuffer = new Dictionary<SteamPlayer, string>();
        
        /// GLOBAL HOST INSTANCE VARIABLES ///
        
        public static Dictionary<CSteamID, GameObject> SyncedHornets = new Dictionary<CSteamID, GameObject>();
        
        public static GameObject HostHornet;
        
       // public static HashSet<CSteamID> PendingPlayer =  new HashSet<CSteamID>(); // players that haven't responded yet 

        public bool sent;
        public void Awake()
        {
            sent = false;
            _apiRunning = false;
            SilksongModPlugin.Log.LogInfo("Starting API");
            
            Canvas canvas = GetComponent<Canvas>(); // check canvas elements
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                SilksongModPlugin.Log.LogInfo("CANVAS DIDN'T HAVE GRAPHIC RAYCASTER; Creating raycaster");
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            LobbyDisplay.Init(gameObject, DefaultFont);
            ResetLobby();
            
            JoinDisplay.Init(gameObject, DefaultFont);
            JoinDisplay.SetVisible(false);
        }

        private static void ResetLobby()
        {
            string steamName = SteamFriends.GetPersonaName();
            CSteamID PlayerSteamID = SteamUser.GetSteamID();
            CurrPlayer = new SteamPlayer(steamName, PlayerSteamID);
            Players = new Dictionary<SteamPlayer, string>();
            Players.Add(CurrPlayer, "MAINMENU"); // default (when game is loading)
            
            SyncedHornets = new Dictionary<CSteamID, GameObject>();
            PendingLobbyBuffer = new Dictionary<SteamPlayer, string>();
            
            UpdateLobbyUI(); // Update lobby with current player stats
        }

        public void Start()
        {
            SilksongModPlugin.Log.LogInfo("START CALLED");
        }

        public void Update()
        {
            // every frame send position data? try
            if (Players.Count > 1 && CurrScene != "MAINMENU")
            {
                SendPositionToLobby();
            }
        }

        public void DisplayInvite()
        {
            InviteButtonScript.CreateVerticalLayout(gameObject);
        }

        private void OnDestroy()
        {
            SilksongModPlugin.Log.LogInfo("LOBBY MAANGER DESTROYED HOW???");
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                SilksongModPlugin.Log.LogInfo("FOUND CANVAS ON DESTROY! Adding back LobbyManager.");
                canvas.gameObject.AddComponent<LobbyManager>();
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("UHOH. Canvas Destroyed. What do we do?");
            }
        }

        public static void CreateJoin(SteamPlayer player)
        {
            if (!JoinDisplay.IsActive())
            {
                SilksongModPlugin.Log.LogInfo($"Creating Join for player {player.Name}");
                JoinDisplay.SetVisible(true);
                JoinDisplay.SetText(player);
            }
        }

        public static void JoinButtonPressed(string buttonName)
        {
            if (buttonName == "Join") // if join pressed, join the lobby and send to everyone in lobby that you've joined
            {
                if (PendingLobbyBuffer.IsNullOrEmpty())
                {
                    SilksongModPlugin.Log.LogError("PENDING LOBBY BUFFER IS NULL/EMPTY; Incorrect Join request. Please invite the player first!");
                    return;
                }

                SteamPlayer p = new SteamPlayer(JoinDisplay.player.Name, JoinDisplay.player.SteamID);
                if (Players.ContainsKey(p))
                {
                    SilksongModPlugin.Log.LogInfo($"Player {p.Name} is already in lobby! Incorrect Join Request.");
                    return;
                }
                SilksongModPlugin.Log.LogInfo($"player accepted {p.Name}'s ({p.SteamID}) join request");
                // first, set the current player dictionary to the new one, and add yourself into it
                Players = PendingLobbyBuffer;
                PendingLobbyBuffer = null;  // remove previous lobbybuffer reference for safety
                
                //send confirmation of join as a SteamPlayer to all of the people in the new lobby
                foreach (SteamPlayer player in Players.Keys)
                {
                    SteamP2PSender.SendPlayerJoinConfirmation(player.SteamID, CurrPlayer, CurrScene);
                }
                
                Players.Add(CurrPlayer, CurrScene); // add yourself
                UpdateLobbyUI();
            }
            else // cancel pressed
            {
                SilksongModPlugin.Log.LogInfo($"player canceled join request");
                SteamNetworking.CloseP2PSessionWithUser(JoinDisplay.player.SteamID); // canceled, so close session
            }
            JoinDisplay.SetVisible(false);
        }
// current data sending: 
        public static void MoveToNewLobby(Dictionary<SteamPlayer, string> data)
        {
            // first, update your own hashmap to contain this data, (but add yourself as well of course)
            // if you are accepting the join, your send will be a string, so this function will not call.
            Players = data;
        }
        // add new player to lobby (no checks, direct)
        public static void AddPlayerToLobby(KeyValuePair<SteamPlayer, string> player)
        {
            SilksongModPlugin.Log.LogInfo($"Adding player {player.Key.Name} in {player.Value}");
            // to add a player, you need to add it to the dictionary and update the ui
            Players.Add(player.Key, player.Value);
            UpdateLobbyUI(); // upadte your own lobby
           
            //create that player's syncedHornet gameobject
            CreateHornet(player.Key, player.Value);
        }

        private static void UpdateLobbyUI()
        {
            LobbyDisplay.UpdatePlayerList(Players);
        }

        public static void UpdateSyncedHornetPos(CSteamID steamID, Vector3 pos, Vector3 scale)
        {
            GameObject hornet = SyncedHornets[steamID];
            hornet.transform.position = pos; // set all position and scale stuff
            hornet.transform.localScale = scale;
        }
        
        public static void SendPositionToLobby()
        {
            if (HostHornet != null)
            {
                //serialize position and scale (scale for direction, position for position)
                byte[] data = Serializer.SerializeTransform(HostHornet.transform.position, HostHornet.transform.localScale);
                foreach (KeyValuePair<SteamPlayer, string> playerData in Players)
                {
                    if (!playerData.Key.Equals(CurrPlayer) && playerData.Value.Equals(CurrScene))
                    {
                        SteamP2PSender.SendPositionDataTo(playerData.Key, data);
                    }
                }
            }
        }
        
        public static void UpdateSceneForPlayer(SteamPlayer player, string scene) {
            Players[player] = scene; // update players dict
            GameObject hornet = SyncedHornets[player.SteamID];
            hornet.SetActive(scene != "MAINMENU"); // set active only if changed
        }

        public static void SendAnimationChangeToLobby(byte[] data)
        {
            foreach (KeyValuePair<SteamPlayer, string> playerData in Players)
            {
                if (!playerData.Key.Equals(CurrPlayer))
                {
                    if (playerData.Value.Equals(CurrScene)) // only if they are on the same scene, send
                    {
                        SteamP2PSender.SendAnimationChangeTo(playerData.Key, data);
                    }
                }
            }
        }

        public static void SendLobbyToPlayer(SteamPlayer player)
        {
            SteamP2PSender.SendLobbyData(player, Players);
        }

        public static void SendLobbyToPlayerWithJoin(SteamPlayer player)
        {
            SteamP2PSender.SendLobbyDataToJoin(player, Players);
        }

        public static void UpdateCurrSceneAndSend(string scene)
        {
            CurrScene = scene;
            foreach (SteamPlayer player in Players.Keys)
            {
                if (player.Equals(CurrPlayer))
                {
                    Players[player] = scene; // update dictionary's scene
                    continue;
                }
                SteamP2PSender.SendCurrSceneToPlayer(player, CurrScene);
            }
        }
        
        private static void CreateHornet(SteamPlayer player, string scene)
        {
            
            GameObject syncedHornet = new GameObject("SyncedHornet");
            
            SyncedHornets.Add(player.SteamID, syncedHornet);
            if (scene == CurrScene && CurrScene != "MAINMENU")
            {
                SilksongModPlugin.Log.LogInfo("CreateHornet: player added while ingame, in same scene.");
                syncedHornet.SetActive(true);
            }
            else
            {
                syncedHornet.SetActive(false);
            }
            // VERY IMPORTANT: Add script after setting not active
            SyncedHornetScript script = syncedHornet.AddComponent<SyncedHornetScript>(); 
            script.steamID = player.SteamID;
            script.name = player.Name;
            
            SilksongModPlugin.Log.LogInfo("Created Hornet.");
        }
        
        public static void SetHostHornet(GameObject hornet)
        {
            HostHornet = hornet;
        }

        public static void DeActivateHornets()
        {
            foreach (GameObject hornet in SyncedHornets.Values)
            {
                hornet.SetActive(false);
            }
        }

        public static void LeaveButtonPressed()
        {
            SilksongModPlugin.Log.LogInfo("Leave button pressed.");
            //first send to all people in lobby
            byte[] data = Serializer.SerializeLeaveLobby(CurrPlayer);
            foreach (SteamPlayer player in Players.Keys)
            {
                if (!player.Equals(CurrPlayer))
                {
                    SteamP2PSender.SendData(player.SteamID, data, P2PChannel.Lobby);
                }
            }
            ResetLobby();
        }

        public static void LeaveRecievedFromPlayer(SteamPlayer player)
        {
            SilksongModPlugin.Log.LogInfo($"Leave recieved from player {player.Name}");
            Players.Remove(player);
            GameObject hornet = SyncedHornets[player.SteamID];
            Destroy(hornet); //simply destroy him
            SyncedHornets.Remove(player.SteamID);
        }
    }
}