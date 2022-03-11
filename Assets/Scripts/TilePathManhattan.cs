using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface ITilePath
{
    public PlayerStats Owner { get; set; }

    TileStatus TowardsWeighted(TileStatus tile);
}

/// <summary>
/// Pick goal of a path and path towards it one unit at a time without looking ahead for obstacles.
/// </summary>
public class TilePathManhattan : ITilePath
{
    /// <summary> stats of a tile that is a candiate weighted tile to be reinforced relative to the tile that will reinforce it </summary>
    public struct TileCandiate
    {
        public int WeightIndex;
        public int DistanceToTile;
        public int PopulationWeighted;
        public int WeightOnTile;
        public int xDiff;
        public int yDiff;
        public int xDiffAbs;
        public int yDiffAbs;

        /// <summary> Haven't thought through the math on why this way doesn't work as well as the uncommented way. But, NotAsGoodRank does avoid the extra "biggestPopulation" loop </summary>
        public float NotAsGoodRank;
    }

    /// <summary> Swaps between true and false each time it is read. This allows the path taken to be more evenly distributed. </summary>
    private bool goVertical
    {
        get
        {
            _goVertical = false == _goVertical;
            return _goVertical;
        }
    }
    private bool _goVertical = false;

    public PlayerStats Owner { get; set; }

    /// <summary> Owner must be set before TowardsWeighted() can be called </summary>
    public TilePathManhattan() { }
    public TilePathManhattan(PlayerStats owner)
    {
        this.Owner = owner;
    }

    public TileStatus TowardsWeighted(TileStatus tile)
    {
        List<TileWeight> WeightedTiles = Owner.WeightedTiles;

        if (WeightedTiles == null
        || WeightedTiles.Count == 0
        || WeightedTiles.Any(t => t.Tile.Position == tile.Position)) // Don't move any population, we are a weighted tile
        {
            return null;
        }

        var tileToReinforce = pickWeightedTile(tile.Position, WeightedTiles);

        if (tileToReinforce.xDiffAbs == tileToReinforce.yDiffAbs) // send along owned tiles or switch between side using goVertical
        {
            // narrow down to two tiles
            var xTile = pickOnAxis(tileToReinforce.xDiff, tile.Neighbor.East, tile.Neighbor.West);
            var yTile = pickOnAxis(tileToReinforce.yDiff, tile.Neighbor.North, tile.Neighbor.South);
            return pickVerticalOrHorizontal(Owner, xTile, yTile);
        }
        else if (tileToReinforce.xDiffAbs > tileToReinforce.yDiffAbs) // send population along x
        {
            return pickOnAxis(tileToReinforce.xDiff, tile.Neighbor.East, tile.Neighbor.West);
        }
        else // send population along y
        {
            return pickOnAxis(tileToReinforce.yDiff, tile.Neighbor.North, tile.Neighbor.South);
        }
    }

    /// <summary> Vertical tile selection has the same distance to the weighted tile as Horizontal. Determine which one to pick. </summary>
    private TileStatus pickVerticalOrHorizontal(PlayerStats populationOwner, TileStatus xTile, TileStatus yTile)
    {
        // narrow down to one tile
        bool noneOwned = xTile.OwnedByPlayer != populationOwner && xTile.OwnedByPlayer != populationOwner;
        bool allOwned = xTile.OwnedByPlayer == populationOwner && xTile.OwnedByPlayer == populationOwner;
        if (noneOwned || allOwned) // going either way works the same
        {
            // pick which way to go based on the last value of goVertical
            return goVertical ? yTile : xTile;
        }
        else if (xTile == populationOwner) // favor reinforcing a tile over expanding
        {
            return xTile;
        }
        else
        {
            return yTile;
        }
    }

    /// <summary> Pick which tile along an axis, horizontal or vertical, that more quickly gets the population towards the weighted tiles relative location </summary>
    /// <param name="magnitude"> Relative location of the weighted tile goal </param>
    /// <param name="onPositive"></param>
    /// <param name="onNegative"></param>
    /// <returns></returns>
    private TileStatus pickOnAxis(int magnitude, TileStatus onPositive, TileStatus onNegative)
    {
        if (magnitude > 0)
        {
            return onPositive;
        }
        else if (magnitude < 0)
        {
            return onNegative;
        }
        else
        {
            throw new System.Exception("Messed up moveTowards " + magnitude + ", " + onPositive.name + ", " + onNegative.name);
        }
    }

    /// <summary> determine which one of this players weighted tiles this tile should target to move population towards </summary>
    /// <param name="source"></param>
    /// <param name="weightedTiles"></param>
    /// <returns></returns>
    private TileCandiate pickWeightedTile(NesScripts.Controls.PathFind.Point source, List<TileWeight> weightedTiles)
    {
        // get stats for all weighted tiles relative to the source tile
        List<TileCandiate> candiates = new List<TileCandiate>();
        TileCandiate currentCandiate = new TileCandiate();
        TileStatus tile;
        int biggestPopulation = 0;
        foreach (var weight in weightedTiles)
        {
            tile = weight.Tile;
            currentCandiate = new TileCandiate();
            currentCandiate.xDiff = tile.Position.x - source.x;
            currentCandiate.yDiff = tile.Position.y - source.y;
            currentCandiate.xDiffAbs = Mathf.Abs(currentCandiate.xDiff);
            currentCandiate.yDiffAbs = Mathf.Abs(currentCandiate.yDiff);
            currentCandiate.DistanceToTile = currentCandiate.xDiffAbs + currentCandiate.yDiffAbs;
            currentCandiate.WeightOnTile = weight.Current;
            currentCandiate.PopulationWeighted = tile.TilePopulation / weight.Current; // divide by the weight to make the tile look like it needs population
            currentCandiate.NotAsGoodRank = (tile.TilePopulation * weight.Current) / currentCandiate.DistanceToTile;
            candiates.Add(currentCandiate);

            // track which weighted tile has the biggest population
            if (currentCandiate.PopulationWeighted > biggestPopulation)
            {
                biggestPopulation = (tile.TilePopulation * weight.Current);
            }
        }

        // rank the weighted tiles to determine which one needs population the most
        float topRank = 0;
        foreach (var candiate in candiates)
        {
            // reinforcements are favored by the most unpopulated weighted tile
            float candiatesRank = (biggestPopulation - candiate.PopulationWeighted) / candiate.DistanceToTile;
            //Debug.Log(source.ToString() + " Ranks unused of " + candiate.NotAsGoodRank.ToString("0.00")
            // + " vs " + candiatesRank.ToString("0.00") + " on frame " + Time.frameCount);
            if (candiatesRank > topRank)
            {
                topRank = candiatesRank;
                currentCandiate = candiate;
            }
        }

        //return candiates.MaxObject(c => c.NotAsGoodRank);
        return currentCandiate;
    }
}

//https://stackoverflow.com/a/1101850
//static class EnumerableExtensions {
//    public static T MaxObject<T,U>(this IEnumerable<T> source, System.Func<T,U> selector)
//      where U : System.IComparable<U> {
//       if (source == null) throw new System.ArgumentNullException("source");
//       bool first = true;
//       T maxObj = default(T);
//       U maxKey = default(U);
//       foreach (var item in source) {
//           if (first) {
//                maxObj = item;
//                maxKey = selector(maxObj);
//                first = false;
//           } else {
//                U currentKey = selector(item);
//                if (currentKey.CompareTo(maxKey) > 0) {
//                    maxKey = currentKey;
//                    maxObj = item;
//                }
//           }
//       }
//       if (first) throw new System.InvalidOperationException("Sequence is empty.");
//       return maxObj;
//    }
//}
