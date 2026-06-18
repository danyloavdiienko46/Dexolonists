using Godot;
using System;

public partial class MapCreatorToolBox : Control
{
	// Called when the node enters the scene tree for the first time.
	private MapCreator _map_creator;
	[Export] Button AddTileBtn;
	[Export] Button DeleteTileBtn;
	public override void _Ready()
	{
		_map_creator = GetOwner<MapCreator>();
	}

	public void AddNewTileBtnPressed(bool toggled_on)
	{
		AddTileBtn.ButtonPressed = toggled_on;
		if(toggled_on) DeleteTileBtnPressed(false);
		_map_creator.AddTileMode(toggled_on);
	}

	public void DeleteTileBtnPressed(bool toggled_on)
	{
		DeleteTileBtn.ButtonPressed = toggled_on;
		if(toggled_on) AddNewTileBtnPressed(false);
		_map_creator.DeleteTileMode(toggled_on);
		
	}
}
