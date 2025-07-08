// Assets/Scripts/System/DamageDealer.cs
using UnityEngine;

/// <summary>
/// A component for objects (like projectiles) that can deal damage.
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Tooltip("The amount of damage this object deals on impact.")]
    public int damage = 25;

    // We use OnTriggerEnter2D for 2D physics. Ensure your objects have a Collider2D set to 'Is Trigger'.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Try to get the HealthSystem component from the object we collided with.
        HealthSystem healthSystem = other.GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            // If the object has a health system, deal damage to it.
            healthSystem.TakeDamage(damage);

            // For a projectile, we'd typically destroy it after impact.
            // If this were an aura, we would not destroy it.
            Destroy(gameObject);
        }
    }
}