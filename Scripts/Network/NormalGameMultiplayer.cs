using Godot;
using System;
using System.Collections.Generic;
using HelperScripts;

public partial class NormalGameMultiplayer : Node3D
{
	[Export] public NormalGameBase NormalGameBaseNode;
	[Export] public PackedScene MainMenuScene;

	//private NormalGameUi _normal_game_ui;

    public override void _Ready()
    {
       // _normal_game_ui = GetNode("NormalGamePlayer").GetNode<NormalGameUi>("NormalGameUI");
    }

	
	public void LoadMapBase(string file_path)
	{
		NormalGameBaseNode.LoadMap(file_path);
	}

	public void ExitToMainMenu()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}

	public void ItemChosen(int index)
	{
		NormalGameBaseNode.ItemChosenHandler(index);
	}

	public void ItemsDeselect()
	{
		//_normal_game_ui.ObjectListDeselect();
	}
}
