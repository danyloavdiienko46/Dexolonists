using Godot;
using System;

public partial class MultiplayerMenu : Control
{

	[Export] public PackedScene MainMenuScene;
	
	public void HostServerBtnPressed()
	{
		GD.Print("Creating a server!");
		GetNode<NetworkHandler>("/root/NetworkHandler").HostServer();
	}

	public void JoinServerBtnPressed()
	{
		GD.Print("Joining a server!");
		GetNode<NetworkHandler>("/root/NetworkHandler").JoinServer();
	}

	public void MainMenuBtnPressed()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}
}
