using Godot;

public partial class NetworkHandler : Node
{
    private ENetMultiplayerPeer peer;
    private const string IP_ADDRESS = "127.0.0.1";
    private const int PORT = 2137;
    
    public void HostServer()
    {
        peer = new ENetMultiplayerPeer();
        peer.CreateServer(PORT);
        Multiplayer.MultiplayerPeer = peer;

        GD.Print("Server created successfully!");
    }

    public void JoinServer()
    {
        peer = new ENetMultiplayerPeer();
        peer.CreateClient(IP_ADDRESS, PORT);
        Multiplayer.MultiplayerPeer = peer;

        GD.Print("Joined to server successfully!");
    }
}