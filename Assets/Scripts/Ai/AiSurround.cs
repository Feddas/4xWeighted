using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> AI weighs tiles surrounding its enemy. Does not care where its enemy has placed weights. </summary>
[CreateAssetMenu()]
public class AiSurround : IAi
{
    public PlayerStats Enemy
    {
        get
        {
            if (enemy == null)
            {
                enemy = Player.Manager.ClickingPlayer;
            }
            return enemy;
        }
    }
    [Tooltip("Player this AI will surround. If null, targets ClickingPlayer.")]
    [SerializeField]
    private PlayerStats enemy;

    /// <summary> Which AI player is controlling this instance of AiSurround </summary>
    private PlayerStats me;

    public override void Initialize(PlayerStats owner)
    {
        me = owner;
    }

    public override void SolveTick(PlayerStats owner, TileMap allTiles)
    {
        // find tiles that expand this AIs border surrounding the enemy
        var enemyTiles = allTiles.Tiles.Cast<TileStatus>()
            .Where(t => t.OwnedByPlayer == Enemy);
        var tilesBorderingEnemy = enemyTiles
            .SelectMany(unownedBorder)
            .Distinct();

        // set weights
        removeAllWeights(me); // remove old weights
        if (tilesBorderingEnemy.Count() > 0) // border is not yet fully secured
        {
            secureBorder(tilesBorderingEnemy);
        }
        else // border is fully secured, crush what remains
        {
            killEnemy(enemyTiles);
        }
    }

    private void secureBorder(IEnumerable<TileStatus> tilesBorderingEnemy)
    {
        foreach (var tile in tilesBorderingEnemy)
        {
            TileWeight.Add(me, tile, 1);
        }
    }

    /// <summary> Target randomly, one tile at a time, what remains of <paramref name="enemyTiles"/>.</summary>
    private void killEnemy(IEnumerable<TileStatus> enemyTiles)
    {
        var target = enemyTiles.OrderBy(wt => Random.value).FirstOrDefault();
        if (target != null)
        {
            TileWeight.Add(me, target, 1);
        }
        else
        {
            // throw new System.NotImplementedException(); // TODO: find new enemy
        }
    }

    /// <returns> tiles bordering <paramref name="tile" /> that are not already owned by this AI.</returns>
    private IEnumerable<TileStatus> unownedBorder(TileStatus tile)
    {
        return tile.Neighbor.All.Where(t
            => t.OwnedByPlayer != tile.OwnedByPlayer // don't include more of the enemies territory
            && t.OwnedByPlayer != me);               // don't include border this AI already owns
    }

    private void removeAllWeights(PlayerStats owner)
    {
        while (owner.WeightedTiles.Count > 0)
        {
            TileWeight.Add(owner, owner.WeightedTiles[0].Tile, 0);
        }
    }
}
