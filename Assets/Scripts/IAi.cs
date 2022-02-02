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

public class AiRandom : IAi
{
    private int computerTickSkipper = 10;

    public PlayerStats Owner { get; set; }

    public AiRandom(PlayerStats owner)
    {
        this.Owner = owner;
    }

    public void SolveTick(TileMap allTiles)
    {
        // re-evaluate AI every 10 ticks
        if (computerTickSkipper++ < 10) return;
        computerTickSkipper = 0;

        // disable half of the existing weights
        int halfOfWeighted = (Owner.WeightedTiles.Count + 1) / 2;
        foreach (var oldWeight in Owner.WeightedTiles.OrderBy(wt => Random.value).Take(halfOfWeighted))
        {
            TileWeight.Add(Owner, oldWeight.Tile, 0);
            Debug.Log("AI removed " + oldWeight.Tile.name);
        }

        // Choose new tile
        var tile = allTiles.TileAt(Random.Range(0, allTiles.width), Random.Range(0, allTiles.height));

        // update game board
        var weight = TileWeight.Add(Owner, tile, 1);
        weight.UiUpdateDefenseOnly(Owner);
        Debug.Log("AI added " + weight.Tile.name);
    }
}
