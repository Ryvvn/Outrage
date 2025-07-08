using UnityEngine;

/// <summary>
/// A small component to attach to an enemy prefab, linking it back to its ScriptableObject data.
/// </summary>
public class EnemyMetadata : MonoBehaviour
{
    public EnemyData enemyData;
}