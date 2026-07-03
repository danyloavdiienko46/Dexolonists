using Godot;

public partial class MultiplayerSpawner : Godot.MultiplayerSpawner
{
    [Export] public PackedScene network_player;

    public void SpawnPlayer(long id)
    {
        if (!Multiplayer.IsServer()) return;
        
        if (network_player != null)
        {
            Node3D player = network_player.Instantiate<Node3D>();
            player.Name = id.ToString();
            player.Position = new Vector3(0, 10.551f, 7.765f);

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