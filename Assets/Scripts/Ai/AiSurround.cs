using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> AI weighs all tiles surrounding its enemy. Does not care where its enemy has placed weights. </summary>
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

    public override void SolveTick(PlayerStats owner, TileMap allTiles)
    {
        var enemyTiles = allTiles.Tiles.Cast<TileStatus>()
            .Where(t => t.OwnedByPlayer == Enemy);
        var tilesBorderingEnemy = enemyTiles
            .SelectMany(neighborThatAreDifferent)
            .Distinct();
        Debug.Log(Time.frameCount + " AiSurround found border of " + tilesBorderingEnemy.Count());

        // set weights to be only tiles bordering the enemy
        removeAllWeights(owner); // remove old weights
        foreach (var tile in tilesBorderingEnemy) // add new weights
        {
            TileWeight.Add(owner, tile, 1);
        }
    }

    private IEnumerable<TileStatus> neighborThatAreDifferent(TileStatus tile)
    {
        return tile.Neighbor.All.Where(t => t.OwnedByPlayer != tile.OwnedByPlayer);
    }

    private void removeAllWeights(PlayerStats owner)
    {
        while (owner.WeightedTiles.Count > 0)
        {
            TileWeight.Add(owner, owner.WeightedTiles[0].Tile, 0);
        }
    }
}
