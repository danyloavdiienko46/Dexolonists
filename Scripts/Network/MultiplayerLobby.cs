using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MultiplayerLobby : Node3D
{
    [Export] public PackedScene MultiplayerWorldScene;
	[Export] public PackedScene MainMenuScene;
	[Export] public Button StartGameButton;
    private PlayerIconSpawner _player_icon_spawner;

    private List<long> _players = new List<long>();

    public override void _Ready()
    {
        _player_icon_spawner = GetNode<PlayerIconSpawner>("PlayerIconSpawner");

        if (Multiplayer.IsServer())
        {
            _players.Add(1);
            _player_icon_spawner.SpawnPlayerIcon(1);
			StartGameButton.Visible = true;
        }
        else
        {
			StartGameButton.Visible = false;
            var status = Multiplayer.MultiplayerPeer.GetConnectionStatus();
            
            if (status == MultiplayerPeer.ConnectionStatus.Connected)
            {
                RpcId(1, nameof(RequestSpawnOnServer));
            }
            else
            {
                Multiplayer.ConnectedToServer += OnConnectedToServer;
                Multiplayer.ConnectionFailed += OnConnectionFailed;
            }
        }
    }

    private void OnConnectedToServer()
    {
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
        Multiplayer.ConnectionFailed -= OnConnectionFailed;

        GD.Print("Połączenie ustanowione! Wysyłam prośbę o spawn do serwera...");
        
        RpcId(1, nameof(RequestSpawnOnServer));
    }

    private void OnConnectionFailed()
    {
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
        Multiplayer.ConnectionFailed -= OnConnectionFailed;
        
        GD.PrintErr("Nie udało się połączyć z serwerem!");
        // Tutaj możesz np. wyrzucić gracza z powrotem do menu głównego
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestSpawnOnServer()
    {
        if (!Multiplayer.IsServer()) return;

        long clientId = Multiplayer.GetRemoteSenderId();

        if (!_players.Contains(clientId))
        {
            _players.Add(clientId);

            if (_player_icon_spawner != null)
            {
                _player_icon_spawner.SpawnPlayerIcon(clientId);
            }
        }
    }

	public async void StartGameBtnPressed()
	{
		if (!Multiplayer.IsServer()) return;

        if (_players.Count < 2) 
        {
            GD.Print("Cannot start game alone!");
            return;
        }

        var spawnTarget = _player_icon_spawner.GetNode(_player_icon_spawner.SpawnPath);
        foreach (Node child in spawnTarget.GetChildren())
        {
            child.QueueFree();
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        var players = new Godot.Collections.Array<long>(_players);
        Rpc(nameof(RpcStartGame), players);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private async void RpcStartGame(Godot.Collections.Array<long> final_player_list)
    {
        GD.Print($"Switching to game world! Total players: {final_player_list.Count}");

        MultiplayerWorld worldInstance = MultiplayerWorldScene.Instantiate<MultiplayerWorld>();
        worldInstance.SetPlayers(final_player_list);
        GetTree().Root.AddChild(worldInstance);

        if (Multiplayer.IsServer())
        {
            QueueFree();
        }
        else
        {
            this.Visible = false;
            this.ProcessMode = ProcessModeEnum.Disabled;
            await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

            QueueFree();
        }
    }

	public void MainMenuBtnPressed()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}

}
