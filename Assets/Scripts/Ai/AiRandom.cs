using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> Chooses a random AI from a set of AIs </summary>
[CreateAssetMenu()]
public class AiRandom : IAi
{
    public List<IAi> AiCanidates = new List<IAi>();

    private IAi chosenAi;

    public override void Initialize(PlayerStats owner)
    {
        chosenAi = AiCanidates.OrderBy(wt => Random.value).First();
        chosenAi.Initialize(owner);
    }

    public override void SolveTick(PlayerStats owner, TileMap allTiles)
    {
        chosenAi.SolveTick(owner, allTiles);
    }
}
