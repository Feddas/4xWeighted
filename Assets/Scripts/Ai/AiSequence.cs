using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> AI weighs tiles based on sequence of actions </summary>
[CreateAssetMenu()]
public class AiSequence : IAi
{
    /// <summary> How much weight tile X,Y should be set to at a specific tick </summary>
    private struct WeightAction
    {
        /// <summary> The tick at which this weight action should occur </summary>
        public int TickCount;

        /// <summary> The tile that will be the target of this weight action </summary>
        public int TileX, TileY;

        /// <summary> When true, weight is added to the tile. When false, weight is removed from the tile. </summary>
        public int NewWeight;

        public WeightAction(int tickCount, int tileX, int tileY, int newWeight = 1)
        {
            TickCount = tickCount;
            TileX = tileX;
            TileY = tileY;
            NewWeight = newWeight;
        }
    }

    private List<WeightAction> Sequence = new List<WeightAction>()
    {
        new WeightAction(1, 3, 3), // weigh tile 3,3 at tick 1
        new WeightAction(4, 4, 4), // weigh tile 4,4 at tick 4
    };

    [System.NonSerialized]
    private uint tickCount = 0;

    public override void SolveTick(PlayerStats owner, TileMap allTiles)
    {
        tickCount++;

        // preform actions for this tick
        WeightAction currentAction = firstAt(tickCount);
        while (currentAction.TickCount != 0)
        {
            // add weight
            var tile = allTiles.TileAt(currentAction.TileX, currentAction.TileY);
            var weight = TileWeight.Add(owner, tile, currentAction.NewWeight);
            Debug.Log("AI added " + weight);

            // check for other actions at this tickCount
            Sequence.Remove(currentAction);
            currentAction = firstAt(tickCount);
        }
    }

    /// <returns> first action in Sequence that occurs at tickCount </returns>
    private WeightAction firstAt(uint tickCount)
    {
        return Sequence.FirstOrDefault(s => s.TickCount == tickCount);
    }
}
