using System;
using UniRx;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float OffScreenPadding = 0.1f;
    private LevelManager _level;
    
    private bool _isHoming = false;
    private FishController _targetFish;
    public float HomingTurnSpeed = 500f; // Increased from 200f
    public float HomingAcceleration = 50f; // Increased from 10f
    public float HomingMaxSpeed = 20f; // New parameter for maximum speed during homing
    private Rigidbody2D _rb;
    void Start()
    {
        _level = GameManager.Instance.LevelManager;
        _rb = GetComponent<Rigidbody2D>();
        
        Observable.EveryUpdate().Subscribe(_ =>
        {
            // Get the bullet's position in relation to the camera's viewport (0 to 1 range for on-screen)
            Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

            // Check if the bullet is off-screen, plus some extra padding
            if (viewPos.x < -OffScreenPadding || viewPos.x > 1 + OffScreenPadding || 
                viewPos.y < -OffScreenPadding || viewPos.y > 1 + OffScreenPadding)
            {
                Destroy(gameObject);  // Destroy the bullet once it's far enough off-screen
            }
            
            // Handle homing behavior
            if (_isHoming && _targetFish != null)
            {
                HomingTowardsTarget();
            }
            
        }).AddTo(this);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Stone"))
        {
            if (_isHoming) return;
            
            if (!GameManager.Instance.PlayerManager.IsHitNeedsToBeGuaranteed)
            {
                Destroy(gameObject);
            }
            else
            {
                GameManager.Instance.PlayerManager.AllowedToGuaranteeHit = false;
                GameManager.Instance.PlayerManager.IsHitNeedsToBeGuaranteed = false;
                ActivateHomingMissile();
            }
        }
    }
    
    private void ActivateHomingMissile()
    {
        if (_level.Fishes.Count > 0)
        {
            _isHoming = true;
            _targetFish = _level.FindClosestFish();

            if (_targetFish != null)
            {
                // Optionally, you can disable gravity if enabled
                _rb.gravityScale = 0f;
            }
            else
            {
                // If no fish found, revert to regular behavior
                _isHoming = false;
            }
        }
    }

    private void HomingTowardsTarget()
    {
        if (_targetFish == null)
        {
            // If the target fish has been destroyed or is null, stop homing
            _isHoming = false;
            return;
        }

        // Calculate direction towards the target fish
        Vector2 direction = ((Vector2)_targetFish.transform.position - _rb.position).normalized;

        // Calculate the desired velocity
        Vector2 desiredVelocity = direction * HomingMaxSpeed;

        // Calculate acceleration needed to reach desired velocity
        Vector2 steering = desiredVelocity - _rb.velocity;
        steering = Vector2.ClampMagnitude(steering, HomingAcceleration * Time.deltaTime);

        // Apply steering
        _rb.velocity += steering;

        // Clamp the velocity to HomingMaxSpeed
        if (_rb.velocity.magnitude > HomingMaxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * HomingMaxSpeed;
        }
    }

    /// <summary>
    /// Rotates a Vector2 towards a target Vector2 by a maximum angle in radians and a maximum magnitude change.
    /// </summary>
    /// <param name="current">Current Vector2.</param>
    /// <param name="target">Target Vector2.</param>
    /// <param name="maxRadiansDelta">Maximum radians to rotate.</param>
    /// <param name="maxMagnitudeDelta">Maximum change in magnitude.</param>
    /// <returns>New rotated Vector2.</returns>
    private Vector2 RotateTowards(Vector2 current, Vector2 target, float maxRadiansDelta, float maxMagnitudeDelta)
    {
        float currentAngle = Mathf.Atan2(current.y, current.x);
        float targetAngle = Mathf.Atan2(target.y, target.x);

        float angleDifference =
            Mathf.DeltaAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        angleDifference = Mathf.Clamp(angleDifference, -maxRadiansDelta, maxRadiansDelta);

        float newAngle = currentAngle + angleDifference;
        float magnitude = Mathf.MoveTowards(current.magnitude, target.magnitude, maxMagnitudeDelta);

        return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * magnitude;
    }

    private void OnDestroy()
    {
        GameManager.Instance.LevelManager.SpawnDieEffect(transform.position, 0.2f, 0.4f);

        if (_isHoming)
            GameManager.Instance.PlayerManager.AllowedToGuaranteeHit = true;
    }
}
