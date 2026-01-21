using System.Collections.Generic;
using System.Net.Mime;
using GlobalEnums;
using SilksongMod.SteamP2P;
using Steamworks;
using UnityEngine;

namespace SilksongMod
{
    public class NetworkSender : MonoBehaviour
    {
        private const float SendInterval = 1f / 20f; // 20 Hz
        private float _timer;

        private Rigidbody2D _rb;
        private BoxCollider2D _collider;

        Vector3 lastPhysicsPos;
        Vector3 lastPhysicsVel;

        private TextMesh _text;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _text = SyncedHornetScript.CreateTextComponent(gameObject, LobbyManager.CurrName);
            _text.color = Color.yellow;
        }

        void Update()
        {
            _text.font = LobbyManager.DefaultFont;
           // _text.transform.localScale = Vector3.one;
            _timer += Time.deltaTime;

            if (_timer >= SendInterval)
            {
                _timer -= SendInterval; // preserves timing accuracy
                SendNetworkUpdate();
            }
        }
        void LateUpdate()
        {
            Vector3 parentScale = _text.transform.parent.lossyScale;
            _text.transform.localScale = new Vector3(
                1f / parentScale.x,
                1f / parentScale.y,
                1f / parentScale.z
            );
        }

        void FixedUpdate()
        {
            lastPhysicsVel = _rb.linearVelocity;
            lastPhysicsPos = _rb.position;
        }
        

        void SendNetworkUpdate()
        {
            if (!LobbyManager.LobbyPlayers.IsNullOrEmpty() && LobbyManager.CurrScene != "MAINMENU")
            {
                //serialize position and scale (scale for direction, position for position)
                byte[] data = Serializer.SerializePlayerPosData(new PlayerPosData(transform, _rb, _collider));
                LobbyManager.SendDataToLobby(data, P2PChannel.Pos);
            }
        }
    }
}