using Godot;
using System;

public partial class NetworkHandler : Node
{
	private ENetMultiplayerPeer peer;
	private const string IP_ADDRESS = "localhost";
	private const int PORT = 2137;
	
	public void HostServer()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateServer(PORT);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Server created succesfully!");
	}

	public void JoinServer()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(IP_ADDRESS, PORT);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Joined to server succesfully!");
		
	}
}
