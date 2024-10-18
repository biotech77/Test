using UniRx;
using Unity.Netcode;
using UnityEngine;

public class ProjectileControllerNetwork : NetworkBehaviour
{
    public float OffScreenPadding = 0.1f;
    void Start()
    {
        if (IsServer)
        {
            // Only the server should handle despawning
            Observable.EveryUpdate().Subscribe(_ =>
            {
                // Get the bullet's position in relation to the camera's viewport (0 to 1 range for on-screen)
                Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);

                // Check if the bullet is off-screen, plus some extra padding
                if (viewPos.x < -OffScreenPadding || viewPos.x > 1 + OffScreenPadding ||
                    viewPos.y < -OffScreenPadding || viewPos.y > 1 + OffScreenPadding)
                {
                    // Despawn the projectile across the network
                    NotifyClientOfMissClientRpc(OwnerClientId);
                    DespawnProjectile();
                    Debug.Log("Projectile Despawned! " + OwnerClientId);
                }
            }).AddTo(this);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsServer)
        {
            // Check if the projectile hits something
            if (collision.gameObject.CompareTag("Stone"))
            {
                // Despawn the projectile on the server and sync it across all clients
                DespawnProjectile();
            }
            else if (collision.gameObject.CompareTag("Fish"))
            {
                // For example: handling hit on fish and despawning after hit
                DespawnProjectile();
            }
            Debug.Log($"Projectile hit by client {OwnerClientId}");
        }
    }
    
    // Despawn the projectile from the server and notify clients
    private void DespawnProjectile()
    {
        // Check if it's still a valid NetworkObject
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
    
    // Notify clients about the miss, so they can update the UI
    [ClientRpc]
    private void NotifyClientOfMissClientRpc(ulong clientId)
    {
        PlayerManagerNetwork.Instance.RegisterMiss(clientId);
    }
}