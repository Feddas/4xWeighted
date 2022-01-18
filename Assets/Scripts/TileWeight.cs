using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileWeight
{
    [Tooltip("Current amount of weight put on this tile")]
    [Range(0, 2)]
    public int Current;

    private int lastWeight;

    /// <summary> Weight cycles from 0 to 1 to 2 back to 0 </summary>
    public void NextWeight(PlayerStats player, Tile thisTile)
    {
        Current = (Current + 1) % 3;
        UpdateWeight(player, thisTile);
    }

    public void UpdateWeight(PlayerStats player, Tile thisTile)
    {
        // guard clause, weight hasn't changed
        if (player == null || lastWeight == Current)
        {
            return;
        }

        // update which tiles have a weight icon
        if (Current == 0) // this tile just lost its weight
        {
            thisTile.Ui.IconWeight.fillAmount = 0;
            player.WeightedTiles.Remove(thisTile);
        }
        else if (lastWeight == 0) // this tile just gained its first weight
        {
            thisTile.Ui.IconWeight.color = player.Color;
            player.WeightedTiles.Add(thisTile);
        }

        // change TotalWeights
        player.TotalWeights -= lastWeight;
        lastWeight = Current;
        player.TotalWeights += Current;

        // update all weighted icons
        foreach (var tile in player.WeightedTiles)
        {
            tile.RefreshIconWeight();
        }
    }
}
