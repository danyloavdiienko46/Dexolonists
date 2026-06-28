using Godot;
using System;

public partial class MainMenu : Control
{
	private SceneTree _tree;

    public override void _Ready()
    {
        _tree = GetTree();
    }

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
		GetTree().ChangeSceneToFile("res://Scenes/Menus/multiplayer_menu.tscn");
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
