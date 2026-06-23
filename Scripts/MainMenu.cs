using Godot;
using System;

public partial class MainMenu : Control
{
	private SceneTree _tree;

    public override void _Ready()
    {
        _tree = GetTree();
    }

	/*public void Server()
	{
		GetNode<HighLvlNetworkHandler>("/root/Network").StartServer();
		GetTree().ChangeSceneToFile("res://main.tscn");
	}*/

	public void MapCreator()
	{
		GD.Print("Starting Map Creator!");
		//GetNode<HighLvlNetworkHandler>("/root/Network").StartClient();
    	GetTree().ChangeSceneToFile("res://Scenes/map_creator.tscn");
	}

	public void SinglePlayer()
	{
		GD.Print("Starting Single Player!");
		GetTree().ChangeSceneToFile("res://Scenes/normal_game.tscn");
	}

	public void MultiplayerFunc()
	{
		GD.Print("Starting Multiplayer!");
	}

	public void Settings()
	{
		GD.Print("Opening Settings!");
	}

	public void Exit()
	{
		GD.Print("Exiting Game!");
		_tree.Quit();
	}
}
