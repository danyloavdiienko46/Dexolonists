using Godot;

public partial class MultiplayerWorld : Node3D
{
    private NormalGame _gameInstance;
    private MultiplayerSpawner _spawnerInstance;

    public override void _Ready()
    {
        _gameInstance = GetNode<NormalGame>("NormalGameMultiplayer");
        _spawnerInstance = GetNode<MultiplayerSpawner>("PlayerSpawner");

        if (Multiplayer.IsServer())
        {
            // Serwer jest gotowy od razu, więc spawnuje hosta
            _spawnerInstance.SpawnPlayer(1);
        }
        else
        {
            // KLIENT: Sprawdzamy aktualny status połączenia
            var status = Multiplayer.MultiplayerPeer.GetConnectionStatus();
            
            if (status == MultiplayerPeer.ConnectionStatus.Connected)
            {
                // Jeśli jakimś cudem połączył się natychmiast – wyślij prośbę
                RpcId(1, nameof(RequestSpawnOnServer));
            }
            else
            {
                // W większości przypadków status to "Connecting". 
                // Podpinamy się pod sygnały sieciowe Godota i czekamy na finał.
                Multiplayer.ConnectedToServer += OnConnectedToServer;
                Multiplayer.ConnectionFailed += OnConnectionFailed;
            }
        }
    }

    private void OnConnectedToServer()
    {
        // Dobra praktyka: natychmiast odpinamy sygnały, żeby nie wisiały w pamięci
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
        Multiplayer.ConnectionFailed -= OnConnectionFailed;

        GD.Print("Połączenie ustanowione! Wysyłam prośbę o spawn do serwera...");
        
        // Teraz gniazdo jest w stanie CONNECTED, więc to wywołanie zadziała idealnie
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
        if (_spawnerInstance != null)
        {
            _spawnerInstance.SpawnPlayer(clientId);
        }
    }
}