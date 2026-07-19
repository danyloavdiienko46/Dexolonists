using Godot;
using System;
using System.Collections.Generic;

public partial class Road : StaticBody3D
{
	public int colour_index;
	private MeshInstance3D _road_mesh;

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
        _road_mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		ChangeObjectMeshColour(colour_index);
    }


	public void ChangeObjectMeshColour(int colour_index)
	{
        StandardMaterial3D mat = new StandardMaterial3D();
        mat.AlbedoColor = _player_colors[colour_index];
        _road_mesh.MaterialOverride = mat;
	}
}
