using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;
using Steamworks;
using UnityEngine;

namespace SilksongMod
{
    public static class Serializer
    {
        public static byte[] SerializePlay(AnimType animType, string clipName, float clipTime, float fps)
        {
            // Encode string as UTF8
            byte[] stringBytes = Encoding.UTF8.GetBytes(clipName);
            if (stringBytes.Length > ushort.MaxValue)
                throw new Exception("Clip name too long!");

            byte[] data = new byte[1 + 1 + 2 + stringBytes.Length + 4 + 4];

            int offset = 0;
            data[offset++] = (byte)animType;

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
        
        public static byte[] SerializeNailPlay(int index, string clipName, float clipTime, float fps)
        {
            // Encode string as UTF8
            byte[] stringBytes = Encoding.UTF8.GetBytes(clipName);
            if (stringBytes.Length > ushort.MaxValue)
                throw new Exception("Clip name too long!");

            byte[] data = new byte[1 + 1 + 1 + 1 + 2 + stringBytes.Length + 4 + 4];

            int offset = 0;
            data[offset++] = (byte)AnimType.NailAttack;
            data[offset++] = (byte)NailAttackType.Anim;
            data[offset++] = (byte)index;

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

        public static byte[] SerializeStop(AnimType animType)
        {
            byte[] data = new byte[2];

            data[0] = (byte)animType;
            // 1. Method enum
            data[1] = (byte)RPCMethod.Stop;

            return data;
        }
        public static byte[] SerializeNailStop(AnimType animType, int index)
        {
            byte[] data = new byte[3];

            data[0] = (byte)animType;
            data[1] = (byte)NailAttackType.Anim;
            data[2] = (byte)index;
            // 1. Method enum
            data[3] = (byte)RPCMethod.Stop;

            return data;
        }
        
        public static byte[] SerializeLobbyInfo(Dictionary<CSteamID, SyncedHornetScript> dict)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    // Packet type header (your enum)
                    writer.Write((byte)LobbyCommand.LobbyDict);

                    // Number of entries
                    writer.Write(dict.Count + 1); // include the current player
                    
                    //Write the current player's steam id and name (so we know the sending player's name)
                    writer.Write(LobbyManager.CurrSteamID.m_SteamID);
                    
                    writer.Write(LobbyManager.CurrName);

                    // Write each SteamPlayer + associated string
                    foreach (SyncedHornetScript script in dict.Values)
                    {
                        // 1️⃣ SteamID
                        writer.Write(script.steamID.m_SteamID);

                        // 2️⃣ Name
                        writer.Write(script.name ?? string.Empty);
                    }
                }

                return ms.ToArray();
            }
        }

        public static byte[] SerializeSinglePlayer(CSteamID steamID, string name, string currScene)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // 1️⃣ Write packet type header (1 byte)
                writer.Write((byte)LobbyCommand.PlayerJoined);

                // 2️⃣ Write SteamID (ulong = 8 bytes)
                writer.Write(steamID.m_SteamID);

                // 3️⃣ Write player name (string, variable length)
                writer.Write(name ?? string.Empty);
                
                // Write player current scene (string)
                writer.Write(currScene);

                // Return the byte array
                return ms.ToArray();
            }
        }
        
        public static byte[] SteamIDNameToBytes(UDPCommand command, ulong steamID, string name)
        {
            // Convert command to a single bytea
            byte commandByte = (byte)command;

            // Convert SteamID to 8 bytes (little-endian)
            byte[] steamIDBytes = BitConverter.GetBytes(steamID);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(steamIDBytes);
            }

            // Convert name to UTF-8 bytes
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            // Allocate result array: 1 byte command + 8 bytes SteamID + name bytes
            byte[] result = new byte[1 + steamIDBytes.Length + nameBytes.Length];

            // Fill array
            result[0] = commandByte;
            Buffer.BlockCopy(steamIDBytes, 0, result, 1, steamIDBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, result, 1 + steamIDBytes.Length, nameBytes.Length);

            return result;
        }

        public static byte[] SerializeSteamID(UDPCommand command, ulong steamID)
        {
            byte[] buffer = new byte[1 + 8]; // 1 byte header + 8 bytes SteamID

            buffer[0] = (byte)command;

            // SteamID -> bytes (little-endian)
            byte[] steamBytes = BitConverter.GetBytes(steamID);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(steamBytes);

            Buffer.BlockCopy(steamBytes, 0, buffer, 1, 8);

            return buffer;
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
                writer.Write(posData.ColliderOffset);
                writer.Write(posData.ColliderSize);
                return ms.ToArray();
            }
        }
        
        private static void Write(this BinaryWriter writer, Vector3 v)
        {
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }
        private static void Write(this BinaryWriter writer, Vector2 v)
        {
            writer.Write(v.x);
            writer.Write(v.y);
        }

        public static byte[] SerializeLeaveLobby()
        {
            return new byte[1] {(byte)LobbyCommand.LeaveLobby};
        }

        public static byte[] SerializeMessage(string name, string message)
        {
            // 1️⃣ Convert strings to UTF-8 bytes
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            if (nameBytes.Length > ushort.MaxValue)
                throw new ArgumentException("Name is too long to serialize.");

            // 2️⃣ Allocate array: 1 byte header + 2 bytes name length + name + message
            byte[] result = new byte[1 + 2 + nameBytes.Length + messageBytes.Length];

            int offset = 0;

            // 3️⃣ Header
            result[offset++] = (byte)LobbyCommand.Message;

            // 4️⃣ Name length (2 bytes, big-endian)
            ushort nameLength = (ushort)nameBytes.Length;
            result[offset++] = (byte)((nameLength >> 8) & 0xFF);
            result[offset++] = (byte)(nameLength & 0xFF);

            // 5️⃣ Copy name bytes
            System.Buffer.BlockCopy(nameBytes, 0, result, offset, nameBytes.Length);
            offset += nameBytes.Length;

            // 6️⃣ Copy message bytes
            System.Buffer.BlockCopy(messageBytes, 0, result, offset, messageBytes.Length);

            return result;
        }


        //public static byte[] 
    }
}