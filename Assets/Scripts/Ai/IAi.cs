using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is used like an interface. The only reason it is a class is so that it can easily be shown in the Unity inspector.
/// </summary>
public abstract class IAi : ScriptableObject
{
    public virtual event System.Action OnEnemyDefeated;

    public virtual PlayerStats GetEnemy(PlayerStats owner)
    {
        return Player.Manager.AllPlayers.Where(p => p != owner).FirstOrDefault();
    }

    /// <summary> Ai choses which tiles to assign weight </summary>
    public abstract void SolveTick(PlayerStats owner, PlayerStats enemy, TileMap allTiles);
}
