using Godot;
using System;

public partial class NormalGameUi : Control
{
	[Export] public ItemList ObjectList;
	private NormalGame _normal_game;
	private MultiplayerWorld _multiplayer_world;
	private bool _is_item_chosen = false;

    public override void _Ready()
    {
        _normal_game = GetParent().GetParent<NormalGame>();
		_multiplayer_world = GetNodeOrNull<MultiplayerWorld>("/root/MultiplayerWorld");

    }


	public override void _Input(InputEvent @event)
    {
		//if(!IsMultiplayerAuthority()) return;
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

	public void ObjectListItemChosen(int index)
	{
		_is_item_chosen = true;
		GD.Print("Chosen item is " + index);
		_normal_game.ItemChosen(index);
	}

	public void ObjectListDeselect()
	{
		ObjectList.DeselectAll();
		_is_item_chosen = false;	
	}
}
