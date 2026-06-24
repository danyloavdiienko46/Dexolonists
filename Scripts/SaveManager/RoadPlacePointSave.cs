using Godot;
using System;

public partial class RoadPlacePointSave
{
	public string point_node_name {get; set;}
	public Vector3 point_node_position {get; set;}

	public Godot.Collections.Dictionary ToDictionary()
	{
		return new Godot.Collections.Dictionary
		{
			{"point_name", point_node_name},
			{"point_position", point_node_position}
		};
	}

	public static RoadPlacePointSave FromDictionary(Godot.Collections.Dictionary dict)
	{
		return new RoadPlacePointSave
		{
			point_node_name = (string)dict["point_name"],
			point_node_position = (Vector3)dict["point_position"]
		};
	}
}
