using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> AI weighs tiles surrounding its enemy. Does not care where its enemy has placed weights. </summary>
[CreateAssetMenu()]
public class AiSurround : IAi
{
    public override event System.Action OnEnemyDefeated;

    public override void SolveTick(PlayerStats owner, PlayerStats enemy, TileMap allTiles)
    {
        // find tiles that expand this AIs border surrounding the enemy
        var enemyTiles = allTiles.Tiles.Cast<TileStatus>()
            .Where(t => t.OwnedByPlayer == enemy);
        var tilesBorderingEnemy = enemyTiles
            .SelectMany(t => unownedBorder(owner, t))
            .Distinct();

        // set weights
        removeAllWeights(owner); // remove old weights
        if (tilesBorderingEnemy.Count() > 0) // border is not yet fully secured
        {
            secureBorder(owner, tilesBorderingEnemy);
        }
        else // border is fully secured, crush what remains
        {
            killEnemy(owner, enemyTiles);
        }
        // Debug.Log($"{Time.frameCount} AI {owner.name} using {owner.WeightedTiles.Count} weights to target {enemy.name}.");
    }

    private void secureBorder(PlayerStats owner, IEnumerable<TileStatus> tilesBorderingEnemy)
    {
        foreach (var tile in tilesBorderingEnemy)
        {
            TileWeight.Add(owner, tile, 1);
        }
    }

    /// <summary> Target randomly, one tile at a time, what remains of <paramref name="enemyTiles"/>.</summary>
    private void killEnemy(PlayerStats owner, IEnumerable<TileStatus> enemyTiles)
    {
        var target = enemyTiles.OrderBy(wt => Random.value).FirstOrDefault();
        if (target != null)
        {
            TileWeight.Add(owner, target, 1);
        }
        else
        {
            OnEnemyDefeated.Invoke();
        }
    }

    /// <returns> tiles bordering <paramref name="tile" /> that are not already owned by this AI.</returns>
    private IEnumerable<TileStatus> unownedBorder(PlayerStats owner, TileStatus tile)
    {
        return tile.Neighbor.All.Where(t
            => t.OwnedByPlayer != tile.OwnedByPlayer // don't include more of the enemies territory
            && t.OwnedByPlayer != owner);            // don't include border this AI already owns
    }

    private void removeAllWeights(PlayerStats owner)
    {
        while (owner.WeightedTiles.Count > 0)
        {
            TileWeight.Add(owner, owner.WeightedTiles[0].Tile, 0);
        }
    }
}
