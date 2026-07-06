using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MultiplayerWorld : Node3D
{
    private NormalGame _gameInstance;
    private MultiplayerSpawner _spawnerInstance;

    public Queue<long> players_turn = new Queue<long>();
    private List<long> _players;

    public void SetPlayers(Godot.Collections.Array<long> playerList)
    {
        _players = new List<long>(playerList);
    }

    public override void _Ready()
    {
        _gameInstance = GetNode<NormalGame>("NormalGameMultiplayer");
        _spawnerInstance = GetNode<MultiplayerSpawner>("PlayerSpawner");

        if (Multiplayer.IsServer())
        {
            foreach (long clientId in _players)
            {
                if (_spawnerInstance != null)
                {
                    _spawnerInstance.SpawnPlayer(clientId);
                    GD.Print($"Server authoritative spawn successful for player ID: {clientId}");
                }
            }
        }
    }
}