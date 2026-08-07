using Godot;
using System;

using System.Collections.Generic;
using HelperScripts;

public partial class NormalGameUi : Control
{
	[Export] public ItemList ObjectList;
	[Export] public Panel DeckPanel;
	[Export] public Panel PointsPanel;
	private NormalGame _normal_game;
	private MultiplayerWorld _multiplayer_world;
	private NormalGamePlayer _normal_game_player;
	private bool _is_item_chosen = false;
	private long _player_ID = 0;

	private ColourList _colour_list = new ColourList();
	private Dictionaries _dict = new Dictionaries();
	

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

	public void EnableItemChoosement()
	{
		if(!IsInstanceValid(ObjectList)) return;

		for(int i = 0; i < ObjectList.ItemCount; i++)
		{
			ObjectList.SetItemDisabled(i, false);
		}
		
	}

	public override void _Input(InputEvent @event)
    {
		if (_normal_game_player.player_info_holder.is_forced_building_enabled)
		{
			return;
		}

		if (Input.IsActionJustPressed("RMB_click"))
		{
			ObjectListDeselect();
		}

		/*
		else if (Input.IsActionJustPressed("LMB_click") && _is_item_chosen == true)
		{
			ObjectListDeselect();
		}*/
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
		if(Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer || _normal_game.GetCurrentTurnID() == _player_ID)
		{
			if (!_normal_game_player.player_info_holder.CanPlayerBuildItem(_dict.ItemType_to_Item[(ItemType)index], (ItemType)index))
			{
				GD.Print("Player #" + _player_ID + " has no enough resources to build that item!");
				ObjectListDeselect();
				return;
			}

			_is_item_chosen = true;
			GD.Print("Player " + _player_ID + " has chosen item with index " + index);
			_normal_game.ItemChosen(index, _player_ID);
		}
		else
		{
			GD.Print("Not your turn, brother in Christ! Current turn is for ID: " + _normal_game.GetCurrentTurnID());
		}
	}

	public void UpdateDeckPanel()
	{
		int[] deck = _normal_game_player.player_info_holder.GetCardArray();

		DeckPanel.GetNode<VBoxContainer>("TreeCards").GetNode<Label>("Label").Text = deck[0].ToString();
		DeckPanel.GetNode<VBoxContainer>("BrickCards").GetNode<Label>("Label").Text = deck[1].ToString();
		DeckPanel.GetNode<VBoxContainer>("WheatCards").GetNode<Label>("Label").Text = deck[2].ToString();
		DeckPanel.GetNode<VBoxContainer>("SheepCards").GetNode<Label>("Label").Text = deck[3].ToString();
		DeckPanel.GetNode<VBoxContainer>("OreCards").GetNode<Label>("Label").Text = deck[4].ToString();
	}

	public void UpdatePointsPanel()
	{
		PointsPanel.GetNode<Label>("PointsLabel").Text = _normal_game_player.player_info_holder.GetPoints().ToString();
	}

	public void ObjectListDeselect()
	{
		ObjectList.DeselectAll();
		_is_item_chosen = false;	
	}
}
