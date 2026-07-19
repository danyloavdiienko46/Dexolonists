using System.Collections.Generic;
using System.Linq;
using Godot;
using HelperScripts;

public partial class MultiplayerWorld : Node3D
{
    private NormalGame _gameInstance;
    private MultiplayerSpawner _spawnerInstance;

    public Queue<long> players_turn = new Queue<long>();
    private List<long> _players;
    private Shuffler _shuffler = new Shuffler();

    private readonly List<Color> _player_colors = new()
    {
        Colors.Red,
        Colors.Blue,
        Colors.Green,
        Colors.Yellow,
        Colors.Purple,
        Colors.Orange
    };

    public void SetPlayers(Godot.Collections.Array<long> playerList)
    {
        _players = new List<long>(playerList);
        long[] new_players = _players.ToArray();
        _shuffler.ShuffleArray(new_players);

        foreach(long id in new_players)
        {
            players_turn.Enqueue(id);
        }
    }

    public void NextTurn()
    {
        long player = players_turn.Dequeue();
        players_turn.Enqueue(player);

        GD.Print("------- The state of player turns is: -------");

        for(int i = 0; i < players_turn.Count; i++)
        {
            GD.Print(i+1 + ". " + _player_colors[GetColourID(players_turn.ElementAt(i))]);
        }
    }

    public int GetColourID(long id)
    {
        return _players.IndexOf(id);
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