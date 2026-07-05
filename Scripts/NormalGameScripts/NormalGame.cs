using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NormalGame : Node3D
{
	[Export] public NormalGameBase NormalGameBaseNode;
	[Export] public PackedScene MainMenuScene;

	public bool is_item_placed = false;

    public override void _PhysicsProcess(double delta)
    {
		if (NormalGameBaseNode.is_item_placed)
		{
			is_item_placed = true;
			NormalGameBaseNode.is_item_placed = false;
		}
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
		GD.Print("Trying to choose item with index " + index);
		NormalGameBaseNode.ItemChosenHandler(index);
	}

}
