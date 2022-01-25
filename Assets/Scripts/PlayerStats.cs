using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlayerStats : ScriptableObject
{
    public enum ContributionEnum
    {
        None, // Neutral

        /// <summary> Player allowed to click buttons on this machine </summary>
        OnlyLocalPlayer,

        /// <summary> Computer sets weights for this player </summary>
        Computer,

        /// <summary> Not yet implemented </summary>
        RemotePlayer,
    }

    public int PlayerIndex;
    public Color Color = Color.green;

    [Tooltip("Exactly one player should have IsClicking marked true. This is a human player able to click tiles to set weights.")]
    public ContributionEnum Contribution = ContributionEnum.Computer;

    [Header("Debug")]
    public int TotalWeights;
    public int TotalPopulation;
    public List<TileWeight> WeightedTiles;
    public List<Tile> OccupiedTiles;
    public ITilePath PathingStrategy;

    void OnEnable()
    {
        TotalWeights = 0;
        TotalPopulation = 0;
        WeightedTiles = new List<TileWeight>();
        OccupiedTiles = new List<Tile>();
        PathingStrategy = new TilePathManhattan(this);
    }
}
