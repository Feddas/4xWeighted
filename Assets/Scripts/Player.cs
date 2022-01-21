using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> Manager for all players in the current game </summary>
public class Player : MonoBehaviour
{
    public static Player Manager { get; private set; }

    [Tooltip("static (player# & color) and dynamic (population & weights) stats")]
    public List<PlayerStats> AllPlayers = null;

    [Header("Debug")]
    [Tooltip("Player this is performing actions. Such as tapping on a tiles Button component to set its weight.")]
    public PlayerStats ClickingPlayer = null;

    void Awake()
    {
        if (Manager == null)
        {
            Manager = this;
        }
        else
        {
            throw new System.Exception("More than one PlayerManager! " + this.name + " vs " + Manager.name);
        }
    }

    void Start()
    {
        // save which player to use when tile buttons are clicked
        ClickingPlayer = AllPlayers.FirstOrDefault(p => p.IsClicking);

        StartCoroutine(doTicks());
    }

    // void Update() { }

    private IEnumerator doTicks()
    {
        // do ticks for all players
        while (AllPlayers != null)
        {
            foreach (var player in AllPlayers)
            {
                movePopulation(player);
            }

            // after all populations have finished moving, update stats
            foreach (var player in AllPlayers)
            {
                updateStats(player);
            }

            // wait for tick interval
            yield return new WaitForSeconds(GameSettings.SecondsPerTick);
        }
    }

    /// <summary> moves population from all tiles occupied by this player to their weighted tiles </summary>
    private void movePopulation(PlayerStats player)
    {
        // create population for this tick
        foreach (var tile in player.OccupiedTiles)
        {
            tile.Tick();
        }

        // Move towards weight
        List<System.Action> ownershipHistory = new List<System.Action>(); // note: this is needed because OccupiedTiles can't be modified while iterating through them
        foreach (var tile in player.OccupiedTiles)
        {
            moveFrom(tile, ref ownershipHistory);
        }

        // Transfer tile ownership
        foreach (var change in ownershipHistory)
        {
            change(); // TODO: BUG: figure out why populations can still be negative after this call
        }
    }

    /// <summary> Queues all tiles occupied by a player to update population visuals on all tiles </summary>
    private void updateStats(PlayerStats player)
    {
        // update stats
        player.TotalPopulation = player.OccupiedTiles.Sum(t => t.TilePopulation + t.TilePopulationAdded);

        // Population moves finished, update Ui
        foreach (var tile in player.OccupiedTiles)
        {
            tile.RefreshIconPopulation();
        }
    }

    /// <summary> Moves population on origin towards a weight </summary>
    /// <param name="origin"></param>
    /// <param name="ownerHistory"> if a new tile is captured due to this move, it is added to this list. The capture action is not done in this function. </param>
    private void moveFrom(Tile origin, ref List<System.Action> ownerHistory)
    {
        Tile destination = origin.Neighbor.TowardsWeighted(origin);

        // do nothing
        if (destination == null) // this tile is weighted or no weights exist
        {
            return;
        }

        // reinforce
        if (destination.OwnedByPlayer == origin.OwnedByPlayer)
        {
            destination.TilePopulationAdded += origin.TilePopulation;
        }

        // attack
        else
        {
            var changeOwner = destination.DefendLater(origin.OwnedByPlayer, origin.TilePopulation);
            if (changeOwner != null)
            {
                ownerHistory.Add(changeOwner);
            }
        }

        // population was depleted from a reinforce or attack
        origin.TilePopulation = 0;
    }
}
