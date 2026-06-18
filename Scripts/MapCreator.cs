using Godot;
using System;

public partial class MapCreator : Node3D
{
	[Export] public PackedScene TileBody;
	[Export] public MapCreatorBase MapCreatorBase;
	
	public void AddTileMode(bool enabled)
	{
		if (enabled)
		{
			GD.Print("Adding enabled!");
			MapCreatorBase.AddingTilesEnabled();
		}
		else
		{
			GD.Print("Adding disabled!");
			MapCreatorBase.AddingTilesDisabled();
		}
		
	}

	public void DeleteTileMode(bool enabled)
	{
		if (enabled)
		{
			GD.Print("Deleting enabled!");	
		}
		else
		{
			GD.Print("Deleting disabled!");
		}
	}
}
