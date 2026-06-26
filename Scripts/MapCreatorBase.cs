using Godot;
using System;
using System.Collections.Generic;
using HelperScripts;
public partial class MapCreatorBase : Node3D
{
	[Export] public Node3D Tiles;
	[Export] public PackedScene TileBody;
	public List<TileBody> tile_list = new List<TileBody>();
	private Dictionaries _dict = new Dictionaries();
	public override void _Ready()
	{
		TileBody tile = Tiles.GetNode<TileBody>("TileBody1");
		tile_list.Add(tile);
	}

    public override void _Process(double delta)
    {
        foreach(TileBody tile in tile_list)
		{
			if (tile.new_tile_signal)
			{
				int hovered_index = tile.last_hovered_TPP;
				tile.new_tile_signal = false;
				TileBody new_tile = TileBody.Instantiate<TileBody>();
				new_tile.Position = tile.Position + _dict.TPP_ind_to_pos[hovered_index];
				string new_name = "TileBody" + (tile_list.Count+1);
				new_tile.Name = new_name;
				new_tile.just_created = true;

				Tiles.AddChild(new_tile, true);
				tile_list.Add(new_tile);

				foreach(TileBody tile_body in tile_list) //handling cleaning TPPs
				{
					tile_body.new_tile_created = true;
				}

				var t = Tiles.GetNode<TileBody>(new_name);
				t.TilePlacePointsChangeMaterial(true);
				break;
			}
		}
    }


	public void AddingTilesEnabled()
	{
		foreach(TileBody tile in tile_list)
		{
			tile.TilePlacePointsChangeMaterial(true);
		}
	}

	public void AddingTilesDisabled()
	{
		foreach(TileBody tile in tile_list)
		{
			tile.TilePlacePointsChangeMaterial(false);
		}
	}
}
