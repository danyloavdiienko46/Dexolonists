using Godot;
using System;

public partial class RoadPlacePoint : Area3D
{
	[Export] public bool IsActive {get; set;} = true;
	public int Index {get; set;} = -1;

	public int ID_in_game_list = -1;
}
