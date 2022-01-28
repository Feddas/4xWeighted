using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Handles attack and defense on a tile. Transfers tile ownership.
/// </summary>
public class TileCombat
{
    private readonly Color darkenStep = new Color(.8f, .8f, .8f, 1); // Weight Icon is in front with brightest color, rest get progressively darker

    /// <summary> An attack pending on this tile </summary>
    private class Attack
    {
        [Tooltip("Who owns the attack")]
        public PlayerStats Owner;

        [Tooltip("Size of the attack")]
        public int Population;

        public Attack(PlayerStats owner, int armySize)
        {
            Owner = owner;
            Population = armySize;
        }
    }

    /// <summary> All attacks batched and pending to be resolved on this tile </summary>
    private List<Attack> pendingAttacks = new List<Attack>();

    /// <summary> Add a new pendingAttacks to be included in the combat resolution for this update tick </summary>
    public void AddAttack(PlayerStats defender, PlayerStats attacker, int armySize)
    {
        if (attacker != defender)
        {
            pendingAttacks.Add(new Attack(attacker, armySize));
        }
    }

    /// <summary> Resolve all pendingAttacks on this tile. </summary>
    public void Resolve(TileStatus tile)
    {
        // Add reinforcements newly added from neighboring tiles to this tile
        tile.TilePopulation += tile.TileReinforcements;
        tile.TileReinforcements = 0;

        // nothing to defend against
        if (pendingAttacks == null || pendingAttacks.Count == 0)
        {
            return;
        }

        // remove biggestAttacker from batchedAttacks
        pendingAttacks = pendingAttacks.GroupBy(a => a.Owner)
            .Select(group => new Attack(group.Key, group.Sum(row => row.Population))) // add together attacks coming from the same owner
            .ToList();
        int maxAttack = pendingAttacks.Max(a => a.Population);
        var biggestAttacker = pendingAttacks.First(a => a.Population == maxAttack);
        pendingAttacks.Remove(biggestAttacker);

        // All attackers evenly attack one another. The only troop count that matters is that of the biggestAttacker.
        // Example: if FinalAttacker = 100, Second = 80, Third = 1 then AttackSize = 100 - 80/2 - 1/2 = 100 - 40 - 0 = 60
        // Example: if FinalAttacker = 100, Second = 98, Third = 98 then AttackSize = 100 - 49 - 49 = 2
        foreach (var attack in pendingAttacks)
        {
            biggestAttacker.Population -= attack.Population / pendingAttacks.Count;
        }

        // What's left of the biggest attack is put up against this tiles defenses
        tile.TilePopulation -= biggestAttacker.Population;
        if (tile.TilePopulation < 0)
        {
            transferOwner(biggestAttacker.Owner, tile);
        }

        // clean up
        pendingAttacks.Clear();
    }

    /// <summary> Transfers the ownership of a tile from its current owner to ownerNew </summary>
    private void transferOwner(PlayerStats ownerNew, TileStatus tile)
    {
        // remove old owner
        if (tile.OwnedByPlayer != null)
        {
            tile.OwnedByPlayer.OccupiedTiles.Remove(tile);
        }

        // set tile ownership
        Color newColor = Color.white;
        tile.OwnedByPlayer = ownerNew; // null value means the tile is now neutral
        if (ownerNew != null) // update player
        {
            ownerNew.OccupiedTiles.Add(tile);
            newColor = ownerNew.Color;
        }
        tile.TilePopulation *= -1; // convert army

        // Set UI colors
        tile.Ui.IconPopulation.color = newColor * darkenStep;
        tile.Ui.TileBackground.color = tile.Ui.IconPopulation.color * darkenStep;
    }
}
