using System.Text;
using Steamworks;
using System.Collections.Generic;
using SilksongMod.Enums;

namespace SilksongMod.SteamP2P
{
    public static class SteamP2PSender
    {
        public static bool SendData(CSteamID target, byte[] data, P2PChannel channel)
        {
            bool success = SteamNetworking.SendP2PPacket(
                target,
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable, // or Unreliable
                (int)channel
            );
            return success;
        }

        public static void SendLobbyData(SteamPlayer target, Dictionary<SteamPlayer, string> lobby)
        {
            byte[] data = Serializer.SerializeLobbyInfo(lobby);
            
            SendData(target.SteamID, data, P2PChannel.Lobby);
        }

        public static void SendLobbyDataToJoin(SteamPlayer target, Dictionary<SteamPlayer, string> lobby)
        {
            byte[] data = Serializer.SerializeLobbyInfo(lobby);
            data[0] = (byte)LobbyCommand.LobbyDictToJoin; // set the first byte to be joining dict send

            if (SendData(target.SteamID, data, P2PChannel.Lobby))
            {
                SilksongModPlugin.Log.LogInfo($"Successfully sent Lobby for {target.Name} to join!");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"Failed to send Lobby for {target.Name} to join.");
            }
        }

        public static void SendPlayerJoinConfirmation(CSteamID target, SteamPlayer currPlayer, string currScene)
        {
            byte[] data = Serializer.SerializeSinglePlayer(currPlayer, currScene);
            if (SendData(target, data, P2PChannel.Lobby))
            {
                SilksongModPlugin.Log.LogInfo($"Successfully sent join confirmation request to Steam ID {target}");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"Failed to send join confirmation request to Steam ID {target}");
            }
        }

        public static void SendCurrSceneToPlayer(SteamPlayer player, string currScene)
        {
            byte[] data = Serializer.SerializeScene(currScene);
            if (SendData(player.SteamID, data, P2PChannel.Lobby))
            {
                SilksongModPlugin.Log.LogInfo($"Successfully sent scene change to Player {player.Name}");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"Failed to send scene change to Player {player.Name}");
            }
        }

        public static void SendPositionDataTo(SteamPlayer player, byte[] data)
        {
            if (SendData(player.SteamID, data, P2PChannel.Pos))
            {
                //SilksongModPlugin.Log.LogInfo($"Successfully sent position data to Player {player.Name}");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"Failed to send position data to Player {player.Name}");
            }
        }
    }
}