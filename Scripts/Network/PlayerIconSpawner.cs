using Godot;
using System;

public partial class PlayerIconSpawner : Godot.MultiplayerSpawner
{
	[Export] public PackedScene player_icon_scene;

    public void SpawnPlayerIcon(long id)
    {
        if (!Multiplayer.IsServer()) return;
        
        if (player_icon_scene != null)
        {
            PlayerLobbyIcon player_icon = player_icon_scene.Instantiate<PlayerLobbyIcon>();
            player_icon.Name = id.ToString();

            Node spawn = GetNode(SpawnPath);
            spawn.AddChild(player_icon, true);
            GD.Print($"Successfully spawned player icon for ID: {id}");
        }
        else
        {
            GD.PrintErr("Missing player_icon scene inside PlayerIconSpawner!");
        }
    }
}
