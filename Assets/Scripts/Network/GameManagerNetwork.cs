using System.Collections.Generic;
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
