using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public PlayerStats Stats;

    void Start()
    {
        StartCoroutine(DoTicks());
    }

    void Update() { }

    public IEnumerator DoTicks()
    {
        while (Stats != null)
        {
            // create new population
            foreach (var tile in Stats.OccupiedTiles)
            {
                tile.Tick();
            }

            // Move towards weight
            foreach (var tile in Stats.OccupiedTiles)
            {
                tile.TowardsWeighted();
            }

            Stats.TotalPopulation = Stats.OccupiedTiles.Sum(t => t.TilePopulation + t.TilePopulationAdded);

            // Population moves finished, update Ui
            foreach (var tile in Stats.OccupiedTiles)
            {
                tile.RefreshIconPopulation();
            }

            yield return new WaitForSeconds(GameSettings.SecondsPerTick);
        }
    }
}
