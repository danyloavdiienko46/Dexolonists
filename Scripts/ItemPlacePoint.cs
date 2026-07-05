using Godot;
using System;

public partial class ItemPlacePoint : Area3D
{
	[Export] public bool IsActive {get; set;} = true;

	[Export] public bool IsHousePlacementPermitted {get; set;} = true;
}
