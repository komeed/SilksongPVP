namespace SilksongMod.Enums
{
    public enum LobbyCommand : byte
    {
        LobbyDict = 1,
        LobbyDictToJoin = 2, // Player 1 sends to player 2 to join (new player) so player 2 retrieves their lobby info before joining
        PlayerJoined = 3, // Player that clicked "Join" sends confirmation to everyone in the lobby fo the person who sent it
        SceneChange = 4, // player changes scene, sends to everyoen else this command
        LeaveLobby = 5,
        Ping = 6,
        Pong = 7
    }
}