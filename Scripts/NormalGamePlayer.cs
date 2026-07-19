using Godot;
using HelperScripts;
using System;

using System.Collections.Generic;

public partial class NormalGamePlayer : Node3D
{
    [Export] public float MoveSpeed = 0.6f;
    [Export] public float RotateKeysSpeed = 1.5f;
    [Export] public float MouseSensitivity = 5.0f;
    private ColourList _colour_list = new ColourList();

    private MultiplayerWorld _mp_world;

    private int _color_index = -1;
    private Vector3 _move_target;
    private float _rotate_y_target;
    private float _rotate_x_target;

    public long player_ID = -1;

    public override void _EnterTree()
    {
        int id = 0;
		Int32.TryParse(Name, out id);
		SetMultiplayerAuthority(id, recursive: true);

        player_ID = id;
    }

    public override void _Ready()
    {
        Position = new Vector3(0, 10.551f, 7.765f);

        _move_target = Position;
        _rotate_y_target = RotationDegrees.Y;
        _rotate_x_target = RotationDegrees.X;

		var camera = GetNodeOrNull<Camera3D>("Camera3D");
		if (IsMultiplayerAuthority())
        {
            camera.MakeCurrent(); 
        }

        if (Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer)
        {
            _mp_world = GetParent().GetParent<MultiplayerWorld>();
            _color_index = _mp_world.GetColourID(player_ID);
        }
        
        ChangeBodyMeshColour();

    }

    public void ChangeBodyMeshColour()
    {
        if (_color_index < 0 || _color_index >= _colour_list.player_colors.Count) 
        {
            GD.Print("Wrong index for colouring, brother!");
            return;
        }

        MeshInstance3D body_mesh = GetNode<MeshInstance3D>("BodyMesh");
        StandardMaterial3D mat = new StandardMaterial3D();
        mat.AlbedoColor = _colour_list.player_colors[_color_index];
        body_mesh.MaterialOverride = mat;

        GD.Print("Changed color for player #" + player_ID);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
		if (!IsMultiplayerAuthority() && Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer) return;
        if (@event is InputEventMouseMotion ev && Input.IsActionPressed("rotate"))
        {
            _rotate_y_target -= ev.Relative.X * MouseSensitivity;
            _rotate_x_target -= ev.Relative.Y * MouseSensitivity;
            _rotate_x_target = Mathf.Clamp(_rotate_x_target, -90, 90);
        }
    }

    public override void _Process(double delta)
    {
		if (!IsMultiplayerAuthority() && Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer) return;
        if (Input.IsActionJustPressed("rotate"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else if (Input.IsActionJustReleased("rotate"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        Vector2 input_direction = Input.GetVector("left", "right", "up", "down");
        Vector3 movement_direction = (Transform.Basis.Orthonormalized() * new Vector3(input_direction.X, 0, input_direction.Y)).Normalized();
        _move_target += movement_direction * MoveSpeed;
        Position = Position.Lerp(_move_target, 0.25f);

        Vector3 rot = RotationDegrees;
        rot.Y = _rotate_y_target;
        rot.X = _rotate_x_target;
        RotationDegrees = rot;
    }
}