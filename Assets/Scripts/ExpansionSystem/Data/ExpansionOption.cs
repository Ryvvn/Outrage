// Scripts/ExpansionSystem/Data/ExpansionOption.cs
using UnityEngine;

/// <summary>
/// Represents a single map expansion choice presented to the player.
/// In the new procedural system, instances of this are created at runtime.
/// </summary>
[CreateAssetMenu(fileName = "NewExpansionOption", menuName = "Tower Defense/Expansion Option")]
public class ExpansionOption : ScriptableObject
{
    [Header("Display Information")]
    public string optionName;
    [TextArea(3, 5)]
    public string strategicDescription;
    public Sprite previewImage;

    [Header("Gameplay Data")]
    public RiskLevel riskAssessment;

    // This field holds the dynamically generated chunk data.
    // It's not serialized, as it only exists at runtime during the expansion phase.
    [System.NonSerialized]
    public ProceduralExpansionChunk proceduralChunk;

    [Header("Balancing Modifiers")]
    [Tooltip("Expected multiplier for path length (1.0 = no change, 1.4 = 40% longer).")]
    public float pathLengthModifier = 1.0f;
    [Tooltip("Expected multiplier for available tower placement tiles (1.0 = no change, 1.25 = 25% more).")]
    public float buildSpaceModifier = 1.0f;
}