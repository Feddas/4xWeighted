using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> AI weighs a random tiles every 10 ticks </summary>
[CreateAssetMenu()]
public class AiAnyTile : IAi
{
    [Tooltip("When set to 0 a random seed will be used. To test specific scenario, set second param to specific value. Like seed:42")]
    public int Seed = 0;

    private System.Random rnd = new System.Random(42);
    private int computerTickSkipper = 10;

    public void OnEnable()
    {
        int seed = Seed;
        if (seed == 0)
        {
            seed = Random.Range(1, 1000);
        }
        this.rnd = new System.Random(seed);
    }

    public override void SolveTick(PlayerStats owner, TileMap allTiles)
    {
        // re-evaluate AI every 10 ticks
        if (computerTickSkipper++ < 10) return;
        computerTickSkipper = 0;

        // disable half of the existing weights
        int halfOfWeighted = (owner.WeightedTiles.Count) / 2; // get to two weights, then randomly remove one of the two weights
        foreach (var oldWeight in owner.WeightedTiles.OrderBy(wt => rnd.Next()).Take(halfOfWeighted))
        {
            TileWeight.Add(owner, oldWeight.Tile, 0);
            Debug.Log("AI removed " + oldWeight.Tile.name);
        }

        // Choose new tile
        var tile = allTiles.TileAt(rnd.Next(0, allTiles.width), rnd.Next(0, allTiles.height));

        // update game board
        var weight = TileWeight.Add(owner, tile, 1);
        Debug.Log("AI added " + weight);
    }
}
