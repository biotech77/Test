using UnityEngine;

[CreateAssetMenu(fileName = "RTPConfig", menuName = "Configs/RTPConfig")]
public class RTPConfig : ScriptableObject
{
    public int ShotsPerCycle = 10;
    public float GuaranteedHitRate = 0.3f;
}
