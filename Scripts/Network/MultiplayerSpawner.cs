using System;
using System.Collections.Generic;
using Godot;

public partial class MultiplayerSpawner : Godot.MultiplayerSpawner
{
    [Export] public PackedScene network_player;

    public void SpawnPlayer(long id)
    {
        if (!Multiplayer.IsServer()) return;
        
        if (network_player != null)
        {
            NormalGamePlayer player = network_player.Instantiate<NormalGamePlayer>();
            player.Name = id.ToString();

            Node spawn = GetNode(SpawnPath);
            spawn.AddChild(player, true);
            GD.Print($"Successfully spawned player for ID: {id}");
        }
        else
        {
            GD.PrintErr("Missing network_player scene inside MultiplayerSpawner!");
        }
    }
}