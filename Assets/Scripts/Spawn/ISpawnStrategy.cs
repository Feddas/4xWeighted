using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnStrategy
{
    /// <returns> The next position chosen by the strategy </returns>
    Vector2Int NextPosition();
}
