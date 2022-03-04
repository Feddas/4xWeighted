using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class TileWeight
{
    public TileStatus Tile { get; private set; }

    [Tooltip("Current amount of weight put on this tile")]
    [Range(0, 2)]
    public int Current;

    private int lastWeight = 0;

    /// <summary> Constructor that adds a TileWeight to a player </summary>
    /// <param name="weight"> throws an error if set to 0 </param>
    private TileWeight(PlayerStats player, TileStatus tile, int weight)
    {
        // guard against invalid weights
        if (weight == 0)
        {
            throw new System.Exception("TileWeight can not construct with weight 0");
        }

        // Add the tile
        this.Tile = tile;
        this.Current = weight;
        player.WeightedTiles.Add(this);
        updateWeight(player);
    }

    /// <summary> Adds a weight if it doesn't already exist </summary>
    /// <param name="newWeight"> Allows newWeight to be set by the non-Clicking players, such as AI </param>
    public static TileWeight Add(PlayerStats toPlayer, TileStatus tile, int newWeight)
    {
        var weight = toPlayer.WeightedTiles.FirstOrDefault(wt => wt.Tile == tile);
        if (weight == null && newWeight == 0)
        {
            return null; // a weight of 0 will remove the tile (in updateWeight()). Since it's not there to be removed, jobs done.
        }

        // make sure player has newWeight
        if (weight == null) // add it
        {
            weight = new TileWeight(toPlayer, tile, newWeight);
        }
        else                // update it
        {
            weight.Current = newWeight;
            weight.updateWeight(toPlayer);
        }
        return weight;
    }

    /// <summary> Weight cycles from 0 to 1 to 2 back to 0 </summary>
    public static TileWeight Next(PlayerStats forPlayer, TileStatus tile)
    {
        var weight = forPlayer.WeightedTiles.FirstOrDefault(wt => wt.Tile == tile);
        if (weight == null)  // this tile just gained its first weight
        {
            weight = new TileWeight(forPlayer, tile, 1);
        }
        else
        {
            weight.Current = (weight.Current + 1) % 2; // 2 - toggles weights. 3 - allows a double weight. 4 - allows triple weight.
            weight.updateWeight(forPlayer);
        }
        return weight;
    }

    /// <summary> Updates the UI specificially for the clickingplayer </summary>
    public void UiUpdateClickingPlayer(PlayerStats clickingPlayer)
    {
        // update Ui
        if (this.Current != 0)
        {
            this.Tile.Ui.IconWeight.color = clickingPlayer.Color;
        }

        // update all weighted icons
        foreach (var weight in clickingPlayer.WeightedTiles)
        {
            weight.UpdateUi(clickingPlayer);
        }
    }

    /// <summary> A tile has called TileWeight.updateWeight(). Update every weighted tile for a player to show what fraction of the total weight it uses. </summary>
    /// <summary> Set tile to a specific player and show what fraction of the total weight this uses. </summary>
    public void UpdateUi(PlayerStats player)
    {
        if (Player.Manager.ClickingPlayer != player // The clicking player can always update the UI of any tile, other players have limited access
            && this.Tile.OwnedByPlayer != player // A computers weight only shows UI if the computer also owns the tile
            && this.Tile.Ui.IconWeight.color != Player.Manager.ClickingPlayer.Color) // Ensure a tile owned and weighted by the computer does not override the clicking players weight
        {
            return;
        }

        this.Tile.Ui.IconWeight.color = player.Color;
        this.Tile.Ui.IconWeight.fillAmount = (float)this.Current / player.TotalWeights;
    }

    /// <summary> Ensures another player can color and use the weight icon </summary>
    public void RemoveUi(PlayerStats player)
    {
        if (this.Tile.Ui.IconWeight.color != player.Color) // UI is already not being shown for this player
        {
            return;
        }

        this.Tile.Ui.IconWeight.color = Color.clear;
        this.Tile.Ui.IconWeight.fillAmount = 0;
    }

    public override string ToString()
    {
        if (Tile != null)
        {
            return Current + " on " + Tile.ToString();
        }
        else
        {
            return Current + " no tile";
        }
    }

    /// <summary> Updates totals used in percentage calculations & removes weights equal to 0 </summary>
    private void updateWeight(PlayerStats player)
    {
        // guard clause, weight hasn't changed
        if (player == null || lastWeight == Current)
        {
            return;
        }

        // update which tiles have a weight icon
        if (Current == 0) // this tile just lost its weight
        {
            player.WeightedTiles.Remove(this);
            this.RemoveUi(player);
        }

        // change TotalWeights
        int weightDelta = Current - lastWeight;
        player.TotalWeights += weightDelta;
        lastWeight = Current;
    }
}
