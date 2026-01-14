using UnityEngine;

namespace SilksongMod
{
    public struct PlayerPosData
    {
        public readonly Vector3 Position;
        public readonly Vector3 LocalScale; // for direction
        public readonly Vector2 Velocity;
        
        public readonly Vector2 ColliderOffset;
        public readonly Vector2 ColliderSize;
        

        public PlayerPosData(Vector3 position, Vector3 localScale, Vector2 velocity, Vector2 colliderOffset, Vector2 colliderSize)
        {
            Position = position;
            LocalScale = localScale;
            Velocity = velocity;
            ColliderOffset = colliderOffset;
            ColliderSize = colliderSize;
        }

        public PlayerPosData(Transform transform, Rigidbody2D rb, BoxCollider2D collider)
        {
            Position = transform.position;
            LocalScale = transform.localScale;
            Velocity = rb.linearVelocity;
            ColliderOffset = collider.offset;
            ColliderSize = collider.size;
        }
    }
}