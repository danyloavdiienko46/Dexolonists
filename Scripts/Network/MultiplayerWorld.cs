using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HelperScripts;
using dexolonists.HelperScripts;

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
    private bool _has_normal_game_started_dice_roll = false;
    private bool _ended_first_half_start_placement = false;

    public bool has_map_just_loaded = false;

    private List<SetupStep> _setup_steps = new List<SetupStep>();
    private int _current_setup_step_index = 0;

    public override void _PhysicsProcess(double delta)
    {
        if (has_map_just_loaded)
        {
            has_map_just_loaded = false;

            _ = HandleMapLoadedAsync();
        }
    }

    private async System.Threading.Tasks.Task HandleMapLoadedAsync()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        if (!has_normal_game_started && _setup_steps.Count > 0)
        {
            TriggerCurrentSetupStep();
        }
    }


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

        BuildSetupSequence();

        if (_setup_steps.Count > 0)
        {
            _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(_setup_steps[0].PlayerId)]);
        }
    }

    public long CurrentTurnID()
    {
        if (!has_normal_game_started)
        {
            if (_setup_steps.Count > 0 && _current_setup_step_index < _setup_steps.Count)
            {
                return _setup_steps[_current_setup_step_index].PlayerId;
            }
            return -1;
        }

        return players_turn.ElementAt(0);
    }

    public void NextTurn()
    {
        if (!has_normal_game_started) return;
        RpcId(1, nameof(RpcNextTurnHandler));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcNextTurnHandler()
    {
        int roll_number = 0;

        int first_roll_number = _rand.Next(1, 7);
        int second_roll_number = _rand.Next(1, 7);
        roll_number = first_roll_number + second_roll_number;
        RpcId(1, nameof(RpcGainResources), roll_number);

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

    public void NextSetupStep()
    {
        if (!Multiplayer.IsServer()) return;

        int next_ind = _current_setup_step_index + 1;
        Rpc(nameof(RpcSyncSetupStep), next_ind);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcSyncSetupStep(int stepIndex)
    {
        _current_setup_step_index = stepIndex;

        if (_current_setup_step_index >= _setup_steps.Count)
        {
            has_normal_game_started = true;
            
            foreach (NormalGamePlayer normal_player in normal_game_players)
            {
                normal_player.player_info_holder.forced_building_type = null;
                normal_player.player_info_holder.is_forced_building_enabled = false;

                normal_player.EnableItemChoosement();
            }

            _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(CurrentTurnID())]);
            GD.Print("--- SETUP PHASE COMPLETE. NORMAL GAME STARTED ---");
            return;
        }

        SetupStep curr_step = _setup_steps[_current_setup_step_index];
        _mp_game_ui.ChangeTurnRectColour(_colour_list.player_colors[GetColourID(curr_step.PlayerId)]);

        TriggerCurrentSetupStep();
    }

    private void TriggerCurrentSetupStep()
    {
        if (_current_setup_step_index >= _setup_steps.Count) return;

        SetupStep step = _setup_steps[_current_setup_step_index];
        
        if (step.PlayerId == Multiplayer.GetUniqueId())
        {
            int player_index = players_IDs.IndexOf(step.PlayerId);
            if (player_index != -1 && normal_game_players.Count > player_index)
            {
                var player = normal_game_players.ElementAt(player_index);
                player.player_info_holder.forced_building_type = step.ItemType;
                player.ForceItemSelection(step.ItemType);
            }
        }
    }

    private void BuildSetupSequence()
    {
        _setup_steps.Clear();
        _current_setup_step_index = 0;

        // Round 1: Forward (P1 -> P2 -> P3 -> ...)
        foreach (long id in players_global_turn)
        {
            _setup_steps.Add(new SetupStep { PlayerId = id, ItemType = ItemType.House });
            _setup_steps.Add(new SetupStep { PlayerId = id, ItemType = ItemType.Road });
        }

        // Round 2: Reverse (... -> P3 -> P2 -> P1)
        long[] reversed_players = players_global_turn.ToArray();
        Array.Reverse(reversed_players);
        foreach (long id in reversed_players)
        {
            _setup_steps.Add(new SetupStep { PlayerId = id, ItemType = ItemType.House });
            _setup_steps.Add(new SetupStep { PlayerId = id, ItemType = ItemType.Road });
        }
    }
}