using Godot;
using System;

public partial class NormalGame : Node3D
{
	[Export] public NormalGameBase NormalGameBaseNode;
	[Export] public NormalGameUi NormalGameUI;
	[Export] public PackedScene MainMenuScene;
	
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
		NormalGameUI.ObjectListDeselect();
	}

}
