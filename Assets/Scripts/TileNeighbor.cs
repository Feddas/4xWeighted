using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

public class TileNeighbor
{
    /// <summary> Neighboring tiles that this tile can move it's population onto </summary>
    public Tile North;
    public Tile South;
    public Tile East;
    public Tile West;

    private NesScripts.Controls.PathFind.Point position { get; set; }

    public TileNeighbor(NesScripts.Controls.PathFind.Point tilePosition)
    {
        position = tilePosition;
    }

    /// <summary> returns population of this tile after population has moved off of it </summary>
    public int TowardsWeighted(int ofPopulation, List<Tile> WeightedTiles)
    {
        if (WeightedTiles == null
        || WeightedTiles.Count == 0
        || WeightedTiles.Any(t => t.Position == this.position)) // Don't move any population, we are a weighted tile
        {
            return ofPopulation;
        }

        var tileToReinforce = pickWeightedTile(this.position, WeightedTiles);

        if (tileToReinforce.xDiffAbs == tileToReinforce.yDiffAbs) // send half both ways
        {
            moveTowards(ofPopulation / 2, tileToReinforce.xDiff, this.East, this.West);
            ofPopulation -= ofPopulation / 2;
            moveTowards(ofPopulation, tileToReinforce.yDiff, this.North, this.South);
        }
        else if (tileToReinforce.xDiffAbs > tileToReinforce.yDiffAbs) // send all along x
        {
            moveTowards(ofPopulation, tileToReinforce.xDiff, this.East, this.West);
        }
        else // send all along y
        {
            moveTowards(ofPopulation, tileToReinforce.yDiff, this.North, this.South);
        }

        return 0; // the population remaining on the tile is always 0. It has all been put into TilePopulationAdded
    }

    private void moveTowards(int ofPopulation, int magnitude, Tile onPositve, Tile onNegative)
    {
        if (magnitude > 0)
        {
            send(ofPopulation, onPositve);
        }
        else if (magnitude < 0)
        {
            send(ofPopulation, onNegative);
        }
        else
        {
            throw new System.Exception("Messed up moveTowards " + magnitude + ", " + onPositve.name + ", " + onNegative);
        }
    }

    private TileCandiate pickWeightedTile(NesScripts.Controls.PathFind.Point source, List<Tile> weightedTiles)
    {
        // get stats for all weighted tiles relative to the source tile
        List<TileCandiate> candiates = new List<TileCandiate>();
        TileCandiate currentCandiate = new TileCandiate();
        int biggestPopulation = 0;
        foreach (var tile in weightedTiles)
        {
            currentCandiate = new TileCandiate();
            currentCandiate.xDiff = tile.Position.x - source.x;
            currentCandiate.yDiff = tile.Position.y - source.y;
            currentCandiate.xDiffAbs = Mathf.Abs(currentCandiate.xDiff);
            currentCandiate.yDiffAbs = Mathf.Abs(currentCandiate.yDiff);
            currentCandiate.DistanceToTile = currentCandiate.xDiffAbs + currentCandiate.yDiffAbs;
            currentCandiate.WeightOnTile = tile.Weight.Current;
            currentCandiate.PopulationWeighted = tile.TilePopulation / tile.Weight.Current; // divide by the weight to make the tile look like it needs population
            currentCandiate.NotAsGoodRank = (tile.TilePopulation * tile.Weight.Current) / currentCandiate.DistanceToTile;
            candiates.Add(currentCandiate);

            // track which weighted tile has the biggest population
            if (currentCandiate.PopulationWeighted > biggestPopulation)
            {
                biggestPopulation = (tile.TilePopulation * tile.Weight.Current);
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

    private void send(int population, Tile neighboringTile)
    {
        if (neighboringTile == null)
            return;

        neighboringTile.TilePopulationAdded += population;
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
