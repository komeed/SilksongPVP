using System;
using Steamworks;
using UnityEngine;

namespace SilksongMod.SteamP2P
{
    public class SteamP2PReceiver : MonoBehaviour
    {
        Callback<P2PSessionRequest_t> sessionRequest;
        Callback<P2PSessionConnectFail_t> p2pFailCallback;
        private byte[] buffer = Array.Empty<byte>();

        void Start()
        {
            sessionRequest = Callback<P2PSessionRequest_t>.Create(OnSessionRequest);
            p2pFailCallback = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
        }

        void OnSessionRequest(P2PSessionRequest_t req)
        {
            CSteamID sender = req.m_steamIDRemote;
            string name = GetNameFromSteamID(req.m_steamIDRemote);
            SteamNetworking.AcceptP2PSessionWithUser(sender); // immediately accept, but show join when lobby received
            
            SilksongModPlugin.Log.LogInfo($"Accepted {name}'s Session Request!");
        }
        
        private void OnP2PSessionConnectFail(P2PSessionConnectFail_t fail)
        {
            SilksongModPlugin.Log.LogInfo($"P2P session failed with user {fail.m_steamIDRemote}");
            SilksongModPlugin.Log.LogInfo($"Reason: {fail.m_eP2PSessionError}");
        }

        void Update()
        {
            uint size;
            while (SteamNetworking.IsP2PPacketAvailable(out size, (byte)P2PChannel.Lobby)) // read Lobby data
            {
                if (buffer.Length != size)
                {
                    buffer = new byte[size];
                }
                CSteamID sender;
                SteamNetworking.ReadP2PPacket(buffer, size, out size, out sender, (byte)P2PChannel.Lobby);
                SilksongModPlugin.Log.LogInfo($"Received {size} bytes from SteamID {sender}");
                Deserializer.RecieveLobbyData(buffer, sender);
            }
            
            while (SteamNetworking.IsP2PPacketAvailable(out size, (byte)P2PChannel.Pos)) // read Anim data
            {
                if (buffer.Length != size)
                {
                    buffer = new byte[size];
                }
                CSteamID sender;
                SteamNetworking.ReadP2PPacket(buffer, size, out size, out sender, (byte)P2PChannel.Pos);
                Deserializer.RecievePosData(buffer, sender);
                SilksongModPlugin.Log.LogInfo($"Received {size} bytes from {sender}");
            }
            
            while (SteamNetworking.IsP2PPacketAvailable(out size, (byte)P2PChannel.Anim)) // read Anim data
            {
                if (buffer.Length != size)
                {
                    buffer = new byte[size];
                }
                CSteamID sender;
                SteamNetworking.ReadP2PPacket(buffer, size, out size, out sender, (byte)P2PChannel.Anim);
                Deserializer.RecieveAnimData(buffer, sender);
                SilksongModPlugin.Log.LogInfo($"Received {size} bytes from {sender}");
            }
        }
        
        public static string GetNameFromSteamID(CSteamID steamID)
        {
            if (steamID == CSteamID.Nil)
                return $"unknown ({steamID})";

            try
            {
                // Try to get persona name (friend name / display name)
                string name = SteamFriends.GetFriendPersonaName(steamID);

                // Fallback if empty or null
                if (string.IsNullOrEmpty(name))
                    return $"unknown ({steamID})";

                return name;
            }
            catch
            {
                return $"unknown ({steamID})";
            }
        }
    }

}