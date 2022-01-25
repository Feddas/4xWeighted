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

    [Tooltip("Used to trigger resolution of attacks")]
    public TileMap AllTiles = null;

    [Header("Debug")]
    [Tooltip("Player this is performing actions. Such as tapping on a tiles Button component to set its weight.")]
    public PlayerStats ClickingPlayer = null;

    private IEnumerable<PlayerStats> computerPlayers;

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
        ClickingPlayer = AllPlayers.FirstOrDefault(p => p.Contribution == PlayerStats.ContributionEnum.OnlyLocalPlayer);
        computerPlayers = AllPlayers.Where(p => p.Contribution == PlayerStats.ContributionEnum.Computer);

        StartCoroutine(doTicks());
    }

    // void Update() { }

    private IEnumerator doTicks()
    {
        // do ticks for all players
        while (AllPlayers != null)
        {
            // initiate attacks
            foreach (var player in AllPlayers)
            {
                movePopulation(player);
            }

            // resolve attacks
            AllTiles.ResolveAttacks();

            // after all populations have finished moving, update stats
            foreach (var player in AllPlayers)
            {
                updateStats(player);
            }

            // run computer
            foreach (var computer in computerPlayers)
            {
                computer.PathingStrategy.RunAi(AllTiles, computer);
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
        foreach (var tile in player.OccupiedTiles)
        {
            moveFrom(tile, player);
        }
    }

    /// <summary> Queues all tiles occupied by a player to update population visuals on all tiles </summary>
    private void updateStats(PlayerStats player)
    {
        // update stats
        player.TotalPopulation = player.OccupiedTiles.Sum(t => t.TilePopulation + t.TileReinforcements);

        // Population moves finished, update Ui
        foreach (var tile in player.OccupiedTiles)
        {
            tile.RefreshIconPopulation();
        }
    }

    /// <summary> Moves population on origin towards a weight </summary>
    /// <param name="origin"></param>
    private void moveFrom(Tile origin, PlayerStats player)
    {
        Tile destination = player.PathingStrategy.TowardsWeighted(origin);

        // do nothing
        if (destination == null) // this tile is weighted or no weights exist
        {
            return;
        }

        // reinforce
        if (destination.OwnedByPlayer == player)
        {
            destination.TileReinforcements += origin.TilePopulation;
        }

        // attack
        else
        {
            destination.DefendAdd(player, origin.TilePopulation);
        }

        // population was depleted from a reinforce or attack
        origin.TilePopulation = 0;
    }
}
