using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Determines the starting (spawn) location of players and the games StartingArmySize
/// </summary>
[System.Serializable]
public class TileSpawn
{
    public enum Strategies
    {
        /// <summary> Players are spread evenly on the board egdes as far from one another as possible </summary>
        Edge,

        /// <summary> Players are randomly placed at least TotalTiles/(players * 20) tiles away from one another </summary>
        Random,
    }

    [Tooltip("Which algorithm to use to distribute the starting position of players")]
    public Strategies StrategyToUse;

    [Tooltip("The games StartingArmySize to be used for all players")]
    public int StartingArmySize = 10;

    /// <summary> Spawn positions of all players </summary>
    public Dictionary<PlayerStats, Vector2Int> PositionOf
    {
        get { return postionOf; }
        private set { postionOf = value; }
    }
    private Dictionary<PlayerStats, Vector2Int> postionOf = new Dictionary<PlayerStats, Vector2Int>();

    private ISpawnStrategy strategy;

    public void NewSpawnPositions(int width, int height)
    {
        // Reset variables
        switch (StrategyToUse)
        {
            case Strategies.Edge:
                strategy = new SpawnEdge(width, height);
                break;
            case Strategies.Random:
                strategy = new SpawnRandom(width, height);
                break;
            default:
                throw new System.NotImplementedException(StrategyToUse.ToString());
        }
        PositionOf.Clear();

        // Build SpawnPositions
        foreach (var player in Player.Manager.AllPlayers)
        {
            PositionOf.Add(player, strategy.NextPosition());
        }
    }
}
