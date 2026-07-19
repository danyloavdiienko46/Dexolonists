using Godot;
using HelperScripts;
using System;
using System.Collections.Generic;
using System.Drawing;

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
	public bool is_item_placed = false;
	private Dictionaries _dict = new Dictionaries();
	private long _placing_player_id = -1;

	private Random _rand = new Random();

    public override void _Ready()
    {
        _normal_game = GetParent<NormalGame>();

		Tiles.ChildEnteredTree += OnTileChildEnteredTree;
    	Tiles.ChildExitingTree += OnTileChildExitingTree;
    }

	private void OnTileChildEnteredTree(Node node)
	{
		if (node is Tile tile && !_tile_list.Contains(tile))
		{
			_tile_list.Add(tile);
		}
	}

	private void OnTileChildExitingTree(Node node)
	{
		if (node is Tile tile && _tile_list.Contains(tile))
		{
			_tile_list.Remove(tile);
		}
	}

    public override void _Process(double delta)
    {
        foreach(Tile tile in _tile_list)
		{
			if(tile.new_item_signal) 
			{
				tile.new_item_signal = false;
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
		if (!Multiplayer.IsServer() && Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer) return;

		ClearAllLists();

		using var file = FileAccess.Open(file_path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.Print("File is null!");
			return;
		}

		var save_array = (Godot.Collections.Array)file.GetVar(allowObjects: true);
		int tile_counter = 0;

		foreach (Godot.Collections.Dictionary dict in save_array)
		{
			TileSave data = TileSave.FromDictionary(dict);

			if (string.IsNullOrEmpty(data.ScenePath)) continue;

			Tile new_tile = TileScene.Instantiate<Tile>();

			new_tile.Name = $"Tile_{tile_counter}";
        	tile_counter++;

			int tile_type = _rand.Next(1, 6);
			int tile_number = -1;
			while(tile_number == -1 || tile_number == 7) tile_number = _rand.Next(2, 13);

			Tiles.AddChild(new_tile, true);

			new_tile.Rpc(nameof(Tile.RpcSyncTileData), dict, tile_type, tile_number);
		}

		GD.Print("Map loaded and spawned successfully!");
	}

	private void ItemPlacementHandler(Tile tile)
	{
		NodePath tile_path = tile.GetPath();
		int item_to_place = _chosen_item;
		int point_index = tile.last_hovered_point;
		Vector3 placement_position = tile.new_item_pos;
		int rpp_index = tile.last_hovered_RPP_ind;
		long placing_player_ID = _placing_player_id;

		if (Multiplayer.IsServer())
		{
			ServerProcessPlacement(item_to_place, tile_path, point_index, placement_position, rpp_index, placing_player_ID);
		}
		else
		{
			RpcId(1, nameof(RpcRequestItemPlacement), item_to_place, tile_path, point_index, placement_position, rpp_index, placing_player_ID);
			_chosen_item = -1;
			LocalClearPlacementUI();
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public void RpcRequestItemPlacement(int chosen_item, NodePath tile_path, int point_index, Vector3 placement_position, int rpp_index, long placing_player_id)
	{
		if (!Multiplayer.IsServer()) return;
		ServerProcessPlacement(chosen_item, tile_path, point_index, placement_position, rpp_index, placing_player_id);
	}

	private void ServerProcessPlacement(int chosen_item, NodePath tile_path, int point_index, Vector3 placement_position, int rpp_index, long placing_player_ID)
	{
		Tile tile = GetNodeOrNull<Tile>(tile_path);
		if(tile == null)
		{
			GD.Print("Tile is null, aborting...");
			return;
		}

		string node_name = "";
		float final_road_rotation = 0.0f;

		switch (chosen_item)
		{
			case 0:
			{
				int index = PlacedItems.GetChildren().Count;
				node_name = $"House_{index}";
				break;
			}
		case 1:
			{
				int index = PlacedRoads.GetChildren().Count;
				node_name = $"Road_{index}";

				float fluctuation = (float)(_rand.NextDouble() + _rand.Next(4) - _rand.Next(4));
				float rot_degree_val = 0.0f;
				if(!_dict.RPP_ind_to_rot_degrees.TryGetValue(rpp_index, out rot_degree_val))
					rot_degree_val = 0.0f;

				final_road_rotation = rot_degree_val + fluctuation;
				break;
			}
			default:
				{
					GD.Print("Wrong index for ItemPlacementHandler");
					break;
				}
		}
		Rpc(nameof(RpcBroadcastSpawnItem), chosen_item, tile_path, point_index, placement_position, node_name, final_road_rotation, placing_player_ID);
		Rpc(nameof(RpcBroadcastClearUI));

		tile.new_item_signal = false;
		_chosen_item = -1;

		is_item_placed = true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] // CallLocal = true forces host + clients to run this
	public void RpcBroadcastSpawnItem(int chosen_item, NodePath tile_path, int point_index, Vector3 placement_position, string node_name, float road_rotation, long placing_player_ID)
	{
		Tile tile = GetNodeOrNull<Tile>(tile_path);
		if (tile != null)
		{
			tile.DeactivatePointLocally(chosen_item == 0, point_index);
		}

		int colour_id = _normal_game.GetColourID(placing_player_ID);

		GD.Print("Placing player id is " + placing_player_ID + ", that means colour id is " + colour_id);

		if (chosen_item == 0) //house
		{
			House new_item = HouseScene.Instantiate<House>();
			new_item.Position = placement_position;
			new_item.Name = node_name;
			new_item.colour_index = colour_id;

			PlacedItems.AddChild(new_item);
		}
		else if (chosen_item == 1) //road
		{
			Road new_item = RoadScene.Instantiate<Road>();
			new_item.Position = placement_position;
			new_item.Name = node_name;
			new_item.Rotate(new Vector3(0, 1, 0), Mathf.DegToRad(road_rotation));
			new_item.colour_index = colour_id;

			PlacedRoads.AddChild(new_item);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RpcBroadcastClearUI()
	{
		LocalClearPlacementUI();
	}

	private void LocalClearPlacementUI()
	{
		foreach(Tile tile in _tile_list)
		{
			if (tile != null)
			{
				tile.ForceClearAllPlacementMaterials();
			}
		}
	}

	public override void _Input(InputEvent @event)
    {
		if (Input.IsActionJustPressed("RMB_click"))
		{
			ItemChosenHandler(_chosen_item);
			_chosen_item = -1;
		}
    }

	public void ItemChosenHandler(int index, long player_ID = 0)
	{
		_placing_player_id = player_ID;
		switch (index)
		{
			case 0:
				{
					GD.Print("Choosing house!");
					foreach(Tile tile in _tile_list)
					{
						tile.last_chosen_item_type = ItemType.House;
						tile.ItemPlacePointsChangeMaterial(ItemType.House);
					}
					_chosen_item = 0;
					break;
				}
			case 1:
				{
					GD.Print("Choosing road!");
					foreach(Tile tile in _tile_list)
					{
						tile.last_chosen_item_type = ItemType.Road;
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
