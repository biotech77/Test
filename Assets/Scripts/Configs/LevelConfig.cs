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
    }
}