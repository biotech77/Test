using System;
using System.Collections.Generic;
using Configs;
using UniRx;
using Unity.Netcode;
using UnityEngine;

public class PlayerManagerNetwork : NetworkBehaviour
{
    public Transform Cannon;
    private float _lastFireTime;
    public Transform BulletSpawnPoint;
    private PlayerConfig _playerConfig;
    private LevelConfig _levelConfig;
    private int _shotsFired;
    private int _totalShotsFired;
    public int TotalShotsFired => _totalShotsFired;
    private int _totalSuccessHits;
    public int TotalSuccessHits => _totalSuccessHits;
    private int _shotsPerCycle;
    public Action HitRegistered { get; set; }
    public Action ShotFired { get; set; }

    private Queue<bool> _last10Shots = new Queue<bool>(10);

    private void Awake()
    {
        _playerConfig = Resources.Load<PlayerConfig>("PlayerConfig");
        if (_playerConfig == null)
            Debug.LogError("PlayerConfig not found");
        
        _levelConfig = Resources.Load<LevelConfig>("LevelConfig");
        if (_levelConfig == null)
            Debug.LogError("LevelConfig not found");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            // Register this player with GameManagerNetwork on the client
            if (GameManagerNetwork.Instance != null)
            {
                GameManagerNetwork.Instance.RegisterPlayer(OwnerClientId, this);
                Debug.Log($"[Client] Player {OwnerClientId} registered with GameManagerNetwork.");
            }
            else
            {
                Debug.LogError("[Client] GameManagerNetwork.Instance is null during player registration.");
            }
        }

        if (IsOwner)
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButton(0) && IsAllowedToFire())
                .Subscribe(_ =>
                {
                    RotateCannonTowardsMouse();
                    _totalShotsFired++;
                    RequestFireServerRpc();
                    _lastFireTime = Time.time;
                })
                .AddTo(this);
        }
    }

    [ServerRpc(RequireOwnership = true)]
    void RequestFireServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong shooterClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[Server] Received FireProjectile request from client {shooterClientId}.");
        FireProjectile(shooterClientId);
    }

    private void FireProjectile(ulong clientId)
    {
        if (clientId == 0)
        {
            Debug.LogError("[Server] Invalid clientId: 0. Cannot spawn projectile.");
            return;
        }

        // Calculate the direction from the cannon to the mouse
        Vector3 baseDirection = (BulletSpawnPoint.right).normalized; // Assuming the cannon's right direction is the firing direction
        Vector3 randomDirection = _levelConfig.GetRandomDirection(baseDirection);
        // Instantiate the projectile on the server
        GameObject projectile = Instantiate(_playerConfig.ProjectileNetworkPrefab, BulletSpawnPoint.position, Quaternion.identity);

        // Set the velocity for the projectile
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = randomDirection * _playerConfig.ProjectileSpeed;
            Debug.Log($"[Server] Projectile velocity set to {rb.velocity}.");
        }
        else
        {
            Debug.LogError("[Server] Rigidbody2D component missing on Projectile prefab.");
        }

        // Spawn the projectile as a networked object with ownership to the shooter client
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        if (projectileNetworkObject != null)
        {
            projectileNetworkObject.SpawnWithOwnership(clientId);
            Debug.Log($"[Server] Projectile spawned with ownership to client {clientId}.");
        }
        else
        {
            Debug.LogError("[Server] NetworkObject component missing on Projectile prefab.");
        }
    }

    private bool IsAllowedToFire()
    {
        return Time.time >= _lastFireTime + _levelConfig.GetFireRate();
    }

    void RotateCannonTowardsMouse()
    {
        // Convert mouse position from screen space to world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));

        // Calculate direction from the cannon to the mouse
        Vector3 dir = mousePosition - Cannon.position;

        // Calculate the angle for the rotation
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Send the angle to the server via Server RPC
        SubmitRotationRequestServerRpc(angle);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitRotationRequestServerRpc(float angle, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] Received rotation request. Angle: {angle} from client {rpcParams.Receive.SenderClientId}");

        // Rotate the cannon on the server
        Cannon.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Debug.Log($"[Server] Cannon rotated to {angle} degrees.");

        // Notify all clients about the updated rotation
        UpdateCannonRotationClientRpc(angle);
    }

    [ClientRpc]
    void UpdateCannonRotationClientRpc(float angle)
    {
        // Apply the rotation on all clients
        Cannon.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Debug.Log($"[Client] Cannon rotated to {angle} degrees.");
    }

    public void RegisterHit(ulong clientId)
    {
        UpdatePlayerHitInfoClientRpc(clientId);
    }

    [ClientRpc]
    void UpdatePlayerHitInfoClientRpc(ulong clientId)
    {
        Debug.Log($"[Client] UpdatePlayerHitInfoClientRpc called for clientId: {clientId}");
        UpdateShotRecord(true);
        var playerSlot = GameManagerNetwork.Instance.GetPlayerSlotByClientId(clientId);
        Debug.Log($"[Client] Player slot retrieved: {playerSlot}");
        _shotsPerCycle++;
        _totalSuccessHits++;
        HitRegistered?.Invoke();

        if (playerSlot == -1) return;
        // Apply the rotation on all clients
        if (clientId == NetworkManager.Singleton.LocalClientId)
            GuiManagerNetwork.Instance.PlayerInfo.UpdatePlayerInfo(clientId, _totalSuccessHits, _totalShotsFired,
                _last10Shots);
    }

    public void RegisterMiss(ulong clientId)
    {
        UpdateShotRecord(false);  // Record a miss

        ShotFired?.Invoke();

        var playerSlot = GameManagerNetwork.Instance.GetPlayerSlotByClientId(clientId);
        Debug.Log($"[Client] RegisterMiss called for clientId: {clientId}, PlayerSlot: {playerSlot}");
        if (playerSlot == -1) return;

        if (NetworkManager.Singleton.LocalClientId == clientId)
            GuiManagerNetwork.Instance.PlayerInfo.UpdatePlayerInfo(clientId, _totalSuccessHits, _totalShotsFired,
                _last10Shots);
    }

    private void UpdateShotRecord(bool isHit)
    {
        if (_last10Shots.Count >= 10)
        {
            _last10Shots.Dequeue(); // Remove the oldest shot result
        }

        _last10Shots.Enqueue(isHit);
    }
}
