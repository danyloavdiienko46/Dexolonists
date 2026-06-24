using Godot;
using System;

public partial class Tile : StaticBody3D
{
	[Export] public Node3D ItemPlacePoints {get;set;}
	[Export] public Node3D RoadPlacePoints {get;set;}
	[Export] public PackedScene ItemPlacePointScene {get; set;}
	[Export] public PackedScene RoadPlacePointScene {get; set;}
	[Export] public Material ItemPlacePointsVisibleMat;
	[Export] public Material ItemPlacePointsInvisibleMat;
	[Export] public Label3D TypeLabel;
	private Godot.Collections.Array<Node> _item_place_points;
	private Godot.Collections.Array<Node> _road_place_points;
	public TileType type = 0;
	
	public void ItemPlacePointsChangeMaterial(bool active)
	{
		Material mat = null;
		//_is_adding_new_tile_enabled = active;
		if (active)
		{
			mat = ItemPlacePointsVisibleMat;
		}
		else
		{
			mat = ItemPlacePointsInvisibleMat;
		}

		foreach(Node IPPnode in _item_place_points)
		{
			if(IPPnode is Area3D IPP)
			{
				IPP.GetNode<MeshInstance3D>("MeshInstance3D").
					MaterialOverride = mat;
			}
		}
	}

	public void LoadData(TileSave data)
    {
        this.Position = data.Position;
		type = (TileType)data.Type;

		TypeLabel.Text = type.ToString();

        foreach (var IPPSave in data.item_place_point_saves)
        {
            var new_IPP = ItemPlacePointScene.Instantiate<Area3D>();
			new_IPP.Name = IPPSave.point_node_name;
			new_IPP.Position = IPPSave.point_node_position;
			ItemPlacePoints.AddChild(new_IPP, true);
        }

		_item_place_points = ItemPlacePoints.GetChildren();

		foreach (var RPPSave in data.road_place_point_saves)
        {
            var new_RPP = RoadPlacePointScene.Instantiate<Area3D>();
			new_RPP.Name = RPPSave.point_node_name;
			new_RPP.Position = RPPSave.point_node_position;
			RoadPlacePoints.AddChild(new_RPP, true);
        }

		_road_place_points = RoadPlacePoints.GetChildren();
    }
}
