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
                doTick(player);
            }
            yield return new WaitForSeconds(GameSettings.SecondsPerTick);
        }
    }

    private void doTick(PlayerStats player)
    {
        // create population for this tick
        foreach (var tile in player.OccupiedTiles)
        {
            tile.Tick();
        }

        // Move towards weight
        foreach (var tile in player.OccupiedTiles)
        {
            tile.TilePopulation = tile.Neighbor.TowardsWeighted(tile.TilePopulation, tile.OwnedByPlayer.WeightedTiles);
        }

        player.TotalPopulation = player.OccupiedTiles.Sum(t => t.TilePopulation + t.TilePopulationAdded);

        // Population moves finished, update Ui
        foreach (var tile in player.OccupiedTiles)
        {
            tile.RefreshIconPopulation();
        }
    }
}
