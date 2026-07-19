using Godot;
using System;
using System.Collections.Generic;

public partial class House : StaticBody3D
{
	[Export] public PackedScene HouseBlockingAreaScene;
	public int colour_index = -1;
	private Area3D _house_blocking_area;
	private bool _just_created = false;

	private MeshInstance3D _house_base;
	private MeshInstance3D _house_roof;

	private readonly List<Color> _player_colors = new()
    {
        Colors.Red,
        Colors.Blue,
        Colors.Green,
        Colors.Yellow,
        Colors.Purple,
        Colors.Orange
    };

	public override void _Ready()
	{
		_house_base = GetNode<MeshInstance3D>("Base");
		_house_roof = GetNode<MeshInstance3D>("Roof");

		ChangeObjectMeshColour(colour_index);

		if(!Multiplayer.IsServer()) return;
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

	public void ChangeObjectMeshColour(int colour_index)
	{
        StandardMaterial3D mat = new StandardMaterial3D();
        mat.AlbedoColor = _player_colors[colour_index];
		if(_house_base != null) _house_base.MaterialOverride = mat;
		else GD.Print("House Base is null!");

		if(_house_roof != null) _house_roof.MaterialOverride = mat;
		else GD.Print("House Roof is null!");
	}
}
