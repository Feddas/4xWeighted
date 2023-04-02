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

    public override void SolveTick(PlayerStats owner, PlayerStats enemy, TileMap allTiles)
    {
        // chose an AI only if one hasn't been chosen
        if (chosenAi == null)
        {
            chosenAi = AiCanidates.OrderBy(wt => Random.value).First();
        }

        chosenAi.SolveTick(owner, enemy, allTiles);
    }
}
