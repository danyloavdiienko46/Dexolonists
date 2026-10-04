using Godot;
using System;
using System.Collections.Generic;

public partial class RoadPlacePoint : Area3D
{
	[Export] public bool IsActive {get; set;} = true;
	public int Index {get; set;} = -1;
	public bool CanBeUsedInSetup {get; set;} = true;

	public List<ItemPlacePoint> connected_IPPs_list = [];

	public int ID_in_game_list = -1;

	
}
