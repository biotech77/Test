using DG.Tweening;
using UniRx;
using UnityEngine;

public class FishController : MonoBehaviour
{
    public float OffScreenPadding = 0.1f;
    private Vector3 targetPosition;
    private LevelManager _level;
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Die();
        }
        //
        // if (collision.gameObject.CompareTag("Stone"))
        // {
        //     // Calculate the reflection direction
        //     Vector2 normal = collision.contacts[0].normal;  // The normal of the surface the bullet hit
        //     Vector2 incomingVelocity = GetComponent<Rigidbody2D>().velocity;  // Bullet's current velocity
        //     Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);  // Reflect the velocity based on the normal
        //
        //     // Apply the reflected velocity
        //     GetComponent<Rigidbody2D>().velocity = reflectedVelocity;
        // }
    }

    public void Initialize()
    {
        _level = GameManager.Instance.LevelManager;
        Observable.EveryUpdate().Subscribe(_ =>
        {
            Swim();
        });
    }

    public void MoveToTarget(Vector3 target)
    {
        targetPosition = target;
    }
    
    private void Swim()
    {
        transform.position =
            Vector3.MoveTowards(transform.position, targetPosition, _level.Config.FishSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = _level.GetRandomTargetPosition();
        }
    }

    private void Die()
    {
        _level.SpawnDieEffect(transform.position);
        _level.Fishes.Remove(this);
        Destroy(gameObject);
    }
}