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
			foreach (var tile in Stats.OccupiedTiles)
			{
				tile.Tick();
			}

			Stats.TotalPopulation = Stats.OccupiedTiles.Sum(t => t.TilePopulation);

			foreach (var tile in Stats.OccupiedTiles)
			{
				tile.RefreshIconPopulation();
			}

            yield return new WaitForSeconds(GameSettings.SecondsPerTick);
        }
    }
}
