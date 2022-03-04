﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class PlayerStats : ScriptableObject
{
    public enum ContributionEnum
    {
        None, // Neutral

        /// <summary> Player allowed to click buttons on this machine </summary>
        OnlyLocalPlayer,

        /// <summary> Computer sets weights for this player </summary>
        Computer,

        /// <summary> Not yet implemented </summary>
        RemotePlayer,
    }

    public int PlayerIndex;
    public Color Color = Color.green;

    [Tooltip("Exactly one player should have IsClicking marked true. This is a human player able to click tiles to set weights.")]
    public ContributionEnum Contribution = ContributionEnum.Computer;

    [Header("Debug")]
    public int TotalWeights;
    public int TotalPopulation;
    public List<TileWeight> WeightedTiles;
    public List<TileStatus> OccupiedTiles;
    public ITilePath PathingStrategy;
    public IAi Ai;

    public void Reset() // Note: don't use OnEnable here as we want this code called when a 2nd scene is loaded.
    {
        TotalWeights = 0;
        TotalPopulation = 0;
        WeightedTiles = new List<TileWeight>();
        OccupiedTiles = new List<TileStatus>();
        PathingStrategy = new TilePathManhattan(this);
        Ai = new AiRandom(this, Random.Range(1, 1000)); // to test specific scenario, set second param to something like seed:42
    }

    /// <summary>
    /// Shows weights only if the supplied player occupies the tiles and ClickingPlayer does not have weights there
    /// Note: this function has only been tested when called by a computer player (non-clicking player).
    /// 
    /// Functional test: Set computer weight to a specific tile. Click that same tile. Wait for that tile to be transfered to the computer.
    ///       1. Click tile you know will is the computers first weight in a way that also ensures the computer will still capture that tile.
    ///       EXPECTED: That tile gets a weight icon colored to clicking player
    ///       2. Wait for computer to capture that tile.
    ///       EXPECTED: Computer should not get weight icon when it finally captures the tile as the clicking player has priority on that icon.
    ///       3. Click that tile again to remove the clicking players weight icon.
    ///       EXPECTED: Computer should now get weight icon
    /// </summary>
    public void UiUpdateIconWeight()
    {
        // tiles the player occupies and the clicking player has no weight on
        bool justWatching = Player.Manager.ClickingPlayer == null;           // true when the clicking player has been eliminated
        var playersUncontestedDefense = this.WeightedTiles
            .Where(wt => wt.Tile.OwnedByPlayer == this                       // is occupied by this computer player
            && (justWatching || Player.Manager.ClickingPlayer.WeightedTiles  // & clicking player does not have a weight here 
            .All(cp => cp.Tile != wt.Tile)));                                // note: a join might be more performant https://docs.microsoft.com/en-us/dotnet/csharp/linq/perform-left-outer-joins
        foreach (var weight in playersUncontestedDefense)
        {
            weight.UpdateUi(this);
        }
    }
}
