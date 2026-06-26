using Godot;
using HelperScripts;
using System;
using System.Collections.Generic;

public partial class NormalGameBase : Node3D
{
	[Export] public Node3D Tiles;
	[Export] public Node3D PlacedItems;
	[Export] public Node3D PlacedRoads;
	[Export] public PackedScene TileScene;
	[Export] public PackedScene HouseScene;
	[Export] public PackedScene RoadScene;

	private NormalGame _normal_game;
	private List<Tile> _tile_list = new List<Tile>();
	private int _chosen_item = -1;
	private Dictionaries _dict = new Dictionaries();

	private Random _rand = new Random();

    public override void _Ready()
    {
        _normal_game = GetOwner<NormalGame>();
    }

    public override void _Process(double delta)
    {
        foreach(Tile tile in _tile_list)
		{
			if(tile.new_item_signal) 
			{
				ItemPlacementHandler(tile);
				break;
			}
		}
    }

	private void ClearAllLists()
	{
		foreach(Tile tile in Tiles.GetChildren())
		{
			tile.QueueFree();
		}

		foreach(Node item in PlacedItems.GetChildren())
		{
			item.QueueFree();
		}

		foreach(Node item in PlacedRoads.GetChildren())
		{
			item.QueueFree();
		}

		_tile_list.Clear();
	}

	public void LoadMap(string file_path)
	{
		ClearAllLists();

		using var file = FileAccess.Open(file_path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.Print("File is null!");
			return;
		}

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
			_tile_list.Add(new_tile);
		}

		GD.Print("Map loaded and spawned successfully!");
	}

	private void ItemPlacementHandler(Tile tile)
	{
		switch (_chosen_item)
		{
			case 0:
				{
					GD.Print("Placing house!");

					StaticBody3D new_item = HouseScene.Instantiate<StaticBody3D>();
					new_item.Position = tile.new_item_pos;

					PlacedItems.AddChild(new_item);
					
					//tile_list.Add(new_item);

					foreach(Tile titile in _tile_list)
					{
						titile.ItemPlacePointsChangeMaterial();
					}

					//var t = Tiles.GetNode<TileBody>(new_name);
					//t.TilePlacePointsChangeMaterial(true);
					break;
				}
			case 1:
				{
					GD.Print("Placing road!");

					StaticBody3D new_item = RoadScene.Instantiate<StaticBody3D>();
					new_item.Position = tile.new_item_pos;

					float fluctuation = (float)(_rand.NextDouble()+_rand.Next(4)-_rand.Next(4));
					new_item.Rotate(new Vector3(0, 1, 0),
					 Mathf.DegToRad(_dict.RPP_ind_to_rot_degrees[tile.last_hovered_RPP_ind] + fluctuation));

					PlacedItems.AddChild(new_item);

					foreach(Tile titile in _tile_list)
					{
						titile.RoadPlacePointsChangeMaterial();
					}

					break;
				}
			default:
				{
					GD.Print("Wrong index for ItemPlacementHandler");
					break;
				}
		}

		tile.new_item_signal = false;
		_chosen_item = -1;
		_normal_game.ItemsDeselect();
	}

	public override void _Input(InputEvent @event)
    {
		if (Input.IsActionJustPressed("RMB_click"))
		{
			_normal_game.ItemsDeselect();
			ItemChosenHandler(_chosen_item);
			_chosen_item = -1;
		}
    }

	public void ItemChosenHandler(int index)
	{
		switch (index)
		{
			case 0:
				{
					foreach(Tile tile in _tile_list)
					{
						tile.ItemPlacePointsChangeMaterial();
					}
					_chosen_item = 0;
					break;
				}
			case 1:
				{
					foreach(Tile tile in _tile_list)
					{
						tile.RoadPlacePointsChangeMaterial();
					}
					_chosen_item = 1;
					break;
				}
			default:
				{
					GD.Print("Wrong index for ItemChosenHandler!");
					_chosen_item = -1;
					break;
				}
		}
	}
}
