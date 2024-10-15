using RSG;
using UniRx;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject Bullet;
    public Transform Cannon;
    public Transform BulletSpawnPoint;
    private float _lastFireTime;
    private PlayerConfig _config;
    
    public IPromise Initialize()
    {
        _config = Resources.Load<PlayerConfig>("PlayerConfig");
        if (_config == null)
            Debug.LogError("PlayerConfig not found");
        
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
        return Time.time >= _lastFireTime + _config.FireRate;
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
        GameObject projectile = Instantiate(_config.ProjectilePrefab, BulletSpawnPoint.position, Quaternion.identity);

        // Set projectile velocity towards the direction
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * _config.ProjectileSpeed;
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

    public IPromise Reset()
    {
        return Promise.Resolved();
    }
    
    
}
