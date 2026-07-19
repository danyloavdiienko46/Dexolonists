using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class NormalGame : Node3D
{
	[Export] public NormalGameBase NormalGameBaseNode;
	[Export] public PackedScene MainMenuScene;

	private MultiplayerWorld _mp_world;

    public override void _Ready()
    {
		if(Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer) _mp_world = GetParent<MultiplayerWorld>();
    }


	public void LoadMapBase(string file_path)
	{
		NormalGameBaseNode.LoadMap(file_path);
	}

	public void ExitToMainMenu()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}

	public void ItemChosen(int index, long player_ID = 0)
	{
		GD.Print("Trying to choose item with index " + index);
		NormalGameBaseNode.ItemChosenHandler(index, player_ID);
	}

	public void NextTurn()
	{
		_mp_world.NextTurn();
	}

	public int GetColourID(long id)
	{
		if(Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer)
			return _mp_world.GetColourID(id);
		else
			return 0;
	}

}
