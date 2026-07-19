using Godot;
using System;

using System.Collections.Generic;
using HelperScripts;

public partial class NormalGameUi : Control
{
	[Export] public ItemList ObjectList;
	private NormalGame _normal_game;
	private MultiplayerWorld _multiplayer_world;
	private NormalGamePlayer _normal_game_player;
	private bool _is_item_chosen = false;
	private long _player_ID = 0;

	private ColourList _colour_list = new ColourList();
	

    public override void _Ready()
    {
		_normal_game_player = GetParent<NormalGamePlayer>();
        _normal_game = GetParent().GetParent<NormalGame>();
		_multiplayer_world = GetNodeOrNull<MultiplayerWorld>("/root/MultiplayerWorld");

		_player_ID = _normal_game_player.player_ID;

		if(_player_ID != Multiplayer.GetUniqueId())
		{
			QueueFree();
			return;
		}

		GD.Print("My player id is " + _player_ID);

		StyleBoxFlat style = new StyleBoxFlat();
		Color new_bg_colour = _colour_list.player_colors[_multiplayer_world.GetColourID(_player_ID)];
		style.BgColor = new Color(new_bg_colour.R, new_bg_colour.G, new_bg_colour.B, 0.42f);
		ObjectList.AddThemeStyleboxOverride("panel", style);
    }


	public override void _Input(InputEvent @event)
    {
		if (Input.IsActionJustPressed("RMB_click"))
		{
			ObjectListDeselect();
		}

		else if (Input.IsActionJustPressed("LMB_click") && _is_item_chosen == true)
		{
			ObjectListDeselect();
		}
    }

	public void LoadGameBtnPressed()
	{
		_normal_game.LoadMapBase("res://Dexolonists_map.bin");
		
	}

	public void ExitNGBtnPressed()
	{
		_normal_game.ExitToMainMenu();
	}

	public void NextTurnBtnPressed()
	{
		_normal_game.NextTurn();
	}

	public void ObjectListItemChosen(int index)
	{
		_is_item_chosen = true;
		GD.Print("Player " + _player_ID + " has chosen item with index " + index);
		_normal_game.ItemChosen(index, _player_ID);
	}

	public void ObjectListDeselect()
	{
		ObjectList.DeselectAll();
		_is_item_chosen = false;	
	}
}
