using Godot;
using System;

public partial class NormalGamePlayer : Node3D
{
    [Export] public float MoveSpeed = 0.6f;
    [Export] public float RotateKeysSpeed = 1.5f;
    [Export] public float MouseSensitivity = 5.0f;

    private Vector3 _move_target;
    private float _rotate_y_target;
    private float _rotate_x_target;

    public override void _EnterTree()
    {
        int id = 0;
		Int32.TryParse(Name, out id);
		SetMultiplayerAuthority(id);
    }

    public override void _Ready()
    {
        _move_target = Position;
        _rotate_y_target = RotationDegrees.Y;
        _rotate_x_target = RotationDegrees.X;

		var camera = GetNodeOrNull<Camera3D>("Camera3D");
		if (IsMultiplayerAuthority())
        {
            camera.MakeCurrent(); 
        }
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