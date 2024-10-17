using UniRx;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float OffScreenPadding = 0.1f;
    private LevelManager _level;
    void Start()
    {
        _level = GameManager.Instance.LevelManager;
        
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
        }).AddTo(this);
        
        
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // // Check if the bullet hits the stone
        // if (collision.gameObject.CompareTag("Stone"))
        // {
        //     Destroy(gameObject);  // Destroy the bullet
        // }
        
        if (collision.gameObject.CompareTag("Stone"))
        {
            // // Calculate the reflection direction
            // Vector2 normal = collision.contacts[0].normal;  // The normal of the surface the bullet hit
            // Vector2 incomingVelocity = GetComponent<Rigidbody2D>().velocity;  // Bullet's current velocity
            // Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);  // Reflect the velocity based on the normal
            //
            // // Apply the reflected velocity
            // GetComponent<Rigidbody2D>().velocity = reflectedVelocity;
            
            if (_level.Fishes.Count > 0)  // Assuming you have a reference to the LevelManager
            {
                // Find a random fish
                var randomFish = _level.FindClosestFish();
            
                // Calculate direction from the bullet to the fish
                Vector2 directionToFish = (randomFish.transform.position - transform.position).normalized;
            
                // Set a faster speed for the ricochet
                var ricochetSpeed = 10f;  // Adjust the speed as necessary
            
                // Apply the new velocity in the direction of the fish
                GetComponent<Rigidbody2D>().velocity = directionToFish * ricochetSpeed;
            }
        }
    }
}
