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
    public List<TileStatus> OccupiedTiles;
    public ITilePath PathingStrategy;
    public IAi Ai;

    public void Reset() // Note: don't use OnEnable here as we want this code called when a 2nd scene is loaded.
    {
        TotalWeights = 0;
        TotalPopulation = 0;
        WeightedTiles = new List<TileWeight>();
        OccupiedTiles = new List<TileStatus>();
        PathingStrategy = new TilePathManhattan(this);
        Ai = new AiRandom(this);
    }
}
