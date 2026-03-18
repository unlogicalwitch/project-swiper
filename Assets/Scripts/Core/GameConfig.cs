using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("HP & Lives")]
    public int startingHP = 3;

    [Header("Symbol Spawning")]
    [Tooltip("Starting seconds between symbol spawns")]
    public float symbolSpawnRate = 3f;
    [Tooltip("Maximum number of symbols falling at the same time")]
    public int maxConcurrentSymbols = 3;

    [Header("Symbol Movement")]
    [Tooltip("Starting fall speed in world units per second")]
    public float symbolFallSpeed = 2.5f;

    [Header("Gesture Recognition")]
    public float matchConfidenceThreshold = 0.7f;

    [Header("Screen Bounds (world units)")]
    [Tooltip("Y position where symbols spawn (above screen top)")]
    public float spawnYPosition = 6f;
    [Tooltip("Y position where a symbol is considered missed (below screen bottom)")]
    public float missYPosition = -6f;
    [Tooltip("Random X spawn range — camera half-width is ~2.8 at 9:16")]
    public float spawnXOffset = 2.5f;

    [Header("Feedback")]
    [Tooltip("Seconds before a matched symbol is removed")]
    public float matchedSymbolRemovalDelay = 0.3f;
    [Tooltip("Seconds after a match before the next symbol can spawn")]
    public float nextSymbolSpawnDelay = 0.5f;

    [Header("Layered Symbols")]
    [Tooltip("Number of gesture layers a layered symbol requires to destroy")]
    public int layeredSymbolLayers = 3;
    [Tooltip("0 = never spawn layered, 1 = always spawn layered, 0.3 = 30% chance")]
    [Range(0f, 1f)]
    public float layeredSymbolChance = 0.3f;

    [Header("Difficulty Progression")]
    [Tooltip("How many seconds between each difficulty step")]
    public float difficultyRampInterval = 20f;
    [Tooltip("How much fall speed increases per step (world units/sec)")]
    public float fallSpeedIncreasePerStep = 0.4f;
    [Tooltip("How much spawn rate decreases per step (seconds)")]
    public float spawnRateDecreasePerStep = 0.25f;
    [Tooltip("Fastest allowed spawn rate (seconds)")]
    public float minSpawnRate = 0.8f;
    [Tooltip("Fastest allowed fall speed (world units/sec)")]
    public float maxFallSpeed = 6f;
}
