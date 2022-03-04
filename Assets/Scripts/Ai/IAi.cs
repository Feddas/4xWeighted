using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IAi
{
    public PlayerStats Owner { get; set; }

    /// <summary> Ai choses which tiles to assign weight </summary>
    public void SolveTick(TileMap allTiles);
}

/// <summary> AI weighs a random cell every 10 ticks </summary>
public class AiRandom : IAi
{
    public PlayerStats Owner { get; set; }

    private System.Random rnd = new System.Random(42);
    private int computerTickSkipper = 10;

    public AiRandom(PlayerStats owner, int seed = 42)
    {
        this.Owner = owner;
        this.rnd = new System.Random(seed);
    }

    public void SolveTick(TileMap allTiles)
    {
        // re-evaluate AI every 10 ticks
        if (computerTickSkipper++ < 10) return;
        computerTickSkipper = 0;

        // disable half of the existing weights
        int halfOfWeighted = (Owner.WeightedTiles.Count) / 2; // get to two weights, then randomly remove one of the two weights
        foreach (var oldWeight in Owner.WeightedTiles.OrderBy(wt => rnd.Next()).Take(halfOfWeighted))
        {
            TileWeight.Add(Owner, oldWeight.Tile, 0);
            Debug.Log("AI removed " + oldWeight.Tile.name);
        }

        // Choose new tile
        var tile = allTiles.TileAt(rnd.Next(0, allTiles.width), rnd.Next(0, allTiles.height));

        // update game board
        var weight = TileWeight.Add(Owner, tile, 1);
        Debug.Log("AI added " + weight);
    }
}
