using dexolonists.Scripts.Enums;
using Godot;
using System;
using System.Collections.Generic;
using HelperScripts;

public partial class Road : Item
{
	public int colour_index;
	private MeshInstance3D _road_mesh;

	private ColourList _colour_list = new ColourList();

    private int[] _building_cost = [0, 0, 0, 0, 0];

    public override void _Ready()
    {
        _road_mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		ChangeObjectMeshColour(colour_index);

        SetBuildingCost();
    }

    private void SetBuildingCost()
	{
		_building_cost[(int)ResourceCardType.Wood] = 1;
		_building_cost[(int)ResourceCardType.Brick] = 1;
		_building_cost[(int)ResourceCardType.Wheat] = 0;
		_building_cost[(int)ResourceCardType.Wool] = 0;
		_building_cost[(int)ResourceCardType.Ore] = 0;
	}

    public override int[] GetBuildingCost()
    {
        return _building_cost;
    }

	public void ChangeObjectMeshColour(int colour_index)
	{
        StandardMaterial3D mat = new StandardMaterial3D();
        mat.AlbedoColor = _colour_list.player_colors[colour_index];
        _road_mesh.MaterialOverride = mat;
	}
}
