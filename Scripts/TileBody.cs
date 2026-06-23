using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class TileBody : StaticBody3D
{
	[Export] public Node3D TilePlacePoints {get;set;}
	[Export] public Node3D ItemPlacePoints {get;set;}
	[Export] public Material TilePlacePointsActiveMat;
	[Export] public Material TilePlacePointsMat;
	[Export] public Material TilePlacePointsActiveHoverMat;
	private Godot.Collections.Array<Node> _tile_place_points;
	private Godot.Collections.Array<Node> _item_place_points;
	private bool _is_adding_new_tile_enabled = false;
	private int _hovered_tile_place_point_index = -1;
	public int last_hovered_TPP = -1;
	public bool new_tile_signal = false;
	public bool new_tile_created = false;
	private bool _cleaning_timer_created = false; //need this for cleaning TPPs
	public bool just_created = false; //also need for clear cleanance of TPPs

	private Random rnd = new Random();
	public override void _Ready()
	{
		_tile_place_points = TilePlacePoints.GetChildren();
		_item_place_points = ItemPlacePoints.GetChildren();

		for(int i = 0; i < _tile_place_points.Count; i++)
		{
			if(_tile_place_points[i] is Area3D tile_place_point)
			{
				int index = i;
				tile_place_point.MouseEntered += () => TilePlacePointMouseHighlight(true, index);
				tile_place_point.MouseExited += () => TilePlacePointMouseHighlight(false, index);
			}
		}
	}

    public override void _PhysicsProcess(double delta)
    {
		if (new_tile_created)
		{
			if (!_cleaning_timer_created)
			{
				_cleaning_timer_created = true;
				var timer = GetTree().CreateTimer(0.1);
				timer.Timeout += () => {
					new_tile_created = false;
					_cleaning_timer_created = false;
					if(just_created)just_created = false;
					};
			}
			for(int i = 0; i < _tile_place_points.Count; i++)
			{
				if(_tile_place_points[i] is Area3D TPP)
				{
					if (TPP.Monitorable == false) continue;
					if(TPP.GetOverlappingAreas().Count != 0 || TPP.GetOverlappingBodies().Count != 0)
					{
						DisableTPP(i, !just_created);
					}
				}
			}
		}
    }


    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("LMB_click") && _hovered_tile_place_point_index != -1)
		{
			AddNewTile();
		}
    }


	private void AddNewTile()
	{
		GD.Print("Placing new Tile as a neighbour of point " + _hovered_tile_place_point_index);
		last_hovered_TPP = _hovered_tile_place_point_index;
		new_tile_signal = true;
		DisableTPP(last_hovered_TPP, true);
	}

	public void DisableTPP(int index, bool disable_IPPs)
	{
		if(_tile_place_points[index] is Area3D tile_place_point)
		{
			tile_place_point.Visible = false;
			tile_place_point.InputRayPickable = false;
			tile_place_point.Monitorable = false;
			tile_place_point.Monitoring = false;

			if (disable_IPPs)
			{
				(_item_place_points[index] as Area3D).Visible = false; //making item place point invisible
				(_item_place_points[(index+1)%6] as Area3D).Visible = false; //making second item place point invisible by adding one and operating only on values less than 6
			}
		}
	}
	private void TilePlacePointMouseHighlight(bool toogle_on, int tile_ind)
	{
		if(tile_ind >= _tile_place_points.Count)
		{
			GD.Print("Wrong tile index! Max index is: " + (_tile_place_points.Count-1));
			GD.Print("This index is: " + tile_ind);
			return;
		}
		if (!_is_adding_new_tile_enabled) return;

		if (toogle_on)
		{
			if(_tile_place_points[tile_ind] is Area3D tile_place_point)
			tile_place_point.GetNode<MeshInstance3D>("MeshInstance3D").
				MaterialOverride = TilePlacePointsActiveHoverMat;
			_hovered_tile_place_point_index = tile_ind;
		}
		else
		{
			if(_tile_place_points[tile_ind] is Area3D tile_place_point)
			tile_place_point.GetNode<MeshInstance3D>("MeshInstance3D").
				MaterialOverride = TilePlacePointsActiveMat;
			_hovered_tile_place_point_index = -1;
		}
	}

	public void TilePlacePointsChangeMaterial(bool active)
	{
		Material mat = null;
		_is_adding_new_tile_enabled = active;
		if (active)
		{
			mat = TilePlacePointsActiveMat;
		}
		else
		{
			mat = TilePlacePointsMat;
		}

		foreach(Node tile_place_point_as_node in _tile_place_points)
		{
			if(tile_place_point_as_node is Area3D tile_place_point)
			{
				tile_place_point.GetNode<MeshInstance3D>("MeshInstance3D").
					MaterialOverride = mat;
			}
		}
	}
}
