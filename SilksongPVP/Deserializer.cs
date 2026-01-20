using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GlobalEnums;
using SilksongMod.Enums;
using SilksongMod.SteamP2P;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace SilksongMod
{
    public static class Deserializer
    {
        public static void RecieveAnimData(byte[] data, CSteamID sender)
        {
            AnimType type =  (AnimType)data[0];
            if (type == AnimType.NailAttack)
            {
                DeserializeNailAttack(data, sender);
                return;
            }
            RPCMethod method = (RPCMethod)data[1];
            tk2dSpriteAnimator animator;
            //SilksongModPlugin.Log.LogInfo($"Size of synced hornet: {LobbyManager.LobbyPlayers.Count}");
            if (LobbyManager.LobbyPlayers.TryGetValue(sender, out var hornet))
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
                default:
                    SilksongModPlugin.Log.LogError("Error: Unknown RPC Method");
                    break;
            }
        }

        public static void RecieveAttackData(byte[] data, CSteamID sender)
        {
            if (data.Length == 1 && data[0] == (byte)LobbyCommand.Ping) // if recieved hit confirmation
            {
                if (LobbyManager.LobbyPlayers.TryGetValue(sender, out var hornet))
                {
                   // SilksongModPlugin.Log.LogInfo("Received hit confirmation! showing animation");
                    hornet.ShowHitAnim();
                }
                else
                {
                    SilksongModPlugin.Log.LogError("Piing in recieveattackdata sender not there OOPS");
                }

                return;
            }
            byte masks = data[0];
            byte direction = data[1];
            CollisionSide x = CollisionSide.left;
            if (direction == 1) // right collider
            {
                x = CollisionSide.right;
            }
            LobbyManager.HeroTakeDamage(masks, x, sender, true);
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
                Vector2 velo = reader.ReadVector2();
                Vector2 colliderOffset = reader.ReadVector2();
                Vector2 colliderSize = reader.ReadVector2();
                return new PlayerPosData(pos, scale, velo, colliderOffset, colliderSize);
            }
        }
        
        private static Vector3 ReadVector3(this BinaryReader reader)
        {
            float x1 = reader.ReadSingle();
            float y1 = reader.ReadSingle();
            float z1 = reader.ReadSingle();
            return new Vector3(x1, y1, z1);
        }
        private static Vector3 ReadVector2(this BinaryReader reader)
        {
            float x1 = reader.ReadSingle();
            float y1 = reader.ReadSingle();
            return new Vector2(x1, y1);
        }


        private static void DeserializePlay(byte[] data, tk2dSpriteAnimator animator, int offset = 2)
        {
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

        private static void DeserializeNailAttack(byte[] data, CSteamID sender)
        {
            NailAttackType type = (NailAttackType)data[1];
            if (LobbyManager.LobbyPlayers.TryGetValue(sender, out var hornet))
            {
                if (type == NailAttackType.NailAttackEnable)
                {
                    byte index = data[2];
                    byte active = data[3];
                    hornet.ActivateNailAttack(index, active != 0);
                }
                else if (type == NailAttackType.Anim)
                {
                    byte index = data[2];
                    tk2dSpriteAnimator animator = hornet.RetrieveAnimatorFromIndex(index);
                    if (animator)
                    {
                        RPCMethod method = (RPCMethod)data[3];
                        if (method == RPCMethod.Play)
                        {
                            DeserializePlay(data, animator, 4);
                        }
                        else if (method == RPCMethod.Stop)
                        {
                            DeserializeStop(animator);
                        }
                    }
                    else
                    {
                        SilksongModPlugin.Log.LogError("Deserialize ERROR: Animator is null!!!");
                    }
                }
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("sender doesn't exist in dictionary what happened ._.");
            }
        }

        public static void RecieveLobbyData(byte[] data, CSteamID sender)
        {
            LobbyCommand lobbyCommand = (LobbyCommand)data[0];
            SilksongModPlugin.Log.LogInfo($"Recieved with Lobby Command {lobbyCommand}");
            if (lobbyCommand == LobbyCommand.LobbyDictToJoin)
            {
                Dictionary<CSteamID, string> result = DeserializeDictionary(data);
                LobbyManager.PendingLobbyBuffer = result; //set this temporarily so it doesn't go away
                LobbyManager.CreateJoin(sender, result); // create the join
            }
            else if (lobbyCommand == LobbyCommand.PlayerJoined)
            {
                SilksongModPlugin.Log.LogInfo($"Recieved player join lobby command from SteamID {sender}");
                (CSteamID, string, string) playerData = DeserializeSinglePlayerData(data); 
                LobbyManager.AddPlayerToLobby(playerData);
                //next, send your current scene over to that person who you've just added
                SteamP2PSender.SendCurrSceneToPlayer(sender, LobbyManager.CurrScene);
            }
            else if (lobbyCommand == LobbyCommand.SceneChange)
            {
                string scene = DeserializeScene(data);
                LobbyManager.UpdateSceneForPlayer(sender, scene);
            }
            else if (lobbyCommand == LobbyCommand.LeaveLobby)
            {
                LobbyManager.LeaveRecievedFromPlayer(sender);
            }
            else if (lobbyCommand == LobbyCommand.Message)
            {
                (string name, string msg) = DeserializeMessage(data);
                ChatDisplay.AddPlayerText(name, msg);
                SilksongModPlugin.Log.LogInfo("received message!");
            }
        }

        //currently only holding steam id and name
        private static Dictionary<CSteamID, string> DeserializeDictionary(byte[] data)
        {
            var result = new Dictionary<CSteamID, string>();

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
                    CSteamID steamID = new CSteamID(steamIdRaw);

                    // 2️⃣ Name
                    string name = reader.ReadString();
                    
                    result[steamID] = name;
                }
            }

            return result;
        }
        
        private static (CSteamID, string, string) DeserializeSinglePlayerData(byte[] data)
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
                return (steamId, name, scene);
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

        public static Dictionary<ulong, string> DeserializeLobbyPlayerDict(byte[] data)
        {
            var dict = new Dictionary<ulong, string>();
            int offset = 0;

            while (offset < data.Length)
            {
                if (offset + 8 > data.Length)
                    throw new Exception("Unexpected end of data while reading SteamID");

                // Read 8-byte SteamID (little-endian)
                ulong steamID = BitConverter.ToUInt64(data, offset);
                offset += 8;

                if (offset >= data.Length)
                    throw new Exception("Unexpected end of data while reading name length");

                // Read 1-byte name length
                byte nameLen = data[offset];
                offset += 1;

                if (offset + nameLen > data.Length)
                    throw new Exception("Unexpected end of data while reading name");

                // Read name bytes and decode
                string name = Encoding.UTF8.GetString(data, offset, nameLen);
                offset += nameLen;

                dict[steamID] = name;
            }

            return dict;
        }
        
        public static (string name, string message) DeserializeMessage(byte[] data)
        {
            if (data == null || data.Length < 3)
                throw new ArgumentException("Data is null or too short.");

            int offset = 0;

            // 1️⃣ Skip header
            offset++;

            // 2️⃣ Read name length (2 bytes, big-endian)
            if (data.Length < offset + 2)
                throw new ArgumentException("Data too short to contain name length.");

            ushort nameLength = (ushort)((data[offset] << 8) | data[offset + 1]);
            offset += 2;

            if (data.Length < offset + nameLength)
                throw new ArgumentException("Data too short for name.");

            // 3️⃣ Read name string
            string name = System.Text.Encoding.UTF8.GetString(data, offset, nameLength);
            offset += nameLength;

            // 4️⃣ Read message string (rest of data)
            string message = System.Text.Encoding.UTF8.GetString(data, offset, data.Length - offset);

            return (name, message);
        }

    }
}