using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        || this.position == WeightedTiles[0].Position) // Don't move any population, we are a weighted tile
        {
            return ofPopulation;
        }

        int xDiff = WeightedTiles[0].Position.x - this.position.x;
        int yDiff = WeightedTiles[0].Position.y - this.position.y;
        int xDiffAbs = Mathf.Abs(xDiff);
        int yDiffAbs = Mathf.Abs(yDiff);

        if (xDiffAbs == yDiffAbs) // send half both ways
        {
            moveTowards(ofPopulation / 2, xDiff, this.East, this.West);
            ofPopulation -= ofPopulation / 2;
            moveTowards(ofPopulation, yDiff, this.North, this.South);
        }
        else if (xDiffAbs > yDiffAbs) // send all along x
        {
            moveTowards(ofPopulation, xDiff, this.East, this.West);
        }
        else // send all along y
        {
            moveTowards(ofPopulation, yDiff, this.North, this.South);
        }

        return 0;
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

    private void send(int population, Tile neighboringTile)
    {
        if (neighboringTile == null)
            return;

        neighboringTile.TilePopulationAdded += population;
    }
}
