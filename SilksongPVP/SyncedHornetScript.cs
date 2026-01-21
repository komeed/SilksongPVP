using System;
using System.Collections.Generic;
using GlobalEnums;
using SilksongMod.SteamP2P;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

//using UnityEngine.Windows;

namespace SilksongMod
{
    public class SyncedHornetScript : MonoBehaviour
    {
        public CSteamID steamID;
        public string name; // for later name above hornet implementation
        public string scene;
        public bool isUDP = false; // default is using steamworks
        
        public tk2dSpriteAnimator animator;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;

        private GameObject[] NailAttacks;
        private SpriteFlash _flash;
        private MeshRenderer _renderer;
        
        private TextMesh _text;
        private static float yOffset = 2f;

        public GameObject compassIcon;
        private bool foundCompassIcon;

        private void Awake()
        {
            SilksongModPlugin.Log.LogInfo("SyncedHornetScript: Awake");
            if (LobbyManager.HostHornet == null)
            {
                SilksongModPlugin.Log.LogError("Awake: Could not find Host Hornet! disabling");
                gameObject.SetActive(false);
                return;
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
            gameObject.layer = (int)PhysLayers.ENEMIES;
            
            _flash = gameObject.AddComponent<SpriteFlash>();
            _flash.enabled = true;

            _text = CreateTextComponent(gameObject, name);
        }

        public static TextMesh CreateTextComponent(GameObject parent, string name)
        {
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.SetParent(parent.transform);

            // Position it y units above this GameObject
            textObj.transform.localPosition = Vector3.up * yOffset;
            textObj.transform.localScale = Vector3.one;
            // Optional: make it follow this object

            TextMesh tmp = textObj.AddComponent<TextMesh>();
            tmp.text = name;
            tmp.fontSize = 30;
            tmp.characterSize = 0.2f;
            //tmp.characterSize = 1;
            tmp.font = LobbyManager.DefaultFont;
            tmp.GetComponent<MeshRenderer>().material = LobbyManager.DefaultFont.material;
            tmp.anchor = TextAnchor.MiddleCenter;
            tmp.alignment = TextAlignment.Center;
            
            if (Camera.main)
            {
                tmp.transform.forward = Camera.main.transform.forward;
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("Camera main doesn't exist!");
            }
           // textObj.transform.SetParent(null);
            return tmp;
        }

        private void Start()
        {
            SilksongModPlugin.Log.LogInfo("First frame of Synced Hornet.");
            
        }

        private void Update()
        {
            _text.text = name;
            _text.font = LobbyManager.DefaultFont;
        }

        private void LateUpdate()
        {
            Vector3 parentScale = _text.transform.parent.lossyScale;
            _text.transform.localScale = new Vector3(
                1f / parentScale.x,
                1f / parentScale.y,
                1f / parentScale.z
            );
        }

        void OnDestroy()
        {
            SilksongModPlugin.Log.LogInfo(
                $"Synced Hornet for Player {name} was destroyed. Destroying CompassIcon for that hornet.");
            Destroy(compassIcon);
        }

        public void UpdatePosition(PlayerPosData posData)
        {
            transform.position = posData.Position;
            transform.localScale = posData.LocalScale;
            _rb.linearVelocity = posData.Velocity;
          //  _collider.size = posData.ColliderSize;
          //  _collider.offset = posData.ColliderOffset;
        }

        public void ShowHitAnim()
        {
            if (!_flash.enabled)
            {
                SilksongModPlugin.Log.LogInfo("THE FLASH COMPONENT ISN'T EVEN ENABLED WTF");
                _flash.enabled = true;
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("Flash is enabled....");
            }

            if (!_renderer.enabled)
            {
                SilksongModPlugin.Log.LogInfo("The Renderer component isn't even enabled wtf!!!");
                _renderer.enabled = true;
            }
            else
            {
                SilksongModPlugin.Log.LogInfo("Rednerered is enabled....");
            }
            _flash.FlashEnemyHit();
        }
        
        private void DrawHornet(tk2dSprite original)
        {
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = LobbyManager.HostHornet.GetComponent<MeshFilter>().sharedMesh;
            _renderer = gameObject.AddComponent<MeshRenderer>();
            CopyMeshRenderer(LobbyManager.HostHornet.GetComponent<MeshRenderer>(), _renderer);
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
        
        public static void CopyMeshRenderer(MeshRenderer source, MeshRenderer target)
        {
            if (source == null || target == null)
            {
                Debug.LogWarning("Source or target is null");
                return;
            }

            // Copy enabled state
            target.enabled = true;

            // Copy shadow settings
            target.shadowCastingMode = source.shadowCastingMode;
            target.receiveShadows = source.receiveShadows;

            // Copy materials safely (unique instances)
            Material[] sourceMats = source.sharedMaterials;       // get the original materials
            Material[] newMats = new Material[sourceMats.Length]; // new array for target

            for (int i = 0; i < sourceMats.Length; i++)
            {
                newMats[i] = Instantiate(sourceMats[i]); // clone each material
            }

            target.materials = newMats; // assign cloned materials to target

            // Copy light probe & reflection settings
            target.lightProbeUsage = source.lightProbeUsage;
            target.reflectionProbeUsage = source.reflectionProbeUsage;
            target.probeAnchor = source.probeAnchor;

            // Copy sorting layer / order (for UI / Sprite Renderer similar)
            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder;

            // Copy motion vector / rendering layer options (optional)
            target.allowOcclusionWhenDynamic = source.allowOcclusionWhenDynamic;
            target.motionVectorGenerationMode = source.motionVectorGenerationMode;
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

        public static void RemoveAllButGraphics(GameObject go)
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
            //SilksongModPlugin.Log.LogInfo("Found Nail Attack! sending hit data"); // mask , direction
            SteamP2PSender.SendData(steamID, new byte[2] {1, direction}, P2PChannel.Attack); // nail damage deals one mask,
            // direction is which side the syncedhornet got hit
        }

        public void ActivateNailAttack(int index, bool active)
        {
           // SilksongModPlugin.Log.LogInfo($"ActivateNailAttack called! for gameobject name: {NailAttacks[index].name} {active}");
            if (index >= 0 && index < NailAttacks.Length)
            {
              //  SilksongModPlugin.Log.LogInfo($"Nail Attack tag: {NailAttacks[index].tag}");
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