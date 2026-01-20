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

        public static void SendLobbyDataToJoin(CSteamID target)
        {
            byte[] data = Serializer.SerializeLobbyInfo(LobbyManager.LobbyPlayers);
            data[0] = (byte)LobbyCommand.LobbyDictToJoin; // set the first byte to be joining dict send

            if (SendData(target, data, P2PChannel.Lobby))
            {
             //   SilksongModPlugin.Log.LogInfo($"Successfully sent Lobby for {target} to join!");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"Failed to send Lobby for {target} to join.");
            }
        }

        public static void SendCurrSceneToPlayer(CSteamID player, string currScene)
        {
            byte[] data = Serializer.SerializeScene(currScene);
            if (SendData(player, data, P2PChannel.Lobby))
            {
                //SilksongModPlugin.Log.LogInfo($"Successfully sent scene change to Player {player}");
            }
            else
            {
                SilksongModPlugin.Log.LogError($"Failed to send scene change to Player {player}");
            }
        }
    }
}