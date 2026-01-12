using System;
using Steamworks;
using UnityEngine;

//using UnityEngine.Windows;

namespace SilksongMod
{
    public class SyncedHornetScript : MonoBehaviour
    {
        public CSteamID steamID;
        public string name; // for later name above hornet implementation
        
        public tk2dSpriteAnimator animator;

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

            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            SilksongModPlugin.Log.LogInfo("Finished setting up Hornet.");
            CopyAnimatorFields();
        }

        private void Start()
        {
            SilksongModPlugin.Log.LogInfo("First frame of Synced Hornet.");
        }

        void OnDestroy()
        {
            SilksongModPlugin.Log.LogInfo($"Synced Hornet for Player {name} WAS DESTROYED. HOW???");
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
        
    }
}