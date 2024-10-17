using UniRx;
using Unity.Netcode;
using UnityEngine;

public class FishControllerNetwork : NetworkBehaviour
{
    public float OffScreenPadding = 0.1f;
    private Vector3 targetPosition;
    private LevelManagerNetwork _level;
    private PlayerManagerNetwork _player;
    private GameManagerNetwork _game;

    private CompositeDisposable _disposables = new();
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(IsServer && collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Fish hit by bullet!");
            // Check if the bullet has an owner to assign points
            var bullet = collision.gameObject.GetComponent<ProjectileControllerNetwork>();
            if (bullet != null)
            {
                Debug.Log("Fish hit by bullet! " + bullet.OwnerClientId);
                Die(bullet.OwnerClientId);
            }
        }
    }

    public void Initialize()
    {
        _player = PlayerManagerNetwork.Instance;
        _level = LevelManagerNetwork.Instance;
        _game = GameManagerNetwork.Instance;
        
        Observable.EveryUpdate().Subscribe(_ =>
        {
            Swim();
        }).AddTo(_disposables);
    }

    public void MoveToTarget(Vector3 target)
    {
        targetPosition = target;
    }
    
    private void Swim()
    {
        if (transform == null) return;
        
        transform.position =
            Vector3.MoveTowards(transform.position, targetPosition, _level.Config.FishSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = _level.GetRandomTargetPosition();
        }
    }
    
    private void Die(ulong clientId)
    {
        Debug.Log("DieServerRpc called! " + clientId);
        // Register hit for the player that killed the fish (you can award points here)
        RegisterHitForPlayer(clientId);

        // Spawn the die effect
        //_level.SpawnDieEffect(transform.position);

        // Notify clients to spawn the die effect at this position
        SpawnDieEffectClientRpc(transform.position);
        
        // Remove the fish from the list
        _level.Fishes.Remove(this);

        // Clean up resources
        _disposables?.Dispose();

        // Notify clients about the fish death
        NotifyClientsOfFishDeathClientRpc(NetworkObjectId);

        // Despawn the fish on the server
        NetworkObject.Despawn();
    }

    [ClientRpc]
    private void SpawnDieEffectClientRpc(Vector3 position)
    {
        // This will spawn the die effect on all clients locally
        Debug.Log("Spawning die effect on client at position: " + position);
    
        // Spawn the effect locally on the client
        _level.SpawnDieEffect(position);
    }
    
    // This method can handle registering points or hits for the player
    private void RegisterHitForPlayer(ulong clientId)
    {
        var playerManager = _game.GetPlayerManagerByClientId(clientId); // Example: find the player's manager
        if (playerManager != null)
        {
            playerManager.RegisterHit(); // Assuming PlayerManagerNetwork has a method to handle registering hits/points
        }
    }

    [ClientRpc]
    private void NotifyClientsOfFishDeathClientRpc(ulong fishNetworkId)
    {
        // Optionally handle any client-side visuals or logic related to fish death
        Debug.Log($"Fish with NetworkObjectId {fishNetworkId} has died.");
    }


    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}