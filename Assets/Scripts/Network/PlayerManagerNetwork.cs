using System;
using UniRx;
using Unity.Netcode;
using UnityEngine;

public class PlayerManagerNetwork : NetworkBehaviour
{
    public Transform Cannon;
    
    public static PlayerManagerNetwork Instance;
    private float _lastFireTime;
    public Transform BulletSpawnPoint;
    private PlayerConfig _playerConfig;
    private int _shotsFired;
    private int _totalShotsFired;
    public int TotalShotsFired => _totalShotsFired;
    private int _totalSuccessHits;
    public int TotalSuccessHits => _totalSuccessHits;
    private int _shotsPerCycle;
    public Action HitRegistered { get; set; }
    public Action ShotFired { get; set; }
    
    private void Awake()
    {
        Instance = this;
        
        _playerConfig = Resources.Load<PlayerConfig>("PlayerConfig");
        if (_playerConfig == null)
            Debug.LogError("PlayerConfig not found");
    }

    void Start()
    {
        if (IsOwner)
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButton(0) && IsAllowedToFire())
                .Subscribe(_ =>
                {
                    RotateCannonTowardsMouse();
                    RequestFireServerRpc();
                    _lastFireTime = Time.time;
                })
                .AddTo(this);
        }
    }

    [ServerRpc]
    void RequestFireServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server handles firing the projectile
        FireProjectile(rpcParams.Receive.SenderClientId);
    }
    
    private void FireProjectile(ulong clientId)
    {
        // Calculate the direction from the cannon to the mouse
        Vector3 direction = (BulletSpawnPoint.right).normalized; // Assuming the cannon's right direction is the firing direction

        // Instantiate the projectile on the server
        GameObject projectile = Instantiate(_playerConfig.ProjectileNetworkPrefab, BulletSpawnPoint.position, Quaternion.identity);

        // Set the velocity for the projectile
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * _playerConfig.ProjectileSpeed;

        // Spawn the projectile as a networked object
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        projectileNetworkObject.SpawnWithOwnership(clientId);
    }

    private bool IsAllowedToFire()
    {
        return Time.time >= _lastFireTime + _playerConfig.FireRate;
    }
    
    void RotateCannonTowardsMouse()
    {
        // // Calculate the rotation angle based on mouse input
        // Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Convert mouse position from screen space to world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));

        // Calculate direction from the cannon to the mouse
        Vector3 dir = mousePosition - Cannon.position;

        // Calculate the angle for the rotation
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Send the angle to the server via Server RPC
        SubmitRotationRequestServerRpc(angle);
    }

    [ServerRpc]
    void SubmitRotationRequestServerRpc(float angle)
    {
        // Optionally snap the angle if needed
        int snappedAngle = (int)((angle + 45.0f) / 90.0f) * 90;
        
        // Rotate the cannon on the server
        Cannon.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Notify all clients about the updated rotation
        UpdateCannonRotationClientRpc(angle);
    }

    [ClientRpc]
    void UpdateCannonRotationClientRpc(float angle)
    {
        // Apply the rotation on all clients
        Cannon.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void RegisterHit()
    {
        _shotsPerCycle++;
        _totalSuccessHits++;
        HitRegistered?.Invoke();
    }
}
