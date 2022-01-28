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

    /// <summary> Adds a TileWeight to a player </summary>
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

    public void UiUpdateAll(PlayerStats player)
    {
        // update Ui
        if (this.Current != 0)
        {
            this.Tile.Ui.IconWeight.color = player.Color;
        }
        else
        {
            this.Tile.Ui.IconWeight.fillAmount = 0;
        }

        // update all weighted icons
        foreach (var weight in player.WeightedTiles)
        {
            refreshIconWeight(player, weight);
        }
    }

    /// <summary>
    /// Shows weights only if the supplied player occupies the tiles and ClickingPlayer does not have weights there
    /// Functional test: Set computer weight to a specific tile. Click that same tile. Wait for that tile to be transfered to the computer.
    ///       EXPECTED: Computer should not get weight icon when it finally captures the tile.
    ///       EXPECTED: When clicking the tile to remove the clicking players weight, computer should now get weight icon
    /// </summary>
    /// <param name="player"></param>
    public void UiUpdateDefenseOnly(PlayerStats player)
    {
        // tiles the player occupies and the clicking player has no weight on
        bool justWatching = Player.Manager.ClickingPlayer == null;           // true when the clicking player has been eliminated
        var playersUncontestedDefense = player.WeightedTiles
            .Where(wt => wt.Tile.OwnedByPlayer == player                     // is occupied by this computer player
            && (justWatching || Player.Manager.ClickingPlayer.WeightedTiles  // & clicking player does not have a weight here 
            .All(cp => cp.Tile != wt.Tile)));                                // note: a join might be more performant https://docs.microsoft.com/en-us/dotnet/csharp/linq/perform-left-outer-joins
        foreach (var weight in playersUncontestedDefense)
        {
            this.Tile.Ui.IconWeight.color = player.Color;
            refreshIconWeight(player, weight);
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
        }

        // change TotalWeights
        int weightDelta = Current - lastWeight;
        player.TotalWeights += weightDelta;
        lastWeight = Current;
    }

    /// <summary> A tile has called TileWeight.updateWeight(). Update every weighted tile for a player to show what fraction of the total weight it uses. </summary>
    private void refreshIconWeight(PlayerStats player, TileWeight weight)
    {
        weight.Tile.Ui.IconWeight.fillAmount = (float)weight.Current / player.TotalWeights;
    }
}
