using System;
using RSG;
using UniRx;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject Bullet;
    public Transform Cannon;
    public Transform BulletSpawnPoint;
    private float _lastFireTime;
    
    private int _shotsFired;
    private int _totalShotsFired;
    public int TotalShotsFired => _totalShotsFired;
    private int _totalSuccessHits;
    public int TotalSuccessHits => _totalSuccessHits;
    private int _shotsPerCycle;
    
    private PlayerConfig _playerConfig;
    private RTPConfig _rtpConfig;
    public Action HitRegistered { get; set; }
    public Action ShotFired { get; set; }

    public IPromise Initialize()
    {
        _playerConfig = Resources.Load<PlayerConfig>("PlayerConfig");
        if (_playerConfig == null)
            Debug.LogError("PlayerConfig not found");
        
        _rtpConfig = Resources.Load<RTPConfig>("RTPConfig");
        if (_rtpConfig == null)
            Debug.LogError("RTPConfig not found");
        
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButton(0) && IsAllowedToFire())
            .Subscribe(_ =>
            {
                RotateCannonTowardsMouse();
                Fire();
                _lastFireTime = Time.time;
            })
            .AddTo(this); 
        
        return Promise.Resolved();
    }

    private bool IsAllowedToFire()
    {
        return Time.time >= _lastFireTime + _playerConfig.FireRate;
    }

    private void Fire()
    {
        // Get the mouse position in world coordinates
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;  // Set the Z distance based on the camera
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Calculate direction from player to mouse position
        Vector3 direction = (worldMousePosition - BulletSpawnPoint.position).normalized;

        // Instantiate the projectile
        GameObject projectile = Instantiate(_playerConfig.ProjectilePrefab, BulletSpawnPoint.position, Quaternion.identity);

        // Set projectile velocity towards the direction
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * _playerConfig.ProjectileSpeed;

        _shotsFired++;
        _totalShotsFired++;
        ShotFired?.Invoke();
    }
    
    void RotateCannonTowardsMouse()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int snappedAngle = (int)((angle + 45.0f) / 90.0f) * 90;
        
        Cannon.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    public void Run()
    {
        
    }

    public void RegisterHit()
    {
        _shotsPerCycle++;
        _totalSuccessHits++;
        HitRegistered?.Invoke();
    }
    
    public IPromise Reset()
    {
        return Promise.Resolved();
    }
}
