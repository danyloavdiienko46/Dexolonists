using Godot;
using System;

public partial class NormalGameUi : Control
{
	[Export] public ItemList ObjectList;
	private NormalGame _normal_game;
	private MultiplayerWorld _multiplayer_world;

    public override void _Ready()
    {
        _normal_game = GetParent().GetParent<NormalGame>();
		_multiplayer_world = GetNodeOrNull<MultiplayerWorld>("/root/MultiplayerWorld");
    }

	public override void _Input(InputEvent @event)
    {
		if (Input.IsActionJustPressed("RMB_click"))
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
		if (Multiplayer.MultiplayerPeer != null && 
        	Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected)
		{
			GD.Print("Doing something for server!");
			_normal_game.ItemChosen(index);
			//_multiplayer_world.RequestPlaceTile(targetPosition, selectedTileType);
		}
		else
		{
			GD.Print("Chosen item is " + index);
			_normal_game.ItemChosen(index);
		}
	}

	public void ObjectListDeselect()
	{
		ObjectList.DeselectAll();
	}
}
