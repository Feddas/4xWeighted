using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.GridLayoutGroup))]
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

    public TileStatus TilePrefab;
    public int width = 3;
    public int height = 4;
    public TileStatus[,] Tiles;
    public List<SpawnLocation> SpawnLocations;

    private UnityEngine.UI.GridLayoutGroup gridLayout
    {
        get
        {
            if (_gridLayout == null)
            {
                _gridLayout = this.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            }
            return _gridLayout;
        }
    }
    private UnityEngine.UI.GridLayoutGroup _gridLayout;

    private Vector2 canvasSize
    {
        get
        {
            if (_canvasSize == null)
            {
                _canvasSize = this.GetComponentInParent<Canvas>().transform as RectTransform;
            }
            return _canvasSize.sizeDelta;
        }
    }
    private RectTransform _canvasSize;


    void Awake()
    {
        createTiles();

        // spawn players
        TileStatus tile;
        HashSet<TileStatus> startingTiles = new HashSet<TileStatus>(); // HashSet used in hopes of duplicates automatically being removed
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

    void Start()
    {
        gridLayout.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.LowerLeft;
        gridLayout.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Vertical;
        gridLayout.cellSize = new Vector2(canvasSize.x / width, canvasSize.y / height);
    }

    // void Update() { }

    public TileStatus TileAt(int x, int y)
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
        Tiles = new TileStatus[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Tiles[x, y] = Instantiate(TilePrefab, this.transform, false);
                Tiles[x, y].name = "Tile" + x + "-" + y;
                Tiles[x, y].Initialize(x, y);
            }
        }

        // link tiles to their neighbors
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Tiles[x, y].SetNeighbors
                (
                    north: TileAt(x, y + 1),
                    south: TileAt(x, y - 1),
                    east: TileAt(x + 1, y),
                    west: TileAt(x - 1, y)
                );
            }
        }
    }
}
