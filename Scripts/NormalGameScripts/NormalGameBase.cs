using Godot;
using System.Collections.Generic;

public partial class NormalGameBase : Node3D
{
	[Export] public Node3D Tiles;
	[Export] public PackedScene TileScene;
	private List<Tile> _tile_list = new List<Tile>();
	public List<Tile> LoadMap(string file_path)
	{
		var new_tile_list = new List<Tile>();

		if (!FileAccess.FileExists(file_path)) return new_tile_list;

		using var file = FileAccess.Open(file_path, FileAccess.ModeFlags.Read);
		if (file == null) return new_tile_list;

		var save_array = (Godot.Collections.Array)file.GetVar(allowObjects: true);

		foreach (Godot.Collections.Dictionary dict in save_array)
		{
			TileSave data = TileSave.FromDictionary(dict);

			if (string.IsNullOrEmpty(data.ScenePath)) continue;

			//var tileScene = GD.Load<PackedScene>(data.ScenePath);
			//if (tileScene == null) continue;

			Tile new_tile = TileScene.Instantiate<Tile>();

			new_tile.LoadData(data);
			Tiles.AddChild(new_tile);
			new_tile_list.Add(new_tile);
		}

		GD.Print("Map loaded and spawned successfully!");
		return new_tile_list;
	}
}
