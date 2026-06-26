using Godot;
using HelperScripts;
using System;

public partial class Tile : StaticBody3D
{
	[Export] public Node3D ItemPlacePoints {get;set;}
	[Export] public Node3D RoadPlacePoints {get;set;}
	[Export] public PackedScene ItemPlacePointScene {get; set;}
	[Export] public PackedScene RoadPlacePointScene {get; set;}
	[Export] public Material ItemPlacePointsVisibleMat;
	[Export] public Material ItemPlacePointsInvisibleMat;
	[Export] public Material ItemPlacePointHighlightedMat;
	[Export] public Label3D TypeLabel;
	[Export] public Label3D NumberLabel;
	[Export] public Label3D ChancesLabel;
	private Godot.Collections.Array<ItemPlacePoint> _item_place_points = new Godot.Collections.Array<ItemPlacePoint>();
	private Godot.Collections.Array<RoadPlacePoint> _road_place_points = new Godot.Collections.Array<RoadPlacePoint>();
	public TileType type = 0;
	public int number = -1;
	private bool _is_IPP_active_for_placement = false;
	private bool _is_RPP_active_for_placement = false;
	private int _hovered_IPP_index = -1;
	private int _hovered_RPP_index = -1;
	public int last_hovered_point = -1;
	public int last_hovered_RPP_ind = -1;
	public bool new_item_signal = false;
	public Vector3 new_item_pos = new Vector3(-1, -1, -1);

	private Random _rand = new Random();

	private Dictionaries _dict = new Dictionaries();
	
	public void ItemPlacePointsChangeMaterial()
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
			if(!IPP.IsActive && _is_IPP_active_for_placement)continue;
			IPP.GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = mat;
		}
	}

	public void RoadPlacePointsChangeMaterial()
	{
		if(_is_IPP_active_for_placement)ItemPlacePointsChangeMaterial();

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

		foreach(RoadPlacePoint RPP in _road_place_points)
		{
			if(!RPP.IsActive && _is_RPP_active_for_placement)continue;
			RPP.GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = mat;
		}
	}

	private void ItemPlacePointMouseHighlight(bool toogle_on, int IPP_ind)
	{
		if(!_item_place_points[IPP_ind].IsActive) return;

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

	public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("LMB_click") && 
			(_hovered_IPP_index != -1 || _hovered_RPP_index != -1))
		{
			AddNewItem();
		}

		else if (Input.IsActionJustPressed("RMB_click"))
		{
			_hovered_IPP_index = -1;
			_hovered_RPP_index = -1;
			last_hovered_point = -1;
			last_hovered_RPP_ind = -1;
		}
    }
	private void AddNewItem()
	{
		GD.Print("Placing new Item for Tile: " + this.Name);

		if(_hovered_IPP_index != -1)
		{
			last_hovered_point = _hovered_IPP_index;
			var point = _item_place_points[last_hovered_point];
			new_item_pos = point.GlobalPosition;
			ItemPlacePointMouseHighlight(false, last_hovered_point);
			_item_place_points[last_hovered_point].IsActive = false;
		}
		else
		{
			last_hovered_point = _hovered_RPP_index;
			var point = _road_place_points[last_hovered_point];
			new_item_pos = point.GlobalPosition;
			RoadPlacePointMouseHighlight(false, last_hovered_point);
			_road_place_points[last_hovered_point].IsActive = false;
			last_hovered_RPP_ind = _road_place_points[last_hovered_point].Index;
		}
		new_item_signal = true;
	}

	public void LoadData(TileSave data)
    {
        this.Position = data.Position;
		type = (TileType)_rand.Next(1, 6);
		while(number == -1 || number == 7) number = _rand.Next(2, 13);
		int chances = _dict.Tile_number_to_chances[number];
		string chances_str = ".";

		TypeLabel.Text = type.ToString();
		TypeLabel.Modulate = new Color(_dict.TileType_to_colour_code[type]);

		NumberLabel.Text = number.ToString();


		for(int i = 1; i < chances; i++)
		{
			chances_str+=".";
		}
		ChancesLabel.Text = chances_str;

		int ind = 0;
        foreach (var IPPSave in data.item_place_point_saves)
        {
            var new_IPP = ItemPlacePointScene.Instantiate<ItemPlacePoint>();
			new_IPP.Name = IPPSave.point_node_name;
			new_IPP.Position = IPPSave.point_node_position;

			int index = ind;
			new_IPP.MouseEntered += () => ItemPlacePointMouseHighlight(true, index);
			new_IPP.MouseExited += () => ItemPlacePointMouseHighlight(false, index);

			ItemPlacePoints.AddChild(new_IPP, true);
			_item_place_points.Add(new_IPP);
			ind++;
        }

		ind = 0;
		foreach (var RPPSave in data.road_place_point_saves)
        {
            var new_RPP = RoadPlacePointScene.Instantiate<RoadPlacePoint>();
			new_RPP.Name = RPPSave.point_node_name;
			new_RPP.Position = RPPSave.point_node_position;
			new_RPP.Index = RPPSave.point_node_index;

			int index = ind;
			new_RPP.MouseEntered += () => RoadPlacePointMouseHighlight(true, index);
			new_RPP.MouseExited += () => RoadPlacePointMouseHighlight(false, index);

			RoadPlacePoints.AddChild(new_RPP, true);
			_road_place_points.Add(new_RPP);
			ind++;
        }

    }
}
