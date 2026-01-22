using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace SilksongMod.SteamP2P
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class UDPConnect : MonoBehaviour
    {
        private UdpClient udpClient;
        private string remoteIP;
        private int remotePort;

        private bool listening = false;
        public bool isServer;

        private TaskCompletionSource<Dictionary<ulong, string>> lobbyResponseTcs;

        public void Init(string remoteIP, int remotePort, bool isServer)
        {
            this.remoteIP = remoteIP;
            this.remotePort = remotePort;
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(remoteIP), remotePort);
            this.isServer = isServer; 
            _ = StartListening();
        }

        void OnDestroy()
        {
            listening = false;
            udpClient?.Close();
            udpClient = null;
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length);
        }

        // Start the async receive loop
        private async Task StartListening()
        {
            listening = true;
            while (listening)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    byte[] data = result.Buffer;
                    if (data.Length == 0)
                    {
                        SilksongModPlugin.Log.LogError("EXTREMELY WEIRD MESSAGE WTF?");
                    }
                    else
                    {
                        if (data[0] == (byte)UDPCommand.Ping)
                        {
                            SilksongModPlugin.Log.LogInfo("PING RECEIVED from server! sending pong.");
                            byte[] newData =
                                Serializer.SerializeSteamID(UDPCommand.Pong, LobbyManager.CurrSteamID.m_SteamID);
                            udpClient.Send(newData, newData.Length);
                        }

                        if (data[0] == (byte)UDPCommand.JoinGlobalLobby)
                        {
                            SilksongModPlugin.Log.LogInfo("Joining lobby");
                            (int lobbyID, Dictionary<ulong, string> dict) = Deserializer.DeserializeLobbyPlayerDict(data);
                            LobbyManager.SetLobbyIDText(lobbyID);
                            if (lobbyResponseTcs != null && !lobbyResponseTcs.Task.IsCompleted)
                            {
                                lobbyResponseTcs.SetResult(dict);
                            }

                            ProcessLobbyDict(dict);
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Socket was closed, exit the loop
                    break;
                }
                catch (SocketException ex)
                {
                    SilksongModPlugin.Log.LogInfo("UDP Socket error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    SilksongModPlugin.Log.LogInfo("Unexpected error in UDP listener: " + ex);

                }
            }
        }

        //this works itself like a ping/pong: after recieving back message, it sends the rest of the lobby (otherwise no)

        public async Task JoinGlobalLobby(CSteamID steamID, string name)
        {
            SilksongModPlugin.Log.LogInfo("Called JoinGlobalLobby method");

            // Prepare message
            byte[] data = Serializer.SteamIDNameToBytes(UDPCommand.JoinGlobalLobby, steamID.m_SteamID, name);
            await udpClient.SendAsync(data, data.Length);
            SilksongModPlugin.Log.LogInfo("Sent join lobby request");

            // Create a TaskCompletionSource to wait for response
            lobbyResponseTcs = new TaskCompletionSource<Dictionary<ulong, string>>();

            // Wait for either response or timeout
            Task delayTask = Task.Delay(2000); // 1.2 seconds
            Task finishedTask = await Task.WhenAny(lobbyResponseTcs.Task, delayTask);

            if (finishedTask == lobbyResponseTcs.Task)
            {
                Dictionary<ulong, string> dict = lobbyResponseTcs.Task.Result;
                SilksongModPlugin.Log.LogInfo("Lobby response received from server");
                // Continue processing
                foreach (var x in dict)
                {
                    CSteamID newID = new CSteamID(x.Key);
                    if (newID != LobbyManager.CurrSteamID)
                    {
                        SilksongModPlugin.Log.LogInfo($"Joining player in lobby: {x.Key}: {x.Value}");
                        // send your local player data via P2P
                        byte[] singlePlayerData = Serializer.SerializeSinglePlayer(LobbyManager.CurrSteamID,
                            LobbyManager.CurrName, LobbyManager.CurrScene);
                        SteamP2PSender.SendData(newID, singlePlayerData, P2PChannel.Lobby);
                        SyncedHornetScript script = LobbyManager.CreateHornet(newID, x.Value, "temp");
                        LobbyManager.LobbyPlayers.Add(newID, script);
                    }
                }
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("No response from server after 2 seconds!");
                InviteButtonScript.CreateErrorLayout(
                    SilksongModPlugin.canvas.gameObject,
                    "Server is not on right now. Please connect via Steam, or try again later."
                );
            }

            // Reset the TCS so it can be reused later
            lobbyResponseTcs = null;
        }

        public void ProcessLobbyDict(Dictionary<ulong, string> dict)
        {
            byte[] singlePlayerData = Serializer.SerializeSinglePlayer(LobbyManager.CurrSteamID, LobbyManager.CurrName,
                LobbyManager.CurrScene);
            foreach (var x in dict)
            {
                CSteamID newID = new CSteamID(x.Key);
                if (newID != LobbyManager.CurrSteamID)
                {
                    SilksongModPlugin.Log.LogInfo($"joining player in lobby: {x.Key}: {x.Value}");
                    SteamP2PSender.SendData(newID, singlePlayerData, P2PChannel.Lobby);
                    SyncedHornetScript script = LobbyManager.CreateHornet(newID, x.Value, "temp");
                    LobbyManager.LobbyPlayers.Add(newID, script);
                }
            }
        }

        public async Task LeaveGlobalLobby(CSteamID steamID, string name)
        {
            SilksongModPlugin.Log.LogInfo("Called leavegloballobby method");
            byte[] data = Serializer.SteamIDNameToBytes(UDPCommand.LeaveGlobalLobby, steamID.m_SteamID, name);
            await udpClient.SendAsync(data, data.Length);
        }
    }
}