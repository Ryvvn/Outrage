using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the scriptable object that holds design-time metadata about an enemy type.
/// This allows other systems to get it information without the prefabs
/// </summary>

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Tower Defense/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "New Enemy";
    public Sprite icon;
    public GameObject enemyPrefab;

    [Header("Strategic Info")]
    [TextArea(3, 5)]
    public string scoutingDescription = "Resistant to X, vulnerable to Y.";
    // We can add specific enums or flags here later for more complex logic
    // public DamageType resistance;
    // public DamageType vulnerability;
}
