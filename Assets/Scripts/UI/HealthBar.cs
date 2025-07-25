// Scripts/UI/HealthBar.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual representation of a health bar in world space.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Tooltip("The UI Image component that shows the current health amount.")]
    [SerializeField] private Image healthFillImage;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Updates the fill amount of the health bar image.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    /// <param name="maxHealth">The maximum possible health value.</param>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth > 0)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    /// <summary>
    /// Ensures the health bar always faces the camera.
    /// </summary>
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Makes the canvas face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}