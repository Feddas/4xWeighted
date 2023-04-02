using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEdge : ISpawnStrategy
{
    private readonly Vector2Int tilesSize;
    private List<Vector2Int> availableTiles;

    public SpawnEdge(int width, int height)
    {
        tilesSize = new Vector2Int(width, height);
        availableTiles = spawnEdgeDistribution();
    }

    /// <inheritdoc/>
    public Vector2Int NextPosition()
    {
        // pick a random tile from what is available
        int randomIndex = Random.Range(0, availableTiles.Count);
        var result = availableTiles[randomIndex];
        availableTiles.RemoveAt(randomIndex);

        return result;
    }

    /// <summary>
    /// Evenly distributes n positions along the edges of 2-dimensional array TilesSize.
    /// </summary>
    private List<Vector2Int> spawnEdgeDistribution()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int n = Player.Manager.AllPlayers.Count; // number of positions to distribute
        int[,] arr = new int[tilesSize.x, tilesSize.y]; // 2-dimensional array of size n x m

        int totalEdgeLength = 2 * tilesSize.x + 2 * (tilesSize.y - 2); // total length of all edges
        int segmentLength = totalEdgeLength / n;

        for (int i = 0; i < n; i++)
        {
            result.Add(indexIntoEdges(i * segmentLength, tilesSize));
        }

        return result;
    }

    private static Vector2Int indexIntoEdges(int index, Vector2Int size)
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
