using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnRandom : ISpawnStrategy
{
    private Vector2Int tilesSize;
    private List<Vector2Int> availableTiles;
    private int spawnMargins;

    public SpawnRandom(int width, int height)
    {
        tilesSize = new Vector2Int(width, height);
        availableTiles = allTiles();

        // the indices into this array are the spawnMargins, the value at those indices are the min number of tiles needed to use that margin.
        var marginRequirements = Enumerable.Range(0, 6).Select(n => Player.Manager.AllPlayers.Count * 8 * triangleNumber(n));
        spawnMargins = marginRequirements.Where(n => n < width * height).Select((n, i) => i).Max();
    }

    /// <inheritdoc/>
    public Vector2Int NextPosition()
    {
        // pick a random tile from what is available
        int randomIndex = Random.Range(0, availableTiles.Count);
        var result = availableTiles[randomIndex];
        availableTiles.RemoveAt(randomIndex);

        // remove all tiles within a spawnMargins
        availableTiles = availableTiles.Where(c => c.x > result.x + spawnMargins || c.x < result.x - spawnMargins || c.y > result.y + spawnMargins || c.y < result.y - spawnMargins).ToList();
        return result;
    }

    /// <returns> A Vector2Int to represent every position in TilesSize </returns>
    private List<Vector2Int> allTiles()
    {
        var result = new List<Vector2Int>();
        for (int x = 0; x < tilesSize.x; ++x)
        {
            for (int y = 0; y < tilesSize.y; ++y)
            {
                result.Add(new Vector2Int(x, y));
            }
        }
        return result;
    }

    /// <summary> A Triangle Number is like factorial, but with addition. i.e. Triangle(5) = 1+2+3+4+5 = 15 </summary>
    int triangleNumber(int n)
    {
        return (n * n + n) / 2;
    }
}
