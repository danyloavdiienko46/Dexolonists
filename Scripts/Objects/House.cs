using Godot;
using System;
using System.Collections.Generic;
using HelperScripts;
using dexolonists.Scripts.Enums;

public partial class House : Item
{
	[Export] public PackedScene HouseBlockingAreaScene;
	public int colour_index = -1;
	private Area3D _house_blocking_area;
	private bool _just_created = false;

	private MeshInstance3D _house_base;
	private MeshInstance3D _house_roof;

	private ColourList _colour_list = new ColourList();

	private int[] _building_cost = [0, 0, 0, 0, 0];
	public int point_addition = 1;

	public override void _Ready()
	{
		_house_base = GetNode<MeshInstance3D>("Base");
		_house_roof = GetNode<MeshInstance3D>("Roof");

		ChangeObjectMeshColour(colour_index);

		if(!Multiplayer.IsServer()) return;
		_house_blocking_area = HouseBlockingAreaScene.Instantiate<Area3D>();
		AddChild(_house_blocking_area);
		_just_created = true;

		SetBuildingCost();
	}

	private void SetBuildingCost()
	{
		_building_cost[(int)ResourceCardType.Wood] = 1;
		_building_cost[(int)ResourceCardType.Brick] = 1;
		_building_cost[(int)ResourceCardType.Wheat] = 1;
		_building_cost[(int)ResourceCardType.Wool] = 1;
		_building_cost[(int)ResourceCardType.Ore] = 0;
	}

	public override int[] GetBuildingCost()
    {
        return _building_cost;
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
        mat.AlbedoColor = _colour_list.player_colors[colour_index];
		if(_house_base != null) _house_base.MaterialOverride = mat;
		else GD.Print("House Base is null!");

		if(_house_roof != null) _house_roof.MaterialOverride = mat;
		else GD.Print("House Roof is null!");
	}
}
