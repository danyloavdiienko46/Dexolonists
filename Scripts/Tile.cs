using Godot;
using HelperScripts;
using System;

public partial class Tile : StaticBody3D
{
	[Export] public Label3D TypeLabel;
	[Export] public Label3D NumberLabel;
	[Export] public Label3D ChancesLabel;
	public TileType type = 0;
	public int number = -1;

	private Random _rand = new Random();

	private Dictionaries _dict = new Dictionaries();

	public void LoadData(TileSave data, TileType type_of_tile, int number_of_tile)
    {
		type = type_of_tile;
		number = number_of_tile;
		int chances = _dict.Tile_number_to_chances[number];

		TypeLabel.Text = type.ToString();
		TypeLabel.Modulate = new Color(_dict.TileType_to_colour_code[type]);

		NumberLabel.Text = number.ToString();

		string chances_str = ".";

		for(int i = 1; i < chances; i++)
		{
			chances_str+=".";
		}
		ChancesLabel.Text = chances_str;

		if(type == TileType.Desert)
		{
			NumberLabel.Text = "";
			ChancesLabel.Text = "";
		}

		this.Position = data.Position;

    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RpcSyncTileData(Godot.Collections.Dictionary dict, int type_of_tile, int number_of_tile)
	{
		TileSave data = TileSave.FromDictionary(dict);
		LoadData(data, (TileType)type_of_tile, number_of_tile);

	}
}
