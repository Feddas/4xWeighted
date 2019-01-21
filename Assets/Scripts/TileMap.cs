using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    //    public static T[,] GetNew2DArray<T>(int x, int y, T initialValue)
    //    {
    //        T[,] nums = new T[x, y];
    //        for (int i = 0; i < x * y; i++) nums[i % x, i / x] = initialValue;
    //        return nums;
    //    }

    public Tile TilePrefab;
    public int width = 3;
    public int height = 4;
    public Tile[,] Tiles;

    //    private bool[,] walkable;// = GetNew2DArray(width, height, true);

    void Start()
    {
        createTiles();
        //        NesScripts.Controls.PathFind.Grid grid = new NesScripts.Controls.PathFind.Grid(walkable);
    }

    void Update() { }

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
