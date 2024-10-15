using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public float FireRate;
    public float ProjectileSpeed;
    public GameObject ProjectilePrefab;
}
