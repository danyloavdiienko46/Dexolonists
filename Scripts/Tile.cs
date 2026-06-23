using Godot;
using System;

public partial class Tile : StaticBody3D
{
	[Export] public Node3D ItemPlacePoints {get;set;}
	[Export] public PackedScene ItemPlacePointScene {get; set;}
	[Export] public Label3D TypeLabel;
	private Godot.Collections.Array<Node> _item_place_points;
	public TileType type = 0;
	
	public void LoadData(TileSave data)
    {
        this.Position = data.Position;
		type = (TileType)data.Type;

		TypeLabel.Text = type.ToString();

        GD.Print("Loading Data...");
        foreach (var IPPSave in data.item_place_point_saves)
        {
			GD.Print("IPP doing smth...");
            var new_IPP = ItemPlacePointScene.Instantiate<Area3D>();
			new_IPP.Name = IPPSave.point_node_name;
			new_IPP.Position = IPPSave.point_node_position;
			ItemPlacePoints.AddChild(new_IPP, true);
        }
    }
}
