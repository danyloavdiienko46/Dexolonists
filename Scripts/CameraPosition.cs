using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class CameraPosition : Camera3D
{
	[Export] public float MoveSpeed = 0.6f;
	[Export] public float RotateKeysSpeed = 1.5f;
	[Export] public float MouseSensitivity = 0.3f;

	private Vector3 _move_target;
	private float _rotate_keys_target;

	public override void _Ready()
	{
		_move_target = Position;
		_rotate_keys_target = RotationDegrees.Y;
	}

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion ev && Input.IsActionPressed("rotate"))
		{
			_rotate_keys_target -= ev.Relative.X * MouseSensitivity;

			Vector3 rot_degrees = RotationDegrees;
			rot_degrees.X -= ev.Relative.Y * MouseSensitivity;
			rot_degrees.X = Mathf.Clamp(rot_degrees.X, -90, 90);
			RotationDegrees = rot_degrees;
		}
    }


	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("rotate"))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else if (Input.IsActionJustReleased("rotate"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		Vector2 input_direction = Input.GetVector("left", "right", "up", "down");
		Vector3 movement_direction = 
		(Transform.Basis * new Vector3(input_direction.X, 0, input_direction.Y)).Normalized();

		float rotate_keys = Input.GetAxis("rotate_left", "rotate_right");

		_move_target += movement_direction*MoveSpeed;
		_rotate_keys_target += rotate_keys*RotateKeysSpeed;

		Position = Position.Lerp(_move_target, 0.25f);

		Vector3 rot = RotationDegrees;
		rot.Y = Mathf.Lerp(rot.Y, _rotate_keys_target, 1.0f);
		RotationDegrees = rot;
	}
}
