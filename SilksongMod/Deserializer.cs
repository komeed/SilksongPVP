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
            byte masks = data[0];
            byte direction = data[1];
            CollisionSide x = CollisionSide.left;
            if (direction == 1) // right collider
            {
                x = CollisionSide.right;
            }
            LobbyManager.HeroController.TakeDamage(null, x, masks, HazardType.ENEMY);
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

    }
}