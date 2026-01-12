using System;
using Steamworks;

namespace SilksongMod
{
    public readonly struct SteamPlayer : IEquatable<SteamPlayer>
    {
        public readonly string Name;
        public readonly CSteamID SteamID;

        public SteamPlayer(string name, CSteamID steamID)
        {
            this.Name = name;
            SteamID = steamID;
        }

        // Equality: ONLY SteamID
        public bool Equals(SteamPlayer other)
        {
            return SteamID == other.SteamID;
        }

        public override bool Equals(object obj)
        {
            return obj is SteamPlayer other && Equals(other);
        }

        // Hash: ONLY SteamID
        public override int GetHashCode()
        {
            return SteamID.GetHashCode();
        }

        // Optional but recommended
        public static bool operator ==(SteamPlayer left, SteamPlayer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SteamPlayer left, SteamPlayer right)
        {
            return !left.Equals(right);
        }
    }

}