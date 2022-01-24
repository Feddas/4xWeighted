using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlayerStats : ScriptableObject
{
    public int PlayerIndex;
    public Color Color = Color.green;

    [Tooltip("Exactly one player should have IsClicking marked true. This is a human player able to click tiles to set weights.")]
    public bool IsClicking = false;

    [Header("Debug")]
    public int TotalWeights;
    public int TotalPopulation;
    public List<Tile> WeightedTiles;
    public List<Tile> OccupiedTiles;
    public ITilePath PathingStrategy;

    void OnEnable()
    {
        TotalWeights = 0;
        TotalPopulation = 0;
        WeightedTiles = new List<Tile>();
        OccupiedTiles = new List<Tile>();
        PathingStrategy = new TilePathManhattan(this);
    }
}
