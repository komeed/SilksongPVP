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
        public void Init(string remoteIP, int remotePort, bool isServer)
        {
            this.remoteIP = remoteIP;
            this.remotePort = remotePort;
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(remoteIP), remotePort);
            this.isServer = isServer;
      //      StartListening();
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
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    SilksongModPlugin.Log.LogInfo($"Received from {result.RemoteEndPoint}: {message}");
                    //HandleMessage(message);
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
        
        public async Task SendPingAndMessage(byte[] messageData)
        {
            if (udpClient == null)
            { 
                SilksongModPlugin.Log.LogInfo("UDP client not initialized!");
                return;
            }

            // 1️⃣ Send ping
            byte[] pingData = Encoding.UTF8.GetBytes("ping");
            udpClient.Send(pingData, pingData.Length);
            SilksongModPlugin.Log.LogInfo($"Ping sent: ping");

            // 2️⃣ Wait for response (~1 second)
            UdpReceiveResult? result = await ReceiveWithTimeout(1000); // 1000ms = 1 sec

            if (result.HasValue)
            {
                string reply = Encoding.UTF8.GetString(result.Value.Buffer);
                SilksongModPlugin.Log.LogInfo($"Ping reply received from {result.Value.RemoteEndPoint}: {reply}");

                // 3️⃣ Send the actual message
                udpClient.Send(messageData, messageData.Length);
                SilksongModPlugin.Log.LogInfo($"Message sent: {messageData.Length} bytes");
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("No ping reply received. Message not sent.");
                if (isServer)
                {
                    SilksongModPlugin.Log.LogInfo("no ping reply received from server. Server is not working right now.");
                //    InviteButtonScript.CreateErrorLayout(SilksongModPlugin.canvas.gameObject, "Server is not on right now. Please connect via steam, or try again later.");
                }
            }
        }

        //this works itself like a ping/pong: after recieving back message, it sends the rest of the lobby (otherwise no)

        public async Task JoinGlobalLobby(CSteamID steamID, string name)
        {
            SilksongModPlugin.Log.LogInfo("Called joingloballobby method");
            byte[] data = Serializer.SteamIDNameToBytes(UDPCommand.JoinGlobalLobby, steamID.m_SteamID, name);
            await udpClient.SendAsync(data, data.Length);
            SilksongModPlugin.Log.LogInfo("sent data");
            // Start waiting for response
            Task<UdpReceiveResult> receiveTask = udpClient.ReceiveAsync();
            Task delayTask = Task.Delay(1000); // 1 second timeout
            SilksongModPlugin.Log.LogInfo("Reached here");

            // Wait for either receive or timeout
            Task finishedTask = await Task.WhenAny(receiveTask, delayTask);
            SilksongModPlugin.Log.LogInfo("and here");
            if (finishedTask == receiveTask)
            {

                // Response received
                UdpReceiveResult result = receiveTask.Result;

                SilksongModPlugin.Log.LogInfo("Received response from server");
                byte[] buffer = result.Buffer;
                Dictionary<ulong, string> dict = Deserializer.DeserializeLobbyPlayerDict(buffer);
                byte[] singlePlayerData = Serializer.SerializeSinglePlayer(LobbyManager.CurrSteamID, LobbyManager.CurrName, LobbyManager.CurrScene);
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
            else
            {
                SilksongModPlugin.Log.LogInfo("no ping reply received from server. Server is not working right now.");
                InviteButtonScript.CreateErrorLayout(SilksongModPlugin.canvas.gameObject, 
                    "Server is not on right now. Please connect via steam, or try again later.");
            }
        }

        public async Task LeaveGlobalLobby(CSteamID steamID, string name)
        {
            SilksongModPlugin.Log.LogInfo("Called leavegloballobby method");
            byte[] data = Serializer.SteamIDNameToBytes(UDPCommand.LeaveGlobalLobby, steamID.m_SteamID, name);
            await udpClient.SendAsync(data, data.Length);
            // Start waiting for response
            Task<UdpReceiveResult> receiveTask = udpClient.ReceiveAsync();
            Task delayTask = Task.Delay(1000); // 1 second timeout

            // Wait for either receive or timeout
            Task finishedTask = await Task.WhenAny(receiveTask, delayTask);
            if (finishedTask == receiveTask)
            {
                UdpReceiveResult result = receiveTask.Result;
                SilksongModPlugin.Log.LogInfo("Received response from server");
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("no ping reply received from server. Server is not working right now.");
                //InviteButtonScript.CreateErrorLayout(SilksongModPlugin.canvas.gameObject, 
                 //   "Server is not on right now. Please connect via steam, or try again later.");
            }
        }
        
        public async Task SendLobbyMessage(UDPCommand command, string lobbyName, byte[] messageData) {
            // Encode lobby name
            byte[] lobbyNameBytes = Encoding.UTF8.GetBytes(lobbyName);
            ushort nameLen = (ushort)lobbyNameBytes.Length;
            // Allocate: 1 byte command + lobby name + message data
            byte[] message = new byte[1 + 2 + nameLen + messageData.Length];

            int offset = 0;
            message[offset++] = (byte)command;
            // Name length (little-endian)
            message[offset++] = (byte)(nameLen & 0xFF);
            message[offset++] = (byte)((nameLen >> 8) & 0xFF);
            
            Buffer.BlockCopy(lobbyNameBytes, 0, message, offset, lobbyNameBytes.Length);
            offset += nameLen;
            
            Buffer.BlockCopy(messageData, 0, message, offset, messageData.Length);
            // Send
            await udpClient.SendAsync(message, message.Length);

            // Start waiting for response
            Task<UdpReceiveResult> receiveTask = udpClient.ReceiveAsync();
            Task delayTask = Task.Delay(1000); // 1 second timeout

            // Wait for either receive or timeout
            Task finishedTask = await Task.WhenAny(receiveTask, delayTask);

            if (finishedTask == receiveTask)
            {
                // Response received
                UdpReceiveResult result = receiveTask.Result;

                SilksongModPlugin.Log.LogInfo("Received response from server");
                byte[] buffer = result.Buffer;
                if (buffer[0] != 0) // 0 is error, 1 is good
                {
                    // I was thinking to send the current users that are in the lobby that aren't steam,
                    // but then I realized that this is never possible.
                    // so let's just skip this for now.
                }
                else
                {
                  //  SilksongModPlugin.Log.LogInfo("Server encountered an error!");
                  //  InviteButtonScript.CreateErrorLayout(SilksongModPlugin.canvas.gameObject, 
                      //  "Server encountered an error. Please connect via steam, or try again later.");
                }
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("no ping reply received from server. Server is not working right now.");
            //    InviteButtonScript.CreateErrorLayout(SilksongModPlugin.canvas.gameObject, 
                    //"Server is not on right now. Please connect via steam, or try again later.");
            }
        }
        
        
        private async Task<UdpReceiveResult?> ReceiveWithTimeout(int millisecondsTimeout)
        {
            var receiveTask = udpClient.ReceiveAsync();
            var delayTask = Task.Delay(millisecondsTimeout);

            var completedTask = await Task.WhenAny(receiveTask, delayTask);

            if (completedTask == receiveTask)
            {
                return receiveTask.Result;
            }

            // Timeout
            return null;
        }
    }

}