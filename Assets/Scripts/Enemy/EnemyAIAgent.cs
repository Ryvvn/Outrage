// Scripts/Enemy/EnemyAIAgent.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem))]
public class EnemyAIAgent : MonoBehaviour
{
    private enum AIState { Pathfinding, Moving, Attacking, Dead }
    private AIState currentState;

    [Header("References")]
    private Transform baseCore;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;
    public int attackDamage = 25;

    [Header("Rewards")]
    public int bountyValue = 10;

    [Header("UI & Effects")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackTorque = 2f;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0.5f, 0);

    private List<Vector3> path;
    private int currentWaypointIndex;
    private float attackTimer;
    private float pathStuckTimer;
    private HealthSystem baseHealthSystem;
    private HealthBar healthBar;

    private Rigidbody2D rb;
    private HealthSystem healthSystem;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
    }

    void OnEnable()
    {
        if (baseCore == null)
        {
            GameObject coreObject = GameObject.FindGameObjectWithTag("BaseCore");
            if (coreObject != null)
            {
                baseCore = coreObject.transform;
                baseHealthSystem = baseCore.GetComponent<HealthSystem>();
            }
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            transform.rotation = Quaternion.identity;
        }

        if (healthBarPrefab != null && healthBar == null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity, transform);
            healthBar = healthBarInstance.GetComponent<HealthBar>();
        }
        if (healthBar != null) healthBar.gameObject.SetActive(true);

        healthSystem.OnHealthChanged += UpdateHealthBar;
        healthSystem.OnDied += HandleDeath;

        if (baseCore == null || Pathfinder.Instance == null)
        {
            Debug.LogError("Enemy AI cannot function without BaseCore or Pathfinder.", this);
            gameObject.SetActive(false);
            return;
        }

        TransitionToState(AIState.Pathfinding);
    }

    private IEnumerator FindPathRoutine()
    {
        yield return null;

        Vector3 startPathPosition = transform.position;
        PathNode startNode = GridManager.Instance.GetNodeFromWorldPoint(startPathPosition);

        if (startNode == null || !startNode.isWalkable)
        {
            Debug.LogWarning($"Enemy spawned at {startPathPosition} which is unwalkable. Finding nearest valid node.");
            var neighbors = GridManager.Instance.GetNeighbours(startNode);
            PathNode validStartNode = neighbors.FirstOrDefault(n => n != null && n.isWalkable);

            if (validStartNode != null)
            {
                startPathPosition = validStartNode.worldPosition; // CORRECTED: Use the valid node's position
                transform.position = startPathPosition; // Warp to valid spot
            }
            else
            {
                path = null;
            }
        }

        // Calculate the path from the (potentially corrected) start position.
        path = Pathfinder.Instance.FindPath(startPathPosition, baseCore.position);

        if (path != null && path.Count > 0)
        {
            TransitionToState(AIState.Moving);
        }
        else
        {
            Debug.LogError($"Enemy at {transform.position} could not find a path to the base at {baseCore.position}. It will be returned to the pool.");
            yield return new WaitForSeconds(1f);
            ReturnToPool();
        }
    }

    // Omitted the rest of the file for brevity as it remains unchanged.
    // Please keep the rest of your original code for this file.
    private void OnDisable()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthBar;
            healthSystem.OnDied -= HandleDeath;
        }
        StopAllCoroutines();
    }

    void Update()
    {
        if (currentState == AIState.Dead || (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Pause))
        {
            if (rb.bodyType == RigidbodyType2D.Kinematic) rb.velocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case AIState.Moving: HandleMovementState(); break;
            case AIState.Attacking: HandleAttackingState(); break;
        }
    }

    private void TransitionToState(AIState newState)
    {
        if (currentState == AIState.Dead) return;
        currentState = newState;

        switch (currentState)
        {
            case AIState.Pathfinding:
                path = null;
                if (rb != null) rb.velocity = Vector2.zero;
                StartCoroutine(FindPathRoutine());
                break;
            case AIState.Moving:
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0;
                rb.freezeRotation = true;
                currentWaypointIndex = 0;
                pathStuckTimer = 0f;
                break;
            case AIState.Attacking:
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                attackTimer = 0f;
                break;
        }
    }

    private void HandleMovementState()
    {
        if (Vector3.Distance(transform.position, baseCore.position) <= attackRange)
        {
            TransitionToState(AIState.Attacking);
            return;
        }

        if (path != null && currentWaypointIndex < path.Count)
        {
            Vector2 direction = (path[currentWaypointIndex] - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            if (Vector2.Distance(transform.position, path[currentWaypointIndex]) < 0.2f)
            {
                currentWaypointIndex++;
                pathStuckTimer = 0f;
            }
            pathStuckTimer += Time.deltaTime;
            if (pathStuckTimer > 3.0f)
            {
                TransitionToState(AIState.Pathfinding);
            }
        }
        else
        {
            TransitionToState(AIState.Pathfinding);
        }
    }

    private void HandleAttackingState()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            if (baseHealthSystem != null)
            {
                baseHealthSystem.TakeDamage(attackDamage);
            }
            attackTimer = attackCooldown;
        }
    }

    private void HandleDeath()
    {
        if (currentState == AIState.Dead) return;

        currentState = AIState.Dead;
        GameManager.Instance.AddMoney(bountyValue);
        GameManager.Instance.waveManager.EnemyDefeated();

        if (healthBar != null) healthBar.gameObject.SetActive(false);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.freezeRotation = false;

        Vector2 knockbackDirection = (transform.position - baseCore.position).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-knockbackTorque, knockbackTorque), ForceMode2D.Impulse);

        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(2f);
        GetComponent<Collider2D>().enabled = true;

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.ReturnEnemyToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }
}