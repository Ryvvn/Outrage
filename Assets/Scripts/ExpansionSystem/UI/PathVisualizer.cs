// Scripts/ExpansionSystem/UI/PathVisualizer.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visualizes potential enemy paths on the map using LineRenderer.
/// </summary>
public class PathVisualizer : MonoBehaviour
{
    public static PathVisualizer Instance { get; private set; }

    [SerializeField] private GameObject pathLinePrefab;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseMagnitude = 0.5f;

    private readonly List<LineRenderer> activeLines = new List<LineRenderer>();
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Draws a set of paths with a given color and starts a pulsing animation.
    /// </summary>
    /// <param name="paths">A list of paths, where each path is a list of Vector3 points.</param>
    /// <param name="color">The color to apply to the path lines.</param>
    public void DrawPaths(List<List<Vector3>> paths, Color color)
    {
        ClearPaths();

        foreach (var path in paths)
        {
            if (path == null || path.Count < 2) continue;

            GameObject lineObj = Instantiate(pathLinePrefab, transform);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

            lineRenderer.positionCount = path.Count;
            lineRenderer.SetPositions(path.ToArray());

            var gradient = new Gradient();
            gradient.SetKeys(
               new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
               new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
           );
            lineRenderer.colorGradient = gradient;

            activeLines.Add(lineRenderer);
        }

        // Only start a new pulse animation if there are lines to animate.
        if (activeLines.Count > 0)
        {
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }
    }

    /// <summary>
    /// Clears all currently displayed paths and stops any active animations.
    /// </summary>
    public void ClearPaths()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        foreach (var line in activeLines)
        {
            if (line != null) Destroy(line.gameObject);
        }
        activeLines.Clear();
    }

    private IEnumerator PulseAnimation()
    {
        // Use unscaledTime to ensure the animation continues even when the game is paused (Time.timeScale = 0).
        while (true)
        {
            // Calculate a sine wave for the alpha pulse effect.
            float alpha = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f * pulseMagnitude + (1 - pulseMagnitude);

            foreach (var line in activeLines)
            {
                if (line != null)
                {
                    Gradient currentGradient = line.colorGradient;
                    Gradient newGradient = new Gradient();
                    newGradient.SetKeys(
                        currentGradient.colorKeys,
                        new[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                    );
                    line.colorGradient = newGradient;
                }
            }
            yield return null;
        }
    }
}