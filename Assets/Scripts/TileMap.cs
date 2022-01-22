using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    [System.Serializable]
    public class SpawnLocation
    {
        public PlayerStats ForPlayer;
        public int StartingArmySize = 10;
        public int xPosition; // TODO: use Vector2Int
        public int yPosition;
    }

    public Tile TilePrefab;
    public int width = 3;
    public int height = 4;
    public Tile[,] Tiles;
    public List<SpawnLocation> SpawnLocations;

    void Awake()
    {
        createTiles();

        // spawn players
        Tile tile;
        HashSet<Tile> startingTiles = new HashSet<Tile>(); // HashSet used in hopes of duplicates automatically being removed
        foreach (var spawn in SpawnLocations)
        {
            tile = TileAt(spawn.xPosition, spawn.yPosition);
            tile.DefendAdd(spawn.ForPlayer, spawn.StartingArmySize);
            startingTiles.Add(tile);
        }

        // resolve which players were able to spawn
        foreach (var startingTile in startingTiles)
        {
            startingTile.DefendResolve();
        }
    }

    // void Update() { }

    /// <summary> Check all tiles on the map to see if they have any attacks that need to be resolved. Note, this should only be done once per tick. </summary>
    public void ResolveAttacks()
    {
        foreach (var tile in Tiles)
        {
            tile.DefendResolve();
        }
    }

    private void createTiles()
    {
        Tiles = new Tile[width, height];
        //        walkable = new bool[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Tiles[x, y] = Instantiate(TilePrefab, this.transform, false);
                Tiles[x, y].name = "Tile" + x + "-" + y;
                Tiles[x, y].SetPosition(x, y);
                //                walkable[x, y] = true;
            }
        }

        // link tiles to their neighbors
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Tiles[x, y].Neighbor.North = TileAt(x, y + 1);
                Tiles[x, y].Neighbor.South = TileAt(x, y - 1);
                Tiles[x, y].Neighbor.East = TileAt(x + 1, y);
                Tiles[x, y].Neighbor.West = TileAt(x - 1, y);
            }
        }
    }

    private Tile TileAt(int x, int y)
    {
        if (x < 0 || x >= width
        || y < 0 || y >= height)
        {
            return null;
        }
        else
        {
            return Tiles[x, y];
        }
    }
}
