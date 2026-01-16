using System;
using System.Collections.Generic;
using GlobalEnums;
using SilksongMod.SteamP2P;
using Steamworks;
using UnityEngine;
using Object = System.Object;

//using UnityEngine.Windows;

namespace SilksongMod
{
    public class SyncedHornetScript : MonoBehaviour
    {
        public CSteamID steamID;
        public string name; // for later name above hornet implementation
        
        public tk2dSpriteAnimator animator;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;

        private GameObject[] NailAttacks;

        private void Awake()
        {
            SilksongModPlugin.Log.LogInfo("SyncedHornetScript: Awake");
            if (LobbyManager.HostHornet == null)
            {
                SilksongModPlugin.Log.LogError("Awake: Could not find Host Hornet!");
            }
            
            if (LobbyManager.HostHornet.TryGetComponent<tk2dSprite>(out var sprite))
            {
                DrawHornet(sprite);
            }
            else
            {
                SilksongModPlugin.Log.LogError("Could not find Host Hornet Sprite!");
            }
            CopyAnimatorFields();
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0;
            
            CreateNailAttacks();
            _collider = gameObject.AddComponent<BoxCollider2D>();
            CopyBoxCollider2D(_collider, transform.gameObject);
            SilksongModPlugin.Log.LogInfo("Finished setting up Hornet.");
            gameObject.layer = (int)PhysLayers.GRASS;
        }

        private void Start()
        {
            SilksongModPlugin.Log.LogInfo("First frame of Synced Hornet.");
        }

        void OnDestroy()
        {
            SilksongModPlugin.Log.LogInfo($"Synced Hornet for Player {name} WAS DESTROYED. HOW???");
        }

        public void UpdatePosition(PlayerPosData posData)
        {
            transform.position = posData.Position;
            transform.localScale = posData.LocalScale;
            _rb.linearVelocity = posData.Velocity;
          //  _collider.size = posData.ColliderSize;
          //  _collider.offset = posData.ColliderOffset;
        }
        
        private void DrawHornet(tk2dSprite original)
        {
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            var sprite = gameObject.AddComponent<tk2dSprite>();
            sprite.Collection = original.Collection;
            sprite.spriteId = original.spriteId;
            sprite.color = original.color;
            sprite.scale = original.scale;

            sprite.Build();
            if (sprite == null)
            {
                SilksongModPlugin.Log.LogError("CopyAnimatorFields: Sprite is null! HOW IS THIS EVEN POSSIBLE WTF");
            }
        }
        
        private void CopyAnimatorFields()
        {
            animator = gameObject.GetComponent<tk2dSpriteAnimator>();
            if (animator == null)
            {
                SilksongModPlugin.Log.LogError("Awake: Animator is null!");
                animator = gameObject.AddComponent<tk2dSpriteAnimator>();
            }
            tk2dSpriteAnimator hostAnimator = LobbyManager.HostHornet.GetComponent<tk2dSpriteAnimator>();
            if (hostAnimator == null)
            {
                SilksongModPlugin.Log.LogError("CopyAnimatorFields: Host Animator is null!");
                return;
            }
            if (animator == null)
            {
                SilksongModPlugin.Log.LogError("CopyAnimatorFields: Animator is null!");
            }
            // Copy library
            animator.Library = hostAnimator.Library;

            // Copy default clip
            animator.DefaultClipId = hostAnimator.DefaultClipId;

            // Copy auto-play
            animator.playAutomatically = hostAnimator.playAutomatically;
            animator.DefaultClipId = hostAnimator.DefaultClipId;
            animator.Paused = hostAnimator.Paused;

            // Copy current animation if any
            if (hostAnimator.CurrentClip != null)
            {
                SilksongModPlugin.Log.LogInfo("CopyAnimatorFields: Displaying Current Animation");
                // Play at the same clip!
                animator.Play(hostAnimator.CurrentClip.name);

                // Copy wrap mode
                animator.CurrentClip.wrapMode = hostAnimator.CurrentClip.wrapMode;
            }

            // Copy event delegates
            animator.AnimationCompleted = hostAnimator.AnimationCompleted;
            animator.AnimationEventTriggered = hostAnimator.AnimationEventTriggered;
        }

        private void CreateNailAttacks()
        {
            NailAttacks = new GameObject[LobbyManager.NABListIndex.Count]; // first initialize
            SilksongModPlugin.Log.LogInfo("Creating Nail Attacks for size: " + NailAttacks.Length);
            if (LobbyManager.AttacksBuffer != null)
            {
                GameObject Attack = Instantiate(LobbyManager.AttacksBuffer, transform);
                for (int i = 0; i < Attack.transform.childCount; i++) // add all children to thing
                {
                    if (i >= 0 && i < NailAttacks.Length)
                    {
                        GameObject child = Attack.transform.GetChild(i).gameObject;
                        NailAttacks[i] = child;
                    }
                    else
                    {
                        SilksongModPlugin.Log.LogInfo("the index is out of bounds. How could this hpapen??");
                    }
                }
                return;
            }
            GameObject Attacks = new GameObject("SyncedNailAttacks");
            Attacks.transform.parent = transform;
            Attacks.SetActive(true); // ensure it's not active

            foreach (var NABIndex in LobbyManager.NABListIndex)
            {
                GameObject hostAttack = NABIndex.Key.gameObject;
                bool wasActive = false;
                if (hostAttack.activeSelf)
                {
                    hostAttack.SetActive(false);
                    wasActive = true;
                } // temporarily set it inactive 

                GameObject nailAttack = Instantiate(hostAttack, Attacks.transform);
                if (wasActive)
                {
                    hostAttack.SetActive(true);
                }

                if (nailAttack.activeSelf)
                {
                    SilksongModPlugin.Log.LogInfo("ATTACK IS STILL ACTIVE, THIS FAILs");
                }
                RemoveAllButGraphics(nailAttack);
                if (NABIndex.Value >= 0 && NABIndex.Value < NailAttacks.Length)
                {
                    NailAttacks[NABIndex.Value] = nailAttack;
                }
                else
                {
                    SilksongModPlugin.Log.LogInfo($"i found the problem OMG IS THIS IT? probably not {NABIndex.Value}");
                }
                nailAttack.SetActive(false);
                MeshRenderer renderer = nailAttack.GetComponent<MeshRenderer>();
                renderer.enabled = true;
                tk2dSpriteAnimator animator =  nailAttack.GetComponent<tk2dSpriteAnimator>();
                animator.enabled = true;
                nailAttack.tag = "";
            }
            LobbyManager.AttacksBuffer = Attacks;
        }

        private void RemoveAllButGraphics(GameObject go)
        {
            var components = go.GetComponents<Component>();

            foreach (var c in components)
            {
                if (c == null) continue; // missing script

                if (c is Transform) continue;
                if (c is MeshRenderer) continue;
                if (c is MeshFilter) continue;
                if (c is tk2dSprite) continue;
                if (c is tk2dSpriteAnimator) continue;

                Destroy(c);
            }
        }

        public void TakeDamage()
        {
            byte direction = 1; // 0 is left, 1 is right
            if (transform.position.x > LobbyManager.HostHornet.transform.position.x)
            {
                direction = 0;
            }
            SilksongModPlugin.Log.LogInfo("Found Nail Attack! sending hit data");
            SteamP2PSender.SendData(steamID, new byte[2] {1, direction}, P2PChannel.Attack); // nail damage deals one mask,
            // direction is which side the syncedhornet got hit
        }

        public void ActivateNailAttack(int index, bool active)
        {
            SilksongModPlugin.Log.LogInfo($"ActivateNailAttack called! for gameobject name: {NailAttacks[index].name} {active}");
            if (index >= 0 && index < NailAttacks.Length)
            {
                SilksongModPlugin.Log.LogInfo($"Nail Attack tag: {NailAttacks[index].tag}");
                NailAttacks[index].SetActive(active);
            }
            else
            {
                SilksongModPlugin.Log.LogError("index is out of bounds IN ACTIVATENAILATTACK");
            }
        }

        public tk2dSpriteAnimator RetrieveAnimatorFromIndex(int index)
        {
            if (index >= 0 && index < NailAttacks.Length)
            {
                if (NailAttacks[index].TryGetComponent(out tk2dSpriteAnimator sprite))
                {
                    return sprite;
                }
                else
                {
                    SilksongModPlugin.Log.LogInfo("This should not happen. You screwed up.");
                }
            }
            else
            {
                SilksongModPlugin.Log.LogError("index is out of bounds, OOPPS");
            }

            return null;
        }

        public void CopyBoxCollider2D(BoxCollider2D source, GameObject target)
        {
            if (source == null || target == null) return;

            // Get or add a BoxCollider2D to the target
            BoxCollider2D copy = target.GetComponent<BoxCollider2D>();
            if (copy == null)
            {
                SilksongModPlugin.Log.LogInfo("ERROR: Host Hornet doesn't have boxcollider");
                return;
            }

            // Copy the main properties
            copy.offset = source.offset;
            copy.size = source.size;
            copy.isTrigger = true;
            copy.enabled = true;
            SilksongModPlugin.Log.LogInfo("Copied Box Collider!");
        }
    }
}