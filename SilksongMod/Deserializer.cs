using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SilksongMod.Enums;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongMod
{
    public static class Deserializer
    {
        public static void RecieveAnimData(byte[] data, CSteamID sender)
        {
            RPCMethod method = (RPCMethod)data[0];
            tk2dSpriteAnimator animator;
            SilksongModPlugin.Log.LogInfo($"Size of synced hornet: {LobbyManager.SyncedHornetScripts.Count}");
            if (LobbyManager.SyncedHornetScripts.TryGetValue(sender, out var hornet))
            {
                animator = hornet.animator;
            }
            else
            {
                SilksongModPlugin.Log.LogError("RecieveAnimData Error: sender not present in players dictionary");
                return;
            }
            
            switch (method)
            {
                case RPCMethod.Play:
                    DeserializePlay(data, animator);
                    break;
                case RPCMethod.Stop:
                    DeserializeStop(animator);
                    break;
                case RPCMethod.NailAttack:
                    
                default:
                    SilksongModPlugin.Log.LogError("Error: Unknown RPC Method");
                    break;
            }
        }

        public static void RecievePosData(byte[] data, CSteamID sender)
        {
            PlayerPosData pos = DeserializePlayerPosData(data);
            LobbyManager.UpdateSyncedHornetPos(sender, pos);
        }

        public static PlayerPosData DeserializePlayerPosData(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Vector3 pos = reader.ReadVector3();
                Vector3 scale = reader.ReadVector3();
                Vector3 velo = reader.ReadVector3();
                return new PlayerPosData(pos, scale, velo);
            }
        }
        
        private static Vector3 ReadVector3(this BinaryReader reader)
        {
            float x1 = reader.ReadSingle();
            float y1 = reader.ReadSingle();
            float z1 = reader.ReadSingle();
            return new Vector3(x1, y1, z1);
        }


        private static void DeserializePlay(byte[] data, tk2dSpriteAnimator animator)
        {
            int offset = 1; // account for first 5 bytes

            // 3. String length (ushort)
            ushort stringLength = (ushort)((data[offset++] << 8) | data[offset++]);

            // 4. String bytes
            string clipName = Encoding.UTF8.GetString(data, offset, stringLength);
            offset += stringLength;

            // 5. clipTime (float)
            float clipTime = BitConverter.ToSingle(data, offset);
            offset += 4;

            // 6. fps (float)
            float fps = BitConverter.ToSingle(data, offset);
            
            tk2dSpriteAnimationClip clip = animator.GetClipByName(clipName);
            animator.Play(clip, clipTime, fps);
        }

        private static void DeserializeStop(tk2dSpriteAnimator animator)
        {
            animator.Stop();
        }

        private static void DeserializeNailAttack(byte[] data)
        {
            
        }

        public static void RecieveLobbyData(byte[] data, CSteamID sender)
        {
            LobbyCommand lobbyCommand = (LobbyCommand)data[0];
            SilksongModPlugin.Log.LogInfo($"Recieved with Lobby Command {lobbyCommand}");
            if (lobbyCommand == LobbyCommand.LobbyDictToJoin)
            {
                Dictionary<SteamPlayer, string> result = DeserializeDictionary(data);
                LobbyManager.CreateJoin(new SteamPlayer(SteamFriends.GetFriendPersonaName(sender), sender)); // create the join
                LobbyManager.PendingLobbyBuffer = result; //set this temporarily so it doesn't go away
            }
            else if (lobbyCommand == LobbyCommand.LobbyDict)
            {
                Dictionary<SteamPlayer, string> result = DeserializeDictionary(data);
                LobbyManager.MoveToNewLobby(result);
            }
            else if (lobbyCommand == LobbyCommand.PlayerJoined)
            {
                SilksongModPlugin.Log.LogInfo($"Recieved player join lobby command from SteamID {sender}");
                KeyValuePair<SteamPlayer, string> playerData = DeserializeSinglePlayerData(data);
                LobbyManager.AddPlayerToLobby(playerData);
            }
            else if (lobbyCommand == LobbyCommand.SceneChange)
            {
                string scene = DeserializeScene(data);
                LobbyManager.UpdateSceneForPlayer(new SteamPlayer("temp", sender), scene);
            }
            else if (lobbyCommand == LobbyCommand.LeaveLobby)
            {
                SteamPlayer playerLeft = DeserializeLeaveLobby(data);
                LobbyManager.LeaveRecievedFromPlayer(playerLeft);
            }
        }

        private static Dictionary<SteamPlayer, string> DeserializeDictionary(byte[] data)
        {
            var result = new Dictionary<SteamPlayer, string>();

            // Skip the first byte (header) if needed
            using (MemoryStream ms = new MemoryStream(data, 1, data.Length - 1))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // Read the number of players
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    // 1️⃣ SteamID
                    ulong steamIdRaw = reader.ReadUInt64();
                    CSteamID steamId = new CSteamID(steamIdRaw);

                    // 2️⃣ Name
                    string name = reader.ReadString();

                    // 3️⃣ Extra string stored in the dictionary
                    string extraValue = reader.ReadString();

                    // Create SteamPlayer and add to dictionary
                    var player = new SteamPlayer(name, steamId);
                    result[player] = extraValue;
                }
            }

            return result;
        }
        
        private static KeyValuePair<SteamPlayer, string> DeserializeSinglePlayerData(byte[] data)
        {
            // Skip the first byte (header)
            using (MemoryStream ms = new MemoryStream(data, 1, data.Length - 1))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // 1️⃣ Read SteamID (ulong)
                ulong steamIdRaw = reader.ReadUInt64();
                CSteamID steamId = new CSteamID(steamIdRaw);

                // 2️⃣ Read player name (string)
                string name = reader.ReadString();
                
                // Read Player Scene
                string scene = reader.ReadString();

                // 3️⃣ Construct SteamPlayer
                return new KeyValuePair<SteamPlayer, string>(new SteamPlayer(name, steamId), scene);
            }
        }
        
        private static string DeserializeScene(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // Skip 1-byte LobbyCommand.SceneChange header
                reader.ReadByte();

                // Read scene name
                return reader.ReadString();
            }
        }

        private static SteamPlayer DeserializeLeaveLobby(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data, 1, data.Length - 1))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // 1️⃣ Read SteamID (ulong)
                ulong steamIdRaw = reader.ReadUInt64();
                CSteamID steamId = new CSteamID(steamIdRaw);

                // 2️⃣ Read player name (string)
                string name = reader.ReadString();

                // 3️⃣ Construct SteamPlayer
                return new SteamPlayer(name, steamId);
            }
        }

    }
}