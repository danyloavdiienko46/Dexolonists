using Godot;
using System;

public partial class MultiplayerMenu : Control
{

	[Export] public PackedScene MainMenuScene;
	[Export] public PackedScene MultiplayerWorldScene;
	
	public void HostServerBtnPressed()
	{
		GD.Print("Creating a server!");
		GetNode<NetworkHandler>("/root/NetworkHandler").HostServer();
		GetTree().ChangeSceneToPacked(MultiplayerWorldScene);

	}

	public void JoinServerBtnPressed()
	{
		GD.Print("Joining a server!");
		GetNode<NetworkHandler>("/root/NetworkHandler").JoinServer();
		GetTree().ChangeSceneToPacked(MultiplayerWorldScene);
	}

	public void MainMenuBtnPressed()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}
}
