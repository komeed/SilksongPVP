using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SilksongMod.Enums;
using Steamworks;
using UnityEngine;

namespace SilksongMod
{
    public static class Serializer
    {
        public static byte[] SerializePlay(string clipName, float clipTime, float fps)
        {
            // Encode string as UTF8
            byte[] stringBytes = Encoding.UTF8.GetBytes(clipName);
            if (stringBytes.Length > ushort.MaxValue)
                throw new Exception("Clip name too long!");

            byte[] data = new byte[1 + 2 + stringBytes.Length + 4 + 4];

            int offset = 0;

            // 1. Method enum
            data[offset++] = (byte)RPCMethod.Play;

            // 3. String length (ushort)
            ushort len = (ushort)stringBytes.Length;
            data[offset++] = (byte)(len >> 8);   // high byte
            data[offset++] = (byte)(len & 0xFF); // low byte

            // 4. String bytes
            Buffer.BlockCopy(stringBytes, 0, data, offset, stringBytes.Length);
            offset += stringBytes.Length;

            // 5. clipTime (float)
            Buffer.BlockCopy(BitConverter.GetBytes(clipTime), 0, data, offset, 4);
            offset += 4;

            // 6. fps (float)
            Buffer.BlockCopy(BitConverter.GetBytes(fps), 0, data, offset, 4);

            return data;
        }

        public static byte[] SerializeStop()
        {
            byte[] data = new byte[1];

            // 1. Method enum
            data[0] = (byte)RPCMethod.Stop;

            return data;
        }
        
        public static byte[] SerializeLobbyInfo(Dictionary<SteamPlayer, string> dict)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    // Packet type header (your enum)
                    writer.Write((byte)LobbyCommand.LobbyDict);

                    // Number of entries
                    writer.Write(dict.Count);

                    // Write each SteamPlayer + associated string
                    foreach (KeyValuePair<SteamPlayer, string> kvp in dict)
                    {
                        SteamPlayer player = kvp.Key;

                        // 1️⃣ SteamID
                        writer.Write(player.SteamID.m_SteamID);

                        // 2️⃣ Name
                        writer.Write(player.Name ?? string.Empty);

                        // 3️⃣ Extra string stored in dictionary (e.g., role / status)
                        writer.Write(kvp.Value ?? string.Empty);
                    }
                }

                return ms.ToArray();
            }
        }

        public static byte[] SerializeSinglePlayer(SteamPlayer player, string currScene)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // 1️⃣ Write packet type header (1 byte)
                writer.Write((byte)LobbyCommand.PlayerJoined);

                // 2️⃣ Write SteamID (ulong = 8 bytes)
                writer.Write(player.SteamID.m_SteamID);

                // 3️⃣ Write player name (string, variable length)
                writer.Write(player.Name ?? string.Empty);
                
                // Write player current scene (string)
                writer.Write(currScene);

                // Return the byte array
                return ms.ToArray();
            }
        }
        
        public static byte[] SerializeScene(string scene)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)LobbyCommand.SceneChange);          // 1-byte header
                writer.Write(scene ?? string.Empty);
                
                return ms.ToArray();
            }
        }

        public static byte[] SerializePlayerPosData(PlayerPosData posData)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(posData.Position);
                writer.Write(posData.LocalScale);
                writer.Write(posData.Velocity);
                
                return ms.ToArray();
            }
        }
        
        private static void Write(this BinaryWriter writer, Vector3 v)
        {
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }

        public static byte[] SerializeLeaveLobby(SteamPlayer player)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // 1️⃣ Write packet type header (1 byte)
                writer.Write((byte)LobbyCommand.LeaveLobby);

                // 2️⃣ Write SteamID (ulong = 8 bytes)
                writer.Write(player.SteamID.m_SteamID);

                // 3️⃣ Write player name (string, variable length)
                writer.Write(player.Name ?? string.Empty);

                // Return the byte array
                return ms.ToArray();
            }
        }
        //public static byte[] 
    }
}