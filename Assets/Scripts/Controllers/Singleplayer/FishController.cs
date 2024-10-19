using UniRx;
using UnityEngine;

public class FishController : MonoBehaviour
{
    public float OffScreenPadding = 0.1f;
    private Vector3 targetPosition;
    private LevelManager _level;
    private PlayerManager _player;

    private CompositeDisposable _disposables = new();
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Die();
            Destroy(collision.gameObject);
        }
    }

    public void Initialize()
    {
        _player = GameManager.Instance.PlayerManager;
        _level = GameManager.Instance.LevelManager;
        
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

    private void Die()
    {
        _player.RegisterHit();
        _level.SpawnDieEffect(transform.position);
        _level.Fishes.Remove(this);
        _disposables.Dispose();
        Destroy(gameObject);
    }
}