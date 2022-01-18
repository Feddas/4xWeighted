using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlayerStats : ScriptableObject
{
	public int PlayerIndex;
	public Color Color = Color.green;
	public int TotalWeights;
	public int TotalPopulation;
	public List<Tile> WeightedTiles;
	public List<Tile> OccupiedTiles;

	void OnEnable()
	{
		TotalWeights = 0;
		TotalPopulation = 0;
		WeightedTiles = new List<Tile>();
		OccupiedTiles = new List<Tile>();
	}
}
