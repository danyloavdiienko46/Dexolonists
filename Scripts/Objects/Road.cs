using Godot;
using System;
using System.Collections.Generic;
using HelperScripts;

public partial class Road : StaticBody3D
{
	public int colour_index;
	private MeshInstance3D _road_mesh;

	private ColourList _colour_list = new ColourList();

    public override void _Ready()
    {
        _road_mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		ChangeObjectMeshColour(colour_index);
    }


	public void ChangeObjectMeshColour(int colour_index)
	{
        StandardMaterial3D mat = new StandardMaterial3D();
        mat.AlbedoColor = _colour_list.player_colors[colour_index];
        _road_mesh.MaterialOverride = mat;
	}
}
