using Godot;
using System.Collections.Generic;

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

	public void SaveMap(string file_path)
	{
		var save_array = new Godot.Collections.Array();
		var tile_list = MapCreatorBase.tile_list;

		foreach (TileBody tile in tile_list)
		{
			TileSave data = tile.SaveData();
			save_array.Add(data.ToDictionary());
		}

		using var file = FileAccess.Open(file_path, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			file.StoreVar(save_array, fullObjects: true);
			GD.Print("Map saved successfully!");
		}
	}
}
