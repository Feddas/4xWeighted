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
    public enum SpawnStrategy
    {
        /// <summary> Players are spread evenly on the board egdes as far from one another as possible </summary>
        Edge,

        /// <summary> Players are randomly placed at least TotalTiles/(players * 20) tiles away from one another </summary>
        Random,
    }

    [Tooltip("Which algorithm to use to distribute the starting position of players")]
    public SpawnStrategy strategy;

    [Tooltip("The games StartingArmySize to be used for all players")]
    public int StartingArmySize = 10;

    /// <summary> Spawn positions of all players </summary>
    public Dictionary<PlayerStats, Vector2Int> PositionOf
    {
        get { return postionOf; }
        private set { postionOf = value; }
    }
    private Dictionary<PlayerStats, Vector2Int> postionOf = new Dictionary<PlayerStats, Vector2Int>();

    private Vector2Int TilesSize;
    private List<Vector2Int> availableTiles;
    private int spawnMargins;

    public void NewSpawnPositions(int width, int height)
    {
        // Reset variables
        TilesSize = new Vector2Int(width, height);
        PositionOf.Clear();
        availableTiles = spawnablePositions();
        spawnMargins = (width * height) / (Player.Manager.AllPlayers.Count * 20);

        // build SpawnPositions
        foreach (var player in Player.Manager.AllPlayers)
        {
            PositionOf.Add(player, nextSpawnPosition());
        }
    }

    /// <returns> A Vector2Int to represent every position in TilesSize </returns>
    private List<Vector2Int> allTiles()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int x = 0; x < TilesSize.x; x++)
        {
            result.AddRange(
                Enumerable.Range(0, TilesSize.y)
                .Select(y => new Vector2Int(x, y)));
        }
        return result;
    }

    private List<Vector2Int> spawnablePositions()
    {
        switch (strategy)
        {
            case SpawnStrategy.Edge: // only a few cells on the edges are available
                return spawnEdgeDistribution();
            case SpawnStrategy.Random: // then every cell on the board is available.
                var result = new List<Vector2Int>();
                for (int x = 0; x < TilesSize.x; ++x)
                {
                    for (int y = 0; y < TilesSize.y; ++y)
                    {
                        result.Add(new Vector2Int(x, y));
                    }
                }
                return result;
            default:
                Debug.LogException(new System.Exception("Strategy " + strategy.ToString() + " is unknown"));
                return null;
        }
    }

    private Vector2Int nextSpawnPosition()
    {
        // pick a random tile from what is available
        int randomIndex = Random.Range(0, availableTiles.Count);
        var result = availableTiles[randomIndex];
        availableTiles.RemoveAt(randomIndex);

        // clean up available tiles
        switch (strategy)
        {
            case SpawnStrategy.Edge:
                return result;
            case SpawnStrategy.Random: // remove all tiles within a spawnMargins
                availableTiles = availableTiles.Where(c => c.x > result.x + spawnMargins || c.x > result.x - spawnMargins || c.y > result.y + spawnMargins || c.y > result.y - spawnMargins).ToList();
                return result;
            default:
                Debug.LogException(new System.Exception("Strategy " + strategy.ToString() + " is unknown"));
                return new Vector2Int(-1, -1);
        }
    }

    /// <summary>
    /// Evenly distributes n positions along the edges of 2-dimensional array TilesSize.
    /// </summary>
    private List<Vector2Int> spawnEdgeDistribution()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int n = Player.Manager.AllPlayers.Count; // number of positions to distribute
        int[,] arr = new int[TilesSize.x, TilesSize.y]; // 2-dimensional array of size n x m

        int totalEdgeLength = 2 * TilesSize.x + 2 * (TilesSize.y - 2); // total length of all edges
        int segmentLength = totalEdgeLength / n;

        for (int i = 0; i < n; i++)
        {
            result.Add(IndexIntoEdges(i * segmentLength, TilesSize));
        }

        return result;
    }

    public static Vector2Int IndexIntoEdges(int index, Vector2Int size)
    {
        int rows = size.x;
        int columns = size.y;
        Vector2Int result = new Vector2Int();

        if (index < columns)
        {
            // Top edge
            result.x = 0;
            result.y = index;
        }
        else if (index < columns + rows - 1)
        {
            // Right edge
            result.x = index - (columns - 1);
            result.y = columns - 1;
        }
        else if (index < columns + rows + columns - 2)
        {
            // Bottom edge
            result.x = rows - 1;
            result.y = columns - (index - (columns + rows - 2)) - 2;
        }
        else
        {
            // Left edge
            result.x = rows - (index - (columns + rows + columns - 3)) - 2;
            result.y = 0;
        }

        return result;
    }
}
