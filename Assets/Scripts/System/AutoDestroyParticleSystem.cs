// Scripts/System/AutoDestroyParticleSystem.cs
using UnityEngine;

/// <summary>
/// A utility script that automatically destroys its GameObject after the
/// attached Particle System has finished playing.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroyParticleSystem : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Check if the particle system is still alive (playing).
        if (ps != null && !ps.IsAlive())
        {
            // Once it's finished, destroy the GameObject it's attached to.
            Destroy(gameObject);
        }
    }
}