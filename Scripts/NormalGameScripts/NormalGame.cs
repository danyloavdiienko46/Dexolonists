using Godot;
using System;

public partial class NormalGame : Node3D
{
	[Export] public NormalGameBase NormalGameBaseNode;
	[Export] public PackedScene MainMenuScene;

	public ItemType? chosen_item = null;
	
	public void LoadMapBase(string file_path)
	{
		NormalGameBaseNode.LoadMap(file_path);
	}

	public void ExitToMainMenu()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}
}
