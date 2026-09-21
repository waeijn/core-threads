using UnityEngine;

/// <summary>
/// ScriptableObject holding all configuration parameters for the procedural map generator.
/// Assign in the Inspector and reference from MapSystem/MapGenerator.
/// </summary>
[CreateAssetMenu(menuName = "Data/MapConfig")]
public class MapConfig : ScriptableObject
{
    [Header("Sprite Sheet")]
    [Tooltip("The network_icons.png sprite sheet sliced into 6 sprites (indices 0-5).")]
    public Sprite[] nodeSprites = new Sprite[6];

    [Header("Enemy Rosters")]
    public ActEnemyRoster act1Roster;
    public ActEnemyRoster act2Roster;
    public ActEnemyRoster act3Roster;

    [Header("Act 1 - Floppy Sector")]
    public int act1StartingPaths = 2;
    [Tooltip("Middle layer count (excluding layer 0 and boss layer).")]
    public int act1MiddleLayers = 2;
    public int act1BossSpriteIndex = 3;

    [Header("Act 2 - System RAM")]
    public int act2StartingPaths = 3;
    public int act2MiddleLayers = 3;
    public int act2BossSpriteIndex = 4;

    [Header("Act 3 - CPU Core")]
    public int act3StartingPaths = 4;
    public int act3MiddleLayers = 4;
    public int act3BossSpriteIndex = 5;

    [Header("Map Layout")]
    [Tooltip("Vertical spacing in pixels between layers.")]
    public float layerHeight = 180f;
    [Tooltip("Horizontal spacing in pixels between nodes in the same layer.")]
    public float columnWidth = 160f;
    [Tooltip("Scale multiplier for boss nodes.")]
    public float bossScale = 1.5f;
    [Tooltip("Width of the line connector images.")]
    public float connectorWidth = 4f;

    [Header("Colors")]
    public Color unlockedColor = Color.white;
    public Color lockedColor   = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color visitedColor  = new Color(0.4f, 0.9f, 0.4f, 0.8f);
    public Color connectorActiveColor = new Color(0.8f, 0.8f, 0.8f, 0.9f);
    public Color connectorLockedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
}
