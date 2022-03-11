using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used like an interface. The only reason it is a class is so that it can easily be shown in the Unity inspector.
/// </summary>
public abstract class IAi : ScriptableObject
{
    /// <summary> Ai choses which tiles to assign weight </summary>
    public abstract void SolveTick(PlayerStats owner, TileMap allTiles);
}
