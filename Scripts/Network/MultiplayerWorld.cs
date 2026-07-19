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
    private ColourList _colour_list = new ColourList();

    private MultiplayerGameUi _mp_game_ui;

    public void SetPlayers(Godot.Collections.Array<long> player_list)
    {
        _players = new List<long>(player_list);
        long[] new_players = _players.ToArray();
        _shuffler.ShuffleArray(new_players);

        foreach(long id in new_players)
        {
            players_turn.Enqueue(id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcSetPlayers(long[] shuffled_players)
    {
        players_turn = new Queue<long>();

        foreach(long id in shuffled_players)
        {
            players_turn.Enqueue(id);
        }

        _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(players_turn.ElementAt(0))]);
    }

    public void NextTurn()
    {
        Rpc(nameof(RpcNextTurn));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcNextTurn()
    {
        long player = players_turn.Dequeue();
        players_turn.Enqueue(player);

        _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(players_turn.ElementAt(0))]);

        if (Multiplayer.IsServer())
        {
            GD.Print("------- The state of player turns is: -------");
            for(int i = 0; i < players_turn.Count; i++)
            {
                GD.Print(i+1 + ". " + _colour_list.player_colors[GetColourID(players_turn.ElementAt(i))]);
            }
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
        _mp_game_ui = GetNode<MultiplayerGameUi>("MultiplayerGameUI");

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
        
        if(Multiplayer.IsServer()) Rpc(nameof(RpcSetPlayers), players_turn.ToArray());
    }
}