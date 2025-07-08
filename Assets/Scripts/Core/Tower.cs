// Assets/Scripts/Core/Tower.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Tower : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseRange = 5f;
    public float baseAttackSpeed = 1f; // Attacks per second

    [Header("Live Stats")]
    private float currentDamage;
    private float currentRange;
    private float currentAttackSpeed;

    [Header("Targeting")]
    private Transform currentTarget;
    private float attackCooldown = 0f;

    // A list of modifications this specific tower has received.
    private List<UpgradeData> appliedModifications = new List<UpgradeData>();
    private TowerManager towerManager;

    void Awake()
    {
        // Add a sphere collider to act as the range trigger
        var rangeCollider = gameObject.AddComponent<SphereCollider>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = baseRange;
    }

    void Start()
    {
        // Initialize current stats with base values.
        currentDamage = baseDamage;
        currentRange = baseRange;
        currentAttackSpeed = baseAttackSpeed;

        towerManager = FindObjectOfType<TowerManager>();
        if (towerManager != null)
        {
            towerManager.RegisterTower(this);
        }
        else
        {
            Debug.LogError("TowerManager not found in scene!");
        }
    }

    void OnDestroy()
    {
        if (towerManager != null)
        {
            towerManager.UnregisterTower(this);
        }
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;

        // If we have a target, check if it's still valid
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > currentRange)
        {
            currentTarget = null; // Target is out of range
        }

        if (currentTarget == null)
        {
            FindNewTarget();
        }

        if (currentTarget != null && attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = 1f / currentAttackSpeed;
        }
    }

    private void FindNewTarget()
    {
        // Find all colliders within range on the "Enemy" layer
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentRange, LayerMask.GetMask("Enemy"));
        if (enemiesInRange.Length > 0)
        {
            currentTarget = enemiesInRange[0].transform; // Target the first one found
        }
    }

    void Attack()
    {
        if (currentTarget == null) return;

        EnemyHealth targetHealth = currentTarget.GetComponent<EnemyHealth>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(currentDamage);
            Debug.DrawLine(transform.position, currentTarget.position, Color.yellow, 0.1f);
        }
        else
        {
            // The target might have been destroyed by another tower
            currentTarget = null;
        }
    }

    public void ApplyModification(UpgradeData upgrade)
    {
        if (upgrade == null || appliedModifications.Contains(upgrade)) return;

        appliedModifications.Add(upgrade);
        RecalculateStats();

        Debug.Log($"Applied {upgrade.upgradeName} to {gameObject.name}. New Stats: Dmg={currentDamage}, Rng={currentRange}, Spd={currentAttackSpeed}");
    }

    private void RecalculateStats()
    {
        // Reset to base before reapplying all mods
        currentDamage = baseDamage;
        currentRange = baseRange;
        currentAttackSpeed = baseAttackSpeed;

        foreach (var mod in appliedModifications)
        {
            foreach (var statMod in mod.statModifications)
            {
                ApplyStat(statMod);
            }
        }

        // Update the trigger collider radius
        GetComponent<SphereCollider>().radius = currentRange;
    }

    private void ApplyStat(StatModification statMod)
    {
        switch (statMod.statName.ToLower())
        {
            case "damage":
                currentDamage += statMod.isPercentage ? baseDamage * statMod.value : statMod.value;
                break;
            case "range":
                currentRange += statMod.isPercentage ? baseRange * statMod.value : statMod.value;
                break;
            case "attackspeed":
                currentAttackSpeed += statMod.isPercentage ? baseAttackSpeed * statMod.value : statMod.value;
                break;
            default:
                Debug.LogWarning($"Unknown stat modification: {statMod.statName}");
                break;
        }
    }

    // Visualize range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentRange > 0 ? currentRange : baseRange);
    }
}