using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HelperScripts;

public partial class MultiplayerWorld : Node3D
{
    private NormalGame _gameInstance;
    private MultiplayerSpawner _spawnerInstance;

    public Queue<long> players_turn = new Queue<long>();

    public Queue<long> players_global_turn = new Queue<long>();
    public List<long> players_IDs;
    public Godot.Collections.Array<NormalGamePlayer> normal_game_players = [];
    private Shuffler _shuffler = new Shuffler();
    private ColourList _colour_list = new ColourList();

    private Random _rand = new Random();

    private MultiplayerGameUi _mp_game_ui;

    public bool has_normal_game_started = false;
    private bool _ended_first_half_start_placement = false;

    public void SetPlayers(Godot.Collections.Array<long> player_list)
    {
        players_IDs = new List<long>(player_list);
        long[] new_players = players_IDs.ToArray();
        _shuffler.ShuffleArray(new_players);

        foreach(long id in new_players)
        {
            players_turn.Enqueue(id);
            players_global_turn.Enqueue(id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcSetPlayers(long[] shuffled_players)
    {
        players_turn = new Queue<long>();
        players_global_turn = new Queue<long>();

        foreach(long id in shuffled_players)
        {
            players_turn.Enqueue(id);
            players_global_turn.Enqueue(id);
        }
        PopulatePlayersListFromExistingIDs();

        _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(players_turn.ElementAt(0))]);
    }

    public long CurrentTurnID()
    {
        return players_turn.ElementAt(0);
    }

    public void NextTurn()
    {
        RpcId(1, nameof(RpcNextTurnHandler));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcNextTurnHandler()
    {
        int roll_number = 0;

        if (has_normal_game_started)
        {
            int first_roll_number = _rand.Next(1, 7);
            int second_roll_number = _rand.Next(1, 7);
            roll_number = first_roll_number + second_roll_number;
            RpcId(1, nameof(RpcGainResources), roll_number);
        }

        Rpc(nameof(RpcNextTurn), roll_number);
        
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcGainResources(int roll_number)
    {
        foreach(NormalGamePlayer player in normal_game_players)
        {
            player.player_info_holder.GetTurnResources(roll_number);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcNextTurn(int roll_number)
    {
        if (has_normal_game_started)
        {
            long player = players_turn.Dequeue();
            players_turn.Enqueue(player);

            _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(players_turn.ElementAt(0))]);
            _mp_game_ui.UpdateDiceRollLabel(roll_number);

            if (Multiplayer.IsServer())
            {
                GD.Print("------- The state of player turns is: -------");
                for(int i = 0; i < players_turn.Count; i++)
                {
                    GD.Print(i+1 + ". " + _colour_list.player_colors[GetColourID(players_turn.ElementAt(i))]);
                }
            }
        }

        else
        {
            long player = players_turn.Dequeue();
            if(players_turn.Count != 0)
            {
                _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(players_turn.ElementAt(0))]);
            }
            else
            {
                if (!_ended_first_half_start_placement)
                {
                    for(int i = players_global_turn.Count-1; i >= 0; i--)
                    {
                        players_turn.Enqueue(players_global_turn.ElementAt(i));
                    }
                    _ended_first_half_start_placement = true;
                }
                else
                {
                    for(int i = 0; i < players_global_turn.Count; i++)
                    {
                        players_turn.Enqueue(players_global_turn.ElementAt(i));
                    }
                    has_normal_game_started = true;
                }
                
            }
            
        }
        
    }

    public int GetColourID(long id)
    {
        return players_IDs.IndexOf(id);
    }


    public override void _Ready()
    {
        _gameInstance = GetNode<NormalGame>("NormalGameMultiplayer");
        _spawnerInstance = GetNode<MultiplayerSpawner>("PlayerSpawner");
        _mp_game_ui = GetNode<MultiplayerGameUi>("MultiplayerGameUI");

        if (Multiplayer.IsServer())
        {

            foreach (long clientId in players_IDs)
            {
                if (_spawnerInstance != null)
                {
                    normal_game_players.Add(_spawnerInstance.SpawnPlayer(clientId));
                    GD.Print($"Server authoritative spawn successful for player ID: {clientId}");
                }
            }

            Rpc(nameof(RpcSetPlayers), players_turn.ToArray());
        }
    }

    public void PopulatePlayersListFromExistingIDs()
    {
        normal_game_players.Clear();

        foreach (long id in players_IDs)
        {
            var playerNode = GetNodeOrNull<NormalGamePlayer>($"NormalGameMultiplayer/{id}");

            if (playerNode != null)
            {
                normal_game_players.Add(playerNode);
            }
            else
            {
                GD.PrintErr($"Client could not find spawned player node for ID: {id}");
            }
        }
    }
}