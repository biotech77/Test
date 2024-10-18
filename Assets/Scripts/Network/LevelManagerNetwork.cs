using System;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using NaughtyAttributes;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManagerNetwork : NetworkBehaviour
{
    public List<FishControllerNetwork> Fishes = new();
    private CompositeDisposable _disposables = new();
    private LevelConfig _config;
    public LevelConfig Config => _config;

    public GameObject fishPrefab;  // Fish prefab must have NetworkObject component
    public GameObject effectPrefab; // Effect prefab must have NetworkObject component
    public static LevelManagerNetwork Instance;
    private void Awake()
    {
        Instance = this;
        
        
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        
        _config = Resources.Load<LevelConfig>("LevelConfig");
        if (_config == null) Debug.LogError("LevelConfig not found");
        else
        {
            if (_config.FishKilledEffectMultiplayerPrefab == null)
            {
                Debug.LogError("FishKilledEffectMultiplayerPrefab not found");
                _config.FishKilledEffectMultiplayerPrefab = Resources.Load<GameObject>("FishKilledEffect");
            }
        }
        
        // if (IsServer)
        // {
        //     // Server controls fish spawning
        //     Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
        //     {
        //         SpawnFishServerRpc();
        //     }).AddTo(_disposables);
        // }
        
        // Server controls fish spawning
        Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
        {
            // if (IsClient)
            // {
            //     //SpawnFishServerRpc();
            // }
            // else
            // {
            //     Spawn();
            // }

            if (IsServer)
            {
                Spawn();
            }
            
        }).AddTo(_disposables);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void Spawn()
    {
        if (Fishes.Count < _config.MaxFishInPond)
        {
            Debug.Log("SpawnFishServerRpc 1");
            var spawnOffset = 2f;
            var spawnPosition = GetRandomSpawnPosition();

            // Instantiate fish on the server and spawn it across all clients
            var fishInstance = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
            var controller = fishInstance.GetComponent<FishControllerNetwork>();

            var randomScale = _config.GetFishSize();
            fishInstance.transform.localScale = randomScale;
            var trail = fishInstance.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.startWidth = randomScale.x / 0.2f;
                trail.endWidth = 0;
            }
            // Spawn the fish as a networked object
            fishInstance.GetComponent<NetworkObject>().Spawn();

            Vector3 targetPosition = GetRandomTargetPosition();

            // Initialize and move the fish (handled locally on the server)
            controller.Initialize();
            controller.MoveToTarget(targetPosition);

            // Add the fish to the server's list
            Fishes.Add(controller);

            // Notify clients of this new fish and its target position
            UpdateFishPositionClientRpc(fishInstance.GetComponent<NetworkObject>().NetworkObjectId, spawnPosition, targetPosition);
        }
    }
    
    [ServerRpc]
    void SpawnFishServerRpc()
    {
        Debug.Log("SpawnFishServerRpc");
        Spawn();
    }

    [ClientRpc]
    void UpdateFishPositionClientRpc(ulong fishId, Vector3 spawnPosition, Vector3 targetPosition)
    {
        Debug.Log("UpdateFishPositionClientRpc " + fishId);
        // Client-side: Move fish to correct position when spawned
        NetworkObject netObject = NetworkManager.SpawnManager.SpawnedObjects[fishId];
        var controller = netObject.GetComponent<FishControllerNetwork>();

        controller.transform.position = spawnPosition;
        controller.MoveToTarget(targetPosition); // Assuming FishController has this method
    }

    public Vector3 GetRandomSpawnPosition()
    {
        var spawnOffset = 2f;
        var spawnPosition = Vector3.zero;
        var spawnSide = Random.Range(0, 4);

        switch (spawnSide)
        {
            case 0: // Left of the screen
                spawnPosition = new Vector3(GetScreenLeft() - spawnOffset, Random.Range(GetScreenBottom(), GetScreenTop()), 10f);
                break;
            case 1: // Right of the screen
                spawnPosition = new Vector3(GetScreenRight() + spawnOffset, Random.Range(GetScreenBottom(), GetScreenTop()), 10f);
                break;
            case 2: // Top of the screen
                spawnPosition = new Vector3(Random.Range(GetScreenLeft(), GetScreenRight()), GetScreenTop() + spawnOffset, 10f);
                break;
            case 3: // Bottom of the screen
                spawnPosition = new Vector3(Random.Range(GetScreenLeft(), GetScreenRight()), GetScreenBottom() - spawnOffset, 10f);
                break;
        }

        return spawnPosition;
    }

    public Vector3 GetRandomTargetPosition()
    {
        return new Vector3(Random.Range(GetScreenLeft(), GetScreenRight()), Random.Range(GetScreenBottom(), GetScreenTop()), 10f);
    }

    public float GetScreenLeft()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x : 0f;
    }

    public float GetScreenRight()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x : 0f;
    }

    public float GetScreenTop()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y : 0f;
    }

    public float GetScreenBottom()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y : 0f;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncExistingFishesForNewClientServerRpc(ulong clientId)
    {
        foreach (var fish in Fishes)
        {
            // Send fish data to the newly connected client
            SyncFishForNewClientClientRpc(fish.GetComponent<NetworkObject>().NetworkObjectId, fish.transform.position);
        }
    }

    [ClientRpc]
    void SyncFishForNewClientClientRpc(ulong fishId, Vector3 position)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(fishId, out NetworkObject fishObject))
        {
            fishObject.transform.position = position;
        }
    }
    
    public void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("OnClientConnected " + clientId);
            // When a client connects, send them all existing fish positions
            SyncExistingFishesForNewClientServerRpc(clientId);
        }
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }

    [Button("Spawn Fish")]
    public void SpawnFish()
    {
        SpawnFishServerRpc();
    }


    public void SpawnDieEffect(Vector3 position)
    {
        Debug.Log("Spawning Die Effect at: " + position);
        if (Config.FishKilledEffectPrefab == null)
            Config.FishKilledEffectPrefab = Resources.Load<GameObject>("FishKilledEffect");
        // Ensure the FishKilledEffectPrefab is not a networked object
        if (Config.FishKilledEffectPrefab != null)
        {
            var effect = Instantiate(Config.FishKilledEffectPrefab, position, Quaternion.identity);
            effect.transform.localScale = 0.5f * Vector3.one;

            // Optional: Add any visual effects like scaling or animation
            effect.transform.DOScale(1f, 0.5f).SetEase(Config.FishDieEaseEffect).OnComplete(() =>
            {
                // Destroy the effect after the animation finishes
                Destroy(effect);
            });
        }
        else
        {
            Debug.LogError("FishKilledEffectPrefab is null! Please assign it in the Config.");
        }
    }
}
