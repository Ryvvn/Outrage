// Scripts/Core/Tower.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Tower : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseRange = 5f;
    public float baseAttackSpeed = 1f; // Attacks per second

    private float currentDamage;
    private float currentRange;
    private float currentAttackSpeed;

    [Header("Targeting")]
    private Transform currentTarget;
    private float attackCooldown;
    private List<HealthSystem> enemiesInRange = new List<HealthSystem>();

    private List<UpgradeData> appliedModifications = new List<UpgradeData>();
    private TowerManager towerManager;
    private CircleCollider2D rangeCollider;

    void Awake()
    {
        // Add required components programmatically.
        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; // Prevents the tower from being moved by physics.
        }

        rangeCollider = gameObject.AddComponent<CircleCollider2D>();
        rangeCollider.isTrigger = true;
    }

    void Start()
    {
        // Initialize stats and register with the manager.
        RecalculateStats();

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

        // Validate the current target.
        if (currentTarget != null && !IsTargetValid(currentTarget.GetComponent<HealthSystem>()))
        {
            currentTarget = null;
        }

        // If we have no target, find a new one from the enemies in range.
        if (currentTarget == null && enemiesInRange.Count > 0)
        {
            FindNewTarget();
        }

        // If we have a valid target and are ready to attack, fire.
        if (currentTarget != null && attackCooldown <= 0f)
        {
            Attack();
            attackCooldown = 1f / currentAttackSpeed;
        }
    }

    // Efficiently add enemies to the list when they enter the tower's range.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null && !enemiesInRange.Contains(enemyHealth))
            {
                enemiesInRange.Add(enemyHealth);
            }
        }
    }

    // Efficiently remove enemies from the list when they leave the tower's range.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemiesInRange.Remove(enemyHealth);
            }
        }
    }

    private void FindNewTarget()
    {
        // Remove any dead or invalid enemies from the list before searching.
        enemiesInRange.RemoveAll(enemy => enemy == null || enemy.CurrentHealth <= 0);

        // Find the closest valid enemy.
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var enemyHealth in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemyHealth.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemyHealth.transform;
            }
        }
        currentTarget = closestEnemy;
    }

    private bool IsTargetValid(HealthSystem target)
    {
        return target != null && target.CurrentHealth > 0;
    }

    void Attack()
    {
        if (currentTarget == null) return;

        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            Debug.DrawLine(transform.position, currentTarget.position, Color.red, 0.1f);
            targetHealth.TakeDamage((int)currentDamage);
        }
        else
        {
            // Target might have been destroyed by another source.
            currentTarget = null;
        }
    }

    public void ApplyModification(UpgradeData upgrade)
    {
        if (upgrade == null || appliedModifications.Contains(upgrade)) return;

        appliedModifications.Add(upgrade);
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        // Reset to base before reapplying all modifications.
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

        // Update the trigger collider radius with the new range.
        if (rangeCollider != null)
        {
            rangeCollider.radius = currentRange;
        }
    }

    private void ApplyStat(StatModification statMod)
    {
        switch (statMod.statName.ToLower())
        {
            case "damage":
                currentDamage += statMod.isPercentage ? baseDamage * (statMod.value / 100f) : statMod.value;
                break;
            case "range":
                currentRange += statMod.isPercentage ? baseRange * (statMod.value / 100f) : statMod.value;
                break;
            case "attackspeed":
                currentAttackSpeed += statMod.isPercentage ? baseAttackSpeed * (statMod.value / 100f) : statMod.value;
                break;
            default:
                Debug.LogWarning($"Unknown stat modification: {statMod.statName}");
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentRange > 0 ? currentRange : baseRange);
    }
}
