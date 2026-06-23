using Godot;
using System;

public partial class NormalGameUi : Control
{
	private NormalGame _normal_game;

    public override void _Ready()
    {
        _normal_game = GetOwner<NormalGame>();
    }

	public void LoadGameBtnPressed()
	{
		_normal_game.LoadMapBase("res://Dexolonists_map.bin");
	}
}
