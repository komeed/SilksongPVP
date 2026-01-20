using System;
using System.Collections.Generic;
using System.Text;
using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using InControl;
using InControl.UnityDeviceProfiles;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;
using UnityEngine;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = System.Object;

namespace SilksongMod 
{
    public class LobbyManager : MonoBehaviour
    {
        public static Font DefaultFont = Font.CreateDynamicFontFromOSFont("Arial", 12);

        public static bool isGlobalLobby;

        public static CSteamID CurrSteamID;
        public static string CurrName;
        public static string CurrScene;
        
        //DOESN'T INCLUDE YOURSELF; HUGE OPTIMIZATION!
        public static Dictionary<CSteamID, SyncedHornetScript> LobbyPlayers = new Dictionary<CSteamID, SyncedHornetScript>();
        
        public static Dictionary<CSteamID, string> PendingLobbyBuffer = new Dictionary<CSteamID, string>();

        public static Dictionary<NailAttackBase, int> NABListIndex = new Dictionary<NailAttackBase, int>();
        
        public static GameObject HostHornet;
        public static HeroController HeroController;

        public static GameObject AttacksBuffer;

        public static bool HitEnemy;
        
        private static string serverIP = "10.0.0.167";

        public static UDPConnect server;

        public static bool foundTraverseMethod = false;
        
       // public static HashSet<CSteamID> PendingPlayer =  new HashSet<CSteamID>(); // players that haven't responded yet 
        
        #region Event Functions
        public void Awake()
        {
            isGlobalLobby = false;
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

            if (server == null)
            {
                server = gameObject.AddComponent<UDPConnect>();
                server.Init(serverIP, 9999, true); // it is the server
            }
            
            LobbySwitch.CreateSwitchWithLabel(canvas);
            ChatDisplay.Init(gameObject, DefaultFont);
            
         //   Traverse.Create(__instance).Method("PrivateMethodName")
        }
        
       /* async void Start()
        {
            SilksongModPlugin.Log.LogInfo("START CALLED");
            await server.JoinGlobalLobby(CurrSteamID, CurrName);
        }*/

        public void Update()
        {
            foreach (SyncedHornetScript script in LobbyPlayers.Values)
            {
                if (CurrScene != "MAINMENU" && script.scene == CurrScene)
                {
                    script.gameObject.SetActive(true);
                }
                else {
                    script.gameObject.SetActive(false);
                }
            }
        }
        
        private void OnDestroy()
        {
            SilksongModPlugin.Log.LogInfo("LOBBY MAANGER DESTROYED. Sending Leave button.");
            SendLeaveToLobby();
        }

        private void OnApplicationQuit()
        {
            SilksongModPlugin.Log.LogInfo("Application Quitting! Sending Leave");
            SendLeaveToLobby();
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
                
                SilksongModPlugin.Log.LogInfo($"player accepted {JoinDisplay.currName}'s ({JoinDisplay.currSteamID}) join request");
                // first, set the current player dictionary to the new one, and add yourself into it
                Dictionary<CSteamID, SyncedHornetScript> temp = new Dictionary<CSteamID, SyncedHornetScript>(); // incase one of the members already exist in lobby
                foreach (var player in PendingLobbyBuffer)
                {
                    if (LobbyPlayers.ContainsKey(player.Key))
                    {
                        SilksongModPlugin.Log.LogInfo($"Player {player.Value} is already in lobby! Incorrect Join Request.");
                        return;
                    }
                    SyncedHornetScript script = CreateHornet(player.Key, player.Value, "temp"); // not set yet
                    temp.Add(player.Key, script);
                }
                //after ensuring nobody is duplicated in lobby, set players to the new temp dict and send to all players in lobby
                LobbyPlayers = temp;
                PendingLobbyBuffer.Clear();  // remove previous lobbybuffer reference for safety
                byte[] data = Serializer.SerializeSinglePlayer(CurrSteamID, CurrName, CurrScene);
                //send confirmation of join as a SteamPlayer to all of the people in the new lobby
                foreach (CSteamID steamID in LobbyPlayers.Keys)
                {
                    SteamP2PSender.SendData(steamID, data, P2PChannel.Lobby); // send current scene now
                }
                UpdateLobbyUI();
            }
            else // cancel pressed
            {
                SilksongModPlugin.Log.LogInfo($"player canceled join request");
                SteamNetworking.CloseP2PSessionWithUser(JoinDisplay.currSteamID); // canceled, so close session
            }
            JoinDisplay.SetVisible(false);
        }
        
        public void DisplayInvite()
        {
            InviteButtonScript.CreateFriendLayout(gameObject);
        }
        
        public static void LeaveButtonPressed()
        {
            SilksongModPlugin.Log.LogInfo("Leave button pressed.");
            if (isGlobalLobby)
            {
                // if you are leaving the global lobby, make sure to send the server that you are leaving as well
                server.LeaveGlobalLobby(CurrSteamID, CurrName);
            }
            //first send to all people in lobby
            SendLeaveToLobby();
            ResetLobby();
        }
        #endregion
        
        #region Lobby Commands
        // add new player to lobby (no checks, direct)
        public static void AddPlayerToLobby((CSteamID steamID, string name, string scene) playerData)
        {
            SilksongModPlugin.Log.LogInfo($"Adding player {playerData.steamID}. Haven't recieved scene yet.");
            ChatDisplay.AddPlayerJoinText(playerData.name);
            // to add a player, you need to add it to the dictionary and update the ui
            SyncedHornetScript script = CreateHornet(playerData.steamID, playerData.name, playerData.scene); // createhornet does appending to dictionary for you
            LobbyPlayers.Add(playerData.steamID, script);
            UpdateLobbyUI(); // upadte your own lobby
        }

        public static void SendLeaveToLobby()
        {
            byte[] data = Serializer.SerializeLeaveLobby();
            foreach (CSteamID player in LobbyPlayers.Keys)
            {
                SteamP2PSender.SendData(player, data, P2PChannel.Lobby);
            }
        }
        
        public static void SendDataToLobby(byte[] data, P2PChannel channel)
        {
            if (CurrScene != "MAINMENU")
            {
                foreach (SyncedHornetScript script in LobbyPlayers.Values)
                {
                    if (script.scene == CurrScene)
                    {
                        SteamP2PSender.SendData(script.steamID, data, channel);
                    }
                }
            }
        }
        
        public static void ActivateHornets(bool active)
        {
            foreach (SyncedHornetScript hornet in LobbyPlayers.Values)
            {
                hornet.gameObject.SetActive(active);
            }
        }
        
        public static void UpdateSyncedHornetPos(CSteamID steamID, PlayerPosData posData)
        {
            SyncedHornetScript hornet = LobbyPlayers[steamID];
            hornet.UpdatePosition(posData);
        }
        
        #endregion
        // incase we want to show the lobby contents as well
        public static void CreateJoin(CSteamID sender, Dictionary<CSteamID, string> lobby)
        {
            if (!JoinDisplay.IsActive())
            {
                if (lobby.TryGetValue(sender, out string name))
                {
                    SilksongModPlugin.Log.LogInfo($"Creating Join for player name");
                    JoinDisplay.SetVisible(true);
                    JoinDisplay.SetText(sender, name);
                }
            }
        }
        
        public static void UpdateSceneForPlayer(CSteamID sender, string scene) {
            if (LobbyPlayers.TryGetValue(sender, out SyncedHornetScript script))
            {
                script.scene = scene;
                SilksongModPlugin.Log.LogInfo($"Player {script.name} joined {scene}");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"ERROR: Couldn't find syncedhornetscript for player {sender}, something bad happeneed");
            }
        }

        public static void SendLobbyToPlayerWithJoin(CSteamID player)
        {
            SteamP2PSender.SendLobbyDataToJoin(player);
        }

        public static void UpdateCurrSceneAndSend(string scene)
        {
            SilksongModPlugin.Log.LogInfo($"Updating CurrSceneAndSend for {scene}");
            CurrScene = scene;
            foreach (CSteamID player in LobbyPlayers.Keys)
            {
                SteamP2PSender.SendCurrSceneToPlayer(player, CurrScene);
            }
        }
        
        public static void SetHostHornet(GameObject hornet)
        {
            HostHornet = hornet;
            HostHornet.AddComponent<NetworkSender>();
            HeroController = HostHornet.GetComponent<HeroController>();
            SilksongModPlugin.Log.LogInfo($"InvulTime for herocontroller: {HeroController.INVUL_TIME}");
        }

        public static void LeaveRecievedFromPlayer(CSteamID player)
        {
            SilksongModPlugin.Log.LogInfo($"Leave recieved from player {player}");
            if (LobbyPlayers.TryGetValue(player, out SyncedHornetScript hornet))
            {
                ChatDisplay.AddPlayerLeaveText(hornet.name);
                Destroy(hornet.gameObject); //simply destroy him
                LobbyPlayers.Remove(player);
                UpdateLobbyUI();
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("Player didn't exist in the first place. Must have canceled p2p.");
            }
        }
        
        #region Helper Methods

        private static void UpdateLobbyUI()
        {
            if (!isGlobalLobby)
            {
                LobbyDisplay.UpdatePlayerList(LobbyPlayers);
            }
        }
        
        public static SyncedHornetScript CreateHornet(CSteamID steamID, string name, string scene)
        {
            GameObject syncedHornet = new GameObject("SyncedHornet");
            // VERY IMPORTANT: Add script after setting not active
            syncedHornet.SetActive(false);
            SyncedHornetScript script = syncedHornet.AddComponent<SyncedHornetScript>(); 
            script.steamID = steamID;
            script.name = name;
            script.scene = scene;
           // LobbyPlayers.Add(steamID, script);
            DontDestroyOnLoad(syncedHornet);
            
            SilksongModPlugin.Log.LogInfo("Created Hornet.");
            return script;
        }
        
        private static void ResetLobby()
        {
            CurrName = SteamFriends.GetPersonaName();
            CurrSteamID = SteamUser.GetSteamID();
            LobbyPlayers.Clear();
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

        public static void HeroTakeDamage(int damageAmount, CollisionSide side, CSteamID sender, bool shortenInvul)
        {
            HitEnemy = false;
            SilksongModPlugin.Log.LogInfo($"InvulTime for herocontroller: {HeroController.INVUL_TIME}");
            // HeroController.INVUL_TIME /= 4;
            HeroController.TakeDamage(null, side, damageAmount, HazardType.ENEMY);
            if (HitEnemy)
            {
                SteamP2PSender.SendData(sender, new byte[1] { (byte)LobbyCommand.Ping }, P2PChannel.Attack);
            }
            // do this some other way
            /*if (HitEnemy) // if the hit registers and player loses health, send hit confirmation
            {
                SilksongModPlugin.Log.LogInfo("hit is registered successfully! sending now.");
                
                HitEnemy = false;
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("I hit you but you didn't recieve the hit, how can this be?");
            }*/
            HitEnemy = false;
        }
        
        #endregion

        #region UDP

        public static void SendJoinLobby(string lobbyName)
        {
            
        }

        public static void SendHostLobby(string lobbyName)
        {
            
        }

        #endregion

        public static void SendMessage(string msg)
        {
            ChatDisplay.AddPlayerText(CurrName, msg);
            SilksongModPlugin.Log.LogInfo("sent message!");
            byte[] data = Serializer.SerializeMessage(CurrName, msg);
            SendDataToLobby(data, P2PChannel.Lobby);
        }

        public static void FreezeGame()
        {
            SilksongModPlugin.Log.LogInfo("freezing game!");
            Time.timeScale = 0f;
        }

        public static void UnfreezeGame()
        {
            Time.timeScale = 1f;
        }
    }
}