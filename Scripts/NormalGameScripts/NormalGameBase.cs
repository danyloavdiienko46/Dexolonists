using dexolonists.Scripts.Enums;
using Godot;
using HelperScripts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class NormalGameBase : Node3D
{
	[Export] public Node3D Tiles;
	[Export] public Node3D PlacedItems;
	[Export] public Node3D PlacedRoads;
	[Export] public Node3D ItemPlacePoints;
	[Export] public Node3D RoadPlacePoints;
	[Export] public PackedScene TileScene;
	[Export] public PackedScene HouseScene;
	[Export] public PackedScene RoadScene;
	[Export] public PackedScene ItemPlacePointScene;
	[Export] public PackedScene RoadPlacePointScene;
	[Export] public Material ItemPlacePointsVisibleMat;
	[Export] public Material ItemPlacePointsInvisibleMat;
	[Export] public Material ItemPlacePointHighlightedMat;

	private NormalGame _normal_game;
	private List<Tile> _tile_list = new List<Tile>();
	private int _chosen_item = -1;
	public bool is_item_placed = false;
	private Dictionaries _dict = new Dictionaries();
	private long _placing_player_id = -1;

	private Godot.Collections.Array<ItemPlacePoint> _item_place_points = new Godot.Collections.Array<ItemPlacePoint>();
	private Godot.Collections.Array<RoadPlacePoint> _road_place_points = new Godot.Collections.Array<RoadPlacePoint>();

	private Vector3 _new_item_pos = new Vector3(-1, -1, -1);

	private bool _is_IPP_active_for_placement = false;
	private bool _is_RPP_active_for_placement = false;
	private int _hovered_IPP_index = -1;
	private int _hovered_RPP_index = -1;
	private int _last_hovered_point = -1;
	private int _last_hovered_RPP_ind = -1;

	private Random _rand = new Random();
	private Shuffler _shuffler = new Shuffler();

	private List<int> _tile_numbers_list = [];
	private List<int> _tile_types_list = [];
	private int _tile_number = 0;

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

		using var file = FileAccess.Open(file_path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.Print("File is null!");
			return;
		}
		var save_array = (Godot.Collections.Array)file.GetVar(allowObjects: true);
		_tile_number = save_array.Count;
		_tile_numbers_list.Clear();
		_tile_types_list.Clear();

		int number_left = _tile_number;
		int desert_number = (int)Math.Ceiling(_tile_number * 0.05);
		int least_tiles_number = desert_number;
		number_left -= desert_number + 2*least_tiles_number;
		int most_tiles_number = (int)Math.Ceiling(number_left/8.0);

		for(int i = 2; i < 13; i++)
		{
			if(i == 7) continue;

			else if(i == 2 || i == 12)
			{
				for(int j = 0; j < least_tiles_number; j++) _tile_numbers_list.Add(i);
			}
			else
			{
				for(int j = 0; j < most_tiles_number; j++) _tile_numbers_list.Add(i);
			}
		}

		int tiles_with_resources_number = _tile_number - desert_number;
		int tree_wheat_sheep = (int)Math.Ceiling(tiles_with_resources_number*0.22);
		int brick_ore_number = (int)Math.Ceiling(tiles_with_resources_number*0.16);

		for(int i = 0; i < tree_wheat_sheep; i++)
		{
			_tile_types_list.Add((int)TileType.Tree);
			_tile_types_list.Add((int)TileType.Wheat);
			_tile_types_list.Add((int)TileType.Sheep);
		}
		for(int i = 0; i < brick_ore_number; i++)
		{
			_tile_types_list.Add((int)TileType.Brick);
			_tile_types_list.Add((int)TileType.Ore);
		}
		for(int i = 0; i < desert_number; i++)
		{
			_tile_types_list.Add((int)TileType.Desert);
		}

		_shuffler.ShuffleList(_tile_numbers_list);
		_shuffler.ShuffleList(_tile_types_list);

    	Rpc(nameof(RpcLoadMap), file_path, _tile_numbers_list.ToArray(), _tile_types_list.ToArray());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RpcLoadMap(string file_path, int[] tile_numbers, int[] tile_types)
	{
		LoadMapExecute(file_path, tile_numbers.Cast<int>().ToList(), tile_types.Cast<int>().ToList());
	}

	private void LoadMapExecute(string file_path, List<int> tile_numbers, List<int> tile_types)
	{
		ClearAllLists();

		using var file = FileAccess.Open(file_path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.Print("File is null!");
			return;
		}

		var save_array = (Godot.Collections.Array)file.GetVar(allowObjects: true);
		int tile_counter = 0;
		int IPP_ind = 0;
		int RPP_ind = 0;

		foreach (Godot.Collections.Dictionary dict in save_array)
		{
			TileSave data = TileSave.FromDictionary(dict);

			if (string.IsNullOrEmpty(data.ScenePath)) continue;

			Tile new_tile = TileScene.Instantiate<Tile>();

			new_tile.Name = $"Tile_{tile_counter}";
        	tile_counter++;

			int tile_type = tile_types.First();
			tile_types.RemoveAt(0);
			int tile_number = -1;
			if(tile_type != (int)TileType.Desert)//So that desert tile doesn't take itself a number
			{
				tile_number = tile_numbers.First();
				tile_numbers.RemoveAt(0); 
			}
			

			Tiles.AddChild(new_tile, true);

			foreach (var IPPSave in data.item_place_point_saves)
			{
				var new_IPP = ItemPlacePointScene.Instantiate<ItemPlacePoint>();
				new_IPP.Name = IPPSave.point_node_name;
				new_IPP.Position = IPPSave.point_node_position;

				int index = IPP_ind;
				new_IPP.MouseEntered += () => ItemPlacePointMouseHighlight(true, index);
				new_IPP.MouseExited += () => ItemPlacePointMouseHighlight(false, index);

				ItemPlacePoints.AddChild(new_IPP, true);
				_item_place_points.Add(new_IPP);
				IPP_ind++;
			}

			foreach (var RPPSave in data.road_place_point_saves)
			{
				var new_RPP = RoadPlacePointScene.Instantiate<RoadPlacePoint>();
				new_RPP.Name = RPPSave.point_node_name;
				new_RPP.Position = RPPSave.point_node_position;
				new_RPP.Index = RPPSave.point_node_index;
				
				int index = RPP_ind;
				new_RPP.ID_in_game_list = index;
				new_RPP.MouseEntered += () => RoadPlacePointMouseHighlight(true, index);
				new_RPP.MouseExited += () => RoadPlacePointMouseHighlight(false, index);

				RoadPlacePoints.AddChild(new_RPP, true);
				_road_place_points.Add(new_RPP);
				RPP_ind++;
			}

			new_tile.Rpc(nameof(Tile.RpcSyncTileData), dict, tile_type, tile_number);
		}

		GD.Print("Map loaded and spawned successfully!");
	}

	private void ItemPlacementHandler()
	{
		int item_to_place = _chosen_item;
		int point_index = _last_hovered_point;
		Vector3 placement_position = _new_item_pos;
		int rpp_index = _last_hovered_RPP_ind;
		long placing_player_ID = _placing_player_id;

		if (Multiplayer.IsServer())
		{
			ServerProcessPlacement(item_to_place, point_index, placement_position, rpp_index, placing_player_ID);
		}
		else
		{
			RpcId(1, nameof(RpcRequestItemPlacement), item_to_place, point_index, placement_position, rpp_index, placing_player_ID);
			_chosen_item = -1;
			ForceClearAllPlacementMaterials();
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public void RpcRequestItemPlacement(int chosen_item, int point_index, Vector3 placement_position, int rpp_index, long placing_player_id)
	{
		if (!Multiplayer.IsServer()) return;
		ServerProcessPlacement(chosen_item, point_index, placement_position, rpp_index, placing_player_id);
	}

	private void ServerProcessPlacement(int chosen_item, int point_index, Vector3 placement_position, int rpp_index, long placing_player_ID)
	{
		ItemType chosen_item_type = (ItemType)chosen_item;

		_normal_game.PlayerBuildItem(placing_player_ID, _dict.ItemType_to_Item[chosen_item_type]); //Subtract resources needed for building

		GD.Print("#" + placing_player_ID + "'s current card situation:");
		int[] player_deck = _normal_game.GetPlayerDeck(placing_player_ID);
		for(int i = 0; i < player_deck.Length; i++)
		{
			GD.Print((ResourceCardType)i + " - " + player_deck[i]);
		}
		GD.Print("Player has " + _normal_game.GetPlayerPoints(placing_player_ID) + " points");

		string node_name = "";
		float final_road_rotation = 0.0f;

		switch (chosen_item)
		{
			case 0:
			{
				int index = PlacedItems.GetChildren().Count;
				node_name = $"House_{index}";

				_normal_game.AddIPPToPlayer(placing_player_ID, ItemPlacePoints.GetChild<ItemPlacePoint>(point_index));

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
		Rpc(nameof(RpcBroadcastSpawnItem), chosen_item, point_index, placement_position, node_name, final_road_rotation, placing_player_ID);
		Rpc(nameof(RpcBroadcastClearUI));

		_chosen_item = -1;

		is_item_placed = true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] // CallLocal = true forces host + clients to run this
	public void RpcBroadcastSpawnItem(int chosen_item, int point_index, Vector3 placement_position, string node_name, float road_rotation, long placing_player_ID)
	{
		DeactivatePointLocally(chosen_item == 0, point_index);

		int colour_id = _normal_game.GetColourID(placing_player_ID);

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
		ForceClearAllPlacementMaterials();
	}

	public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed("RMB_click"))
		{
			ForceClearAllPlacementMaterials();
			_chosen_item = -1;

			_hovered_IPP_index = -1;
			_hovered_RPP_index = -1;
			_last_hovered_point = -1;
			_last_hovered_RPP_ind = -1;
		}

		else if (@event.IsActionPressed("LMB_click") && 
			(_hovered_IPP_index != -1 || _hovered_RPP_index != -1))
		{
			AddNewItem();
		}
		else if (@event.IsActionPressed("LMB_click") && 
			_hovered_IPP_index == -1 && _hovered_RPP_index == -1)
		{
			ForceClearAllPlacementMaterials();
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
					_chosen_item = 0;
					ItemPlacePointsChangeMaterial(ItemType.House);
					break;
				}
			case 1:
				{
					GD.Print("Choosing road!");
					_chosen_item = 1;
					RoadPlacePointsChangeMaterial();
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


	//Taken from Tile.cs

	private void ItemPlacePointMouseHighlight(bool toogle_on, int IPP_ind)
	{
		if(!_item_place_points[IPP_ind].IsActive ||
			(!_item_place_points[IPP_ind].IsHousePlacementPermitted
				&& _chosen_item == 0)) return;

		if(IPP_ind >= _item_place_points.Count)
		{
			GD.Print("Wrong tile index! Max index is: " + (_item_place_points.Count-1));
			GD.Print("This index is: " + IPP_ind);
			return;
		}
		if (!_is_IPP_active_for_placement) return;

		if (toogle_on)
		{
			if(_item_place_points[IPP_ind] is Area3D IPP)
			IPP.GetNode<MeshInstance3D>("MeshInstance3D").
				MaterialOverride = ItemPlacePointHighlightedMat;
			_hovered_IPP_index = IPP_ind;
		}
		else
		{
			if(_item_place_points[IPP_ind] is Area3D IPP)
			IPP.GetNode<MeshInstance3D>("MeshInstance3D").
				MaterialOverride = ItemPlacePointsVisibleMat;
			_hovered_IPP_index = -1;
		}
	}

	private void RoadPlacePointMouseHighlight(bool toogle_on, int RPP_ind)
	{
		if(!_road_place_points[RPP_ind].IsActive) return;

		int[] RPPsIDs = _normal_game.GetPlayerRPPsIDs(Multiplayer.GetUniqueId());
		if(!RPPsIDs.Contains(RPP_ind)) return;

		if(RPP_ind >= _road_place_points.Count)
		{
			GD.Print("Wrong tile index! Max index is: " + (_road_place_points.Count-1));
			GD.Print("This index is: " + RPP_ind);
			return;
		}
		if (!_is_RPP_active_for_placement) return;

		if (toogle_on)
		{
			if(_road_place_points[RPP_ind] is Area3D RPP)
			RPP.GetNode<MeshInstance3D>("MeshInstance3D").
				MaterialOverride = ItemPlacePointHighlightedMat;
			_hovered_RPP_index = RPP_ind;
		}
		else
		{
			if(_road_place_points[RPP_ind] is Area3D RPP)
			RPP.GetNode<MeshInstance3D>("MeshInstance3D").
				MaterialOverride = ItemPlacePointsVisibleMat;
			_hovered_RPP_index = -1;
		}
	}

	public void ItemPlacePointsChangeMaterial(ItemType? item_type = null)
	{
		if(_is_RPP_active_for_placement)RoadPlacePointsChangeMaterial();

		Material mat = null;
		if (!_is_IPP_active_for_placement)
		{
			mat = ItemPlacePointsVisibleMat;
			_is_IPP_active_for_placement = true;
		}
		else
		{
			mat = ItemPlacePointsInvisibleMat;
			_is_IPP_active_for_placement = false;
		}

		foreach(ItemPlacePoint IPP in _item_place_points)
		{
			if((!IPP.IsActive && _is_IPP_active_for_placement)
				|| (item_type == ItemType.House && !IPP.IsHousePlacementPermitted 
					&& _is_IPP_active_for_placement))
					continue;
			IPP.GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = mat;
		}
	}

	public void RoadPlacePointsChangeMaterial()
	{
		if(_is_IPP_active_for_placement)ItemPlacePointsChangeMaterial();

		int[] RPPsIDs = _normal_game.GetPlayerRPPsIDs(_placing_player_id);

		GD.Print("RPPsIDs length = " + RPPsIDs.Length);

		Material mat = null;
		if (!_is_RPP_active_for_placement)
		{
			mat = ItemPlacePointsVisibleMat;
			_is_RPP_active_for_placement = true;
		}
		else
		{
			mat = ItemPlacePointsInvisibleMat;
			_is_RPP_active_for_placement = false;
		}

		foreach(int RPP_ID in RPPsIDs)
		{
			if(!_road_place_points[RPP_ID].IsActive && _is_RPP_active_for_placement)continue;
			_road_place_points[RPP_ID].GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = mat;
		}
	}

	private void AddNewItem()
	{
		if(_hovered_IPP_index != -1)
		{
			_last_hovered_point = _hovered_IPP_index;
			var point = _item_place_points[_last_hovered_point];
			_new_item_pos = point.GlobalPosition;
			ItemPlacePointMouseHighlight(false, _last_hovered_point);
			_item_place_points[_last_hovered_point].IsActive = false;
		}
		else
		{
			_last_hovered_point = _hovered_RPP_index;
			var point = _road_place_points[_last_hovered_point];
			_new_item_pos = point.GlobalPosition;
			RoadPlacePointMouseHighlight(false, _last_hovered_point);
			_road_place_points[_last_hovered_point].IsActive = false;
			_last_hovered_RPP_ind = _road_place_points[_last_hovered_point].Index;
		}

		ItemPlacementHandler();
	}

	public void ForceClearAllPlacementMaterials()
	{
		_is_IPP_active_for_placement = false;
		_is_RPP_active_for_placement = false;
		_hovered_IPP_index = -1;
		_hovered_RPP_index = -1;
		_last_hovered_point = -1;
		_last_hovered_RPP_ind = -1;

		foreach(ItemPlacePoint IPP in _item_place_points)
		{
			IPP.GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = ItemPlacePointsInvisibleMat;
		}
		
		foreach(RoadPlacePoint RPP in _road_place_points)
		{
			RPP.GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = ItemPlacePointsInvisibleMat;
		}
	}

	public void DeactivatePointLocally(bool isItemPoint, int index)
	{
		if (isItemPoint)
		{
			if (index >= 0 && index < _item_place_points.Count)
			{
				_item_place_points[index].IsActive = false;
			}
		}
		else
		{
			if (index >= 0 && index < _road_place_points.Count)
			{
				_road_place_points[index].IsActive = false;
			}
		}
	}

}
