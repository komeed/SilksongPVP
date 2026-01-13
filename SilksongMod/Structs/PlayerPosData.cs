using UnityEngine;

namespace SilksongMod
{
    public struct PlayerPosData
    {
        public readonly Vector3 Position;
        public readonly Vector3 LocalScale;
        public readonly Vector3 Velocity;

        public PlayerPosData(Vector3 position, Vector3 localScale, Vector3 velocity)
        {
            Position = position;
            LocalScale = localScale;
            Velocity = velocity;
        }
    }
}