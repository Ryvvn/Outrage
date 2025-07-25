// Scripts/Player/CameraManager.cs
using UnityEngine;
using Cinemachine;

/// <summary>
/// Manages switching between different camera views (e.g., player follow and overview).
/// Requires a Cinemachine State-Driven Camera setup.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Setup")]
    [Tooltip("The State-Driven Camera that controls the active virtual camera.")]
    [SerializeField] private CinemachineStateDrivenCamera stateDrivenCamera;

    private Animator cameraAnimator;
    private bool isOverviewMode = false;

    void Start()
    {
        if (stateDrivenCamera == null)
        {
            Debug.LogError("State-Driven Camera is not assigned in the CameraManager!", this);
            enabled = false;
            return;
        }

        cameraAnimator = stateDrivenCamera.GetComponent<Animator>();
        if (cameraAnimator == null)
        {
            Debug.LogError("No Animator found on the State-Driven Camera GameObject!", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // Example: Use the Tab key to toggle between camera views.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCameraView();
        }
    }

    /// <summary>
    /// Toggles between the player follow camera and the tactical overview camera.
    /// </summary>
    public void ToggleCameraView()
    {
        isOverviewMode = !isOverviewMode;

        // This parameter name "IsOverview" must match the one in your Animator Controller.
        cameraAnimator.SetBool("IsOverview", isOverviewMode);

        Debug.Log($"Camera view switched to: {(isOverviewMode ? "Overview" : "Player Follow")}");
    }
}