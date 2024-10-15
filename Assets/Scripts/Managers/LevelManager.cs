using System;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using RSG;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public List<FishController> Fishes = new();
    private CompositeDisposable _disposables = new();
    private LevelConfig _config;
    public LevelConfig Config => _config;
    public IPromise Initialize()
    {
        _config = Resources.Load<LevelConfig>("LevelConfig");
        if (_config == null) Debug.LogError("LevelConfig not found");
        
        Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
        {
            SpawnFish();
            
        }).AddTo(_disposables);
        
        return Promise.Resolved();
    }

    private void SpawnFish()
    {
        if (Fishes.Count < _config.MaxFishInPond)
        {
            var spawnOffset = 2f;
            var spawnPosition = Vector3.zero;
            var spawnSide = Random.Range(0, 4);

            switch (spawnSide)
            {
                case 0: // Left of the screen
                    spawnPosition = new Vector3(GetScreenLeft() - spawnOffset, Random.Range(GetScreenBottom(), GetScreenTop()), 10f);
                    break;
                case 1: // Right of the screen
                    spawnPosition = new Vector3(GetScreenRight() + spawnOffset, Random.Range(GetScreenBottom(), GetScreenTop()), 10f);
                    break;
                case 2: // Top of the screen
                    spawnPosition = new Vector3(Random.Range(GetScreenLeft(), GetScreenRight()), GetScreenTop() + spawnOffset, 10f);
                    break;
                case 3: // Bottom of the screen
                    spawnPosition = new Vector3(Random.Range(GetScreenLeft(), GetScreenRight()), GetScreenBottom() - spawnOffset, 10f);
                    break;
            }

            var fish = Instantiate(_config.FishPrefab, spawnPosition, Quaternion.identity);
            var controller = fish.GetComponent<FishController>();

            // Move the fish towards a random target position inside the screen
            Vector3 targetPosition = GetRandomTargetPosition();

            controller.Initialize();
            controller.MoveToTarget(targetPosition); // Assuming FishController has this method
            Fishes.Add(controller);
        }
    }

    public Vector3 GetRandomTargetPosition()
    {
        return new Vector3(Random.Range(GetScreenLeft(), GetScreenRight()),
            Random.Range(GetScreenBottom(), GetScreenTop()),
            10f);
    }

    public float GetScreenLeft()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x : 0f;
    }
    
    public float GetScreenRight()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x : 0f;
    }
    
    public float GetScreenTop()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y : 0f;
    }
    
    public float GetScreenBottom()
    {
        return Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y : 0f;
    }
    

    public FishController FindNearestFishInDirection(Vector3 projectilePosition, Vector3 direction)
    {
        FishController nearestFish = null;
        var minDistance = Mathf.Infinity;

        foreach (FishController fish in Fishes)
        {
            var toFish = fish.transform.position - projectilePosition;
            var angle = Vector3.Angle(toFish, direction);

            if (angle < 30f)  // Adjust angle threshold for how "in line" the fish should be
            {
                var distance = Vector3.Distance(projectilePosition, fish.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestFish = fish;
                }
            }
        }
        return nearestFish;
    }

    public IPromise Reset()
    {
        foreach (var fish in Fishes) Destroy(fish);
        Fishes.Clear();
        _disposables.Dispose();
        
        return Promise.Resolved();
    }

    public void SpawnDieEffect(Vector3 position)
    {
        var effect = Instantiate(Config.FishKilledEffectPrefab, position, Quaternion.identity);
        effect.transform.localScale = 0.5f * Vector3.one;
        effect.transform.DOScale(1f, 0.5f).SetEase(Config.FishDieEaseEffect).OnComplete(() =>
        {
            Destroy(effect);
        });
    }
}
