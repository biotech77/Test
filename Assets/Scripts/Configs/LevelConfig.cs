using DG.Tweening;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public GameObject FishPrefab;
        public GameObject FishKilledEffectPrefab;
        public GameObject FishKilledEffectMultiplayerPrefab;
        public float FishSpeed = 10.0f;
        public int MaxFishInPond = 3;
        public Ease FishDieEaseEffect = Ease.InOutExpo;

        public float MinFishSpeed = 5.0f;
        public float MaxFishSpeed = 15.0f;
        
        public float MinFishSize = 0.15f;
        public float MaxFishSize = 0.3f;

        public int FireRate = 10;
        
        [Header("Shooting Randomness")]
        [Tooltip("Maximum spread angle in degrees for projectile firing.")]
        public float MaxSpreadAngle = 10f; // Adjust as needed
        public float GetFishSpeed()
        {
            var speed = Random.Range(MinFishSpeed, MaxFishSpeed);
            Debug.Log("Speed: " + speed);
            return speed;
        }

        public Vector3 GetFishSize()
        {
            var size = Random.Range(MinFishSize, MaxFishSize);
            return new Vector3(size, size, size);
        }
        
        /// <summary>
        /// Returns a random direction vector within the specified spread angle.
        /// </summary>
        /// <param name="baseDirection">The central direction vector.</param>
        /// <param name="maxSpreadAngle">Maximum spread angle in degrees.</param>
        /// <returns>A new direction vector with random spread applied.</returns>
        public Vector3 GetRandomDirection(Vector3 baseDirection)
        {
            // Generate a random angle between -maxSpreadAngle and +maxSpreadAngle
            float randomAngle = UnityEngine.Random.Range(-MaxSpreadAngle, MaxSpreadAngle);
            Debug.Log($"[Server] Generated random spread angle: {randomAngle} degrees");

            // Create a rotation quaternion around the Z-axis (assuming 2D game)
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);

            // Rotate the base direction by the random angle
            Vector3 newDirection = rotation * baseDirection;
            Debug.Log($"[Server] New direction after applying spread: {newDirection}");

            return newDirection.normalized;
        }
        
        public Vector3 GetRandomDirection(Vector3 baseDirection, float maxSpreadAngle)
        {
            // Generate a random angle between -maxSpreadAngle and +maxSpreadAngle
            float randomAngle = UnityEngine.Random.Range(-maxSpreadAngle, maxSpreadAngle);
            Debug.Log($"[Server] Generated random spread angle: {randomAngle} degrees");

            // Create a rotation quaternion around the Z-axis (assuming 2D game)
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);

            // Rotate the base direction by the random angle
            Vector3 newDirection = rotation * baseDirection;
            Debug.Log($"[Server] New direction after applying spread: {newDirection}");

            return newDirection.normalized;
        }
        
        [ContextMenu("ResetToDefault")]
        public void ResetToDefault()
        {
            FireRate = 10;
            MaxFishInPond = 3;
            MinFishSpeed = 3;
            MaxFishSpeed = 10;
            MaxFishSize = 0.30f;
            MinFishSize = 0.15f;
            MaxSpreadAngle = 10;
        }

        public float GetFireRate()
        {
            if (FireRate != 0)
                return FireRate / 100f;
            return 0.1f;
        }
    }
}