using System.Collections.Generic;
using SilksongMod.SteamP2P;
using UnityEngine;

namespace SilksongMod
{
    public class NetworkSender : MonoBehaviour
    {
        private const float SendInterval = 1f / 40f; // 20 Hz
        private float _timer;

        private Rigidbody2D _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= SendInterval)
            {
                _timer -= SendInterval; // preserves timing accuracy
                SendNetworkUpdate();
            }
        }

        void SendNetworkUpdate()
        {
            if (LobbyManager.Players.Count > 1 && LobbyManager.CurrScene != "MAINMENU")
            {
                //serialize position and scale (scale for direction, position for position)
                byte[] data = Serializer.SerializePlayerPosData(new PlayerPosData(transform.position, transform.localScale, _rb.linearVelocity));
                foreach (KeyValuePair<SteamPlayer, string> playerData in LobbyManager.Players)
                {
                    if (!playerData.Key.Equals(LobbyManager.CurrPlayer) &&
                        playerData.Value.Equals(LobbyManager.CurrScene))
                    {
                        SteamP2PSender.SendPositionDataTo(playerData.Key, data);
                    }
                }
            }
        }
    }
}