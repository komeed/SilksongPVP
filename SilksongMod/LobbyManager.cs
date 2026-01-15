using System;
using System.Collections.Generic;
using System.Text;
using InControl;
using SilksongMod.SteamP2P;
using UnityEngine;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = System.Object;

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
        public static Dictionary<CSteamID, SyncedHornetScript> SyncedHornetScripts = new Dictionary<CSteamID, SyncedHornetScript>();
        
        public static Dictionary<NailAttackBase, int> NABListIndex = new Dictionary<NailAttackBase, int>();
     //   public static Dictionary<CSteamID, GameObject> SyncedHornets = new Dictionary<CSteamID, GameObject>();
        
        public static GameObject HostHornet;
        public static HeroController HeroController;

        public static GameObject AttacksBuffer;
        
       // public static HashSet<CSteamID> PendingPlayer =  new HashSet<CSteamID>(); // players that haven't responded yet 
        
        #region Event Functions
        public void Awake()
        {
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
        
        public void Start()
        {
            SilksongModPlugin.Log.LogInfo("START CALLED");
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
        
        #endregion
        
        #region Button Listeners
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
                foreach (var player in Players)
                {
                    SteamP2PSender.SendPlayerJoinConfirmation(player.Key.SteamID, CurrPlayer, CurrScene);
                    CreateHornet(player.Key, player.Value);
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
        
        public void DisplayInvite()
        {
            InviteButtonScript.CreateVerticalLayout(gameObject);
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
        #endregion
        
        #region Lobby Commands
        
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
        
        public static void SendDataToLobby(byte[] data, P2PChannel channel)
        {
            foreach (KeyValuePair<SteamPlayer, string> playerData in Players)
            {
                if (!playerData.Key.Equals(CurrPlayer))
                {
                    if (playerData.Value.Equals(CurrScene) && CurrScene != "MAINMENU") // only if they are on the same scene, send
                    {
                        if (SteamP2PSender.SendData(playerData.Key.SteamID, data, channel))
                        {
                            SilksongModPlugin.Log.LogInfo($"Successfully sent animation change to Player {playerData.Key.Name}");
                        }
                        else
                        {
                            SilksongModPlugin.Log.LogError($"Failed to send animation change to Player {playerData.Key.Name}");
                        }
                    }
                }
            }
        }
        
        public static void ActivateHornets(bool active)
        {
            foreach (SyncedHornetScript hornet in SyncedHornetScripts.Values)
            {
                hornet.gameObject.SetActive(active);
            }
        }
        
        
        public static void UpdateSyncedHornetPos(CSteamID steamID, PlayerPosData posData)
        {
            SyncedHornetScript hornet = SyncedHornetScripts[steamID];
            hornet.UpdatePosition(posData);
        }
        
        #endregion

        public static void CreateJoin(SteamPlayer player)
        {
            if (!JoinDisplay.IsActive())
            {
                SilksongModPlugin.Log.LogInfo($"Creating Join for player {player.Name}");
                JoinDisplay.SetVisible(true);
                JoinDisplay.SetText(player);
            }
        }
        
        public static void UpdateSceneForPlayer(SteamPlayer player, string scene) {
            Players[player] = scene; // update players dict
            SyncedHornetScript hornet = SyncedHornetScripts[player.SteamID];
            hornet.gameObject.SetActive(scene != "MAINMENU" && CurrScene != "MAINMENU" && scene == CurrScene); // set active only if changed
            SilksongModPlugin.Log.LogInfo($"Player {player.Name} joined {scene}");
        }

        public static void SendLobbyToPlayerWithJoin(SteamPlayer player)
        {
            SteamP2PSender.SendLobbyDataToJoin(player, Players);
        }

        public static void UpdateCurrSceneAndSend(string scene)
        {
            SilksongModPlugin.Log.LogInfo($"Updating CurrSceneAndSend for {scene}");
            CurrScene = scene;
            SilksongModPlugin.Log.LogInfo($"Players count before send: {Players.Count}");
            Players[CurrPlayer] = scene;
            foreach (SteamPlayer player in Players.Keys)
            {
                if (player.Equals(CurrPlayer))
                {
                    continue;
                }
                SteamP2PSender.SendCurrSceneToPlayer(player, CurrScene);
            }
            
            SilksongModPlugin.Log.LogInfo($"Players count after send: {Players.Count}");
        }
        
        public static void SetHostHornet(GameObject hornet)
        {
            HostHornet = hornet;
            HostHornet.AddComponent<NetworkSender>();
            HeroController = HostHornet.GetComponent<HeroController>();
        }

        public static void LeaveRecievedFromPlayer(SteamPlayer player)
        {
            SilksongModPlugin.Log.LogInfo($"Leave recieved from player {player.Name}");
            Players.Remove(player);
            SyncedHornetScript hornet = SyncedHornetScripts[player.SteamID];
            Destroy(hornet.gameObject); //simply destroy him
            SyncedHornetScripts.Remove(player.SteamID);
            UpdateLobbyUI();
        }
        
        #region Helper Methods
        
        private static void UpdateLobbyUI() { LobbyDisplay.UpdatePlayerList(Players); }
        
        private static void CreateHornet(SteamPlayer player, string scene)
        {
            GameObject syncedHornet = new GameObject("SyncedHornet");
            
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
            SyncedHornetScripts.Add(player.SteamID, script);
            DontDestroyOnLoad(syncedHornet);
            
            SilksongModPlugin.Log.LogInfo("Created Hornet.");
            
            // now that the script is inactive, instantiate a 
        }
        
        private static void ResetLobby()
        {
            string steamName = SteamFriends.GetPersonaName();
            CSteamID PlayerSteamID = SteamUser.GetSteamID();
            CurrPlayer = new SteamPlayer(steamName, PlayerSteamID);
            Players = new Dictionary<SteamPlayer, string>();
            Players.Add(CurrPlayer, "MAINMENU"); // default (when game is loading)

            SyncedHornetScripts.Clear();
            PendingLobbyBuffer.Clear();
            
            UpdateLobbyUI(); // Update lobby with current player stats
        }

        public static void StoreNailAttackComponents(GameObject hornet)
        {
            NABListIndex.Clear(); // clear everything because new memory references
            NailAttackBase[] NABList =
                    hornet.gameObject.GetComponentsInChildren<NailAttackBase>(true);
            // used to retrieve the index with O(1) search instead of O(n)
            for (int i = 0; i < NABList.Length; i++)
            {
                NABListIndex[NABList[i]] = i;
            }
        }
        
        #endregion
    }
}