using Godot;
using System;

public partial class House : StaticBody3D
{
	[Export] public PackedScene HouseBlockingAreaScene;
	private Area3D _house_blocking_area;
	private bool _just_created = false;
	public override void _Ready()
	{
		_house_blocking_area = HouseBlockingAreaScene.Instantiate<Area3D>();
		AddChild(_house_blocking_area);
		_just_created = true;
	}

	public override void _PhysicsProcess(double delta)
    {
		if (_just_created)
		{
			var IPPs_for_house_placement_deactivation = _house_blocking_area.GetOverlappingAreas();
			if(IPPs_for_house_placement_deactivation.Count >= 2)
			{
				GD.Print("Deactivating IPPs for house placement!");
				for(int i = 0; i < IPPs_for_house_placement_deactivation.Count; i++)
				{
					if(IPPs_for_house_placement_deactivation[i] is ItemPlacePoint IPP)
					{
						IPP.IsHousePlacementPermitted = false;
					}
				}
				_just_created = false;
				GD.Print("Finished deactivating IPPs for house placement!");
			}

		}
    }
}
