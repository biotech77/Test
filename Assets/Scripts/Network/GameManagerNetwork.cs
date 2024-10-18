using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManagerNetwork : NetworkBehaviour
{
    public GameObject PlayerPrefab;
    public Transform[] SpawnPoints;
    private Dictionary<ulong, PlayerManagerNetwork> _players = new Dictionary<ulong, PlayerManagerNetwork>();

    public static GameManagerNetwork Instance;
    void Awake()
    {
        Instance = this;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_players.ContainsKey(clientId))
        {
            _players.Remove(clientId);
            Debug.Log($"Client {clientId} disconnected and removed from player list.");
        }
    }

    private NetworkVariable<ulong> Player1ClientId = new(ulong.MaxValue);
    private NetworkVariable<ulong> Player2ClientId = new(ulong.MaxValue);
    private void OnClientConnected(ulong clientId)
    {
        var clientIndex = NetworkManager.Singleton.ConnectedClientsList.Count - 1;
        
        if (clientIndex < SpawnPoints.Length)
        {
            var position = new Vector3(SpawnPoints[clientIndex].position.x, 0, SpawnPoints[clientIndex].position.z);
            var playerInstance = Instantiate(PlayerPrefab, position, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            var playerController = playerInstance.GetComponent<PlayerManagerNetwork>();
            if (!_players.ContainsKey(clientId))
            {
                if (Player1ClientId.Value == ulong.MaxValue)
                {
                    Player1ClientId.Value = clientId;
                    Debug.Log($"Player 1 registered with ClientId: {clientId}");
                }
                else if (Player2ClientId.Value == ulong.MaxValue)
                {
                    Player2ClientId.Value = clientId;
                    Debug.Log($"Player 2 registered with ClientId: {clientId}");
                }
                
                _players.Add(clientId, playerController);
                Debug.Log($"Player {clientId} registered.");
            }
        }
        else
        {
            Debug.LogWarning("Not enough spawn positions defined.");
        }
    }

    public PlayerManagerNetwork GetPlayerManagerByClientId(ulong clientId)
    {
        return _players.GetValueOrDefault(clientId); // Return null if no matching player is found
    }
    
    public int GetPlayerSlotByClientId(ulong clientId)
    {
        if (clientId == Player1ClientId.Value)
            return 1;
        else if (clientId == Player2ClientId.Value)
            return 2;

        return -1;  // Not found
    }
    
    public int GetPlayerManagerSlotByClient(ulong clientId)
    {
        var player1 = _players.Values.ElementAtOrDefault(0);
        if (player1 != null)
            Debug.Log("player 1 " + player1.OwnerClientId);
        if (player1 != null && player1.OwnerClientId == clientId)
            return 1;
        else
        {
            var player2 = _players.Values.ElementAtOrDefault(1);
            
            if (player2 != null)
                Debug.Log("player 2 " + player2.OwnerClientId);
            
            if (player2 != null && player2.OwnerClientId == clientId)
                return 2;
        }

        return -1;
    }
    
    public PlayerManagerNetwork GetPlayerBySlot(int slot)
    {
        if (slot == 1)
        {
            return _players.Values.ElementAtOrDefault(0);  // First registered player is Player 1
        }
        else if (slot == 2)
        {
            return _players.Values.ElementAtOrDefault(1);  // Second registered player is Player 2
        }
        return null;
    }
    
    public override void OnDestroy()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
        
        base.OnDestroy();
    }
}
