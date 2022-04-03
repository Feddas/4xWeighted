using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> Single tile cell on the map </summary>
[RequireComponent(typeof(RectTransform))]
public class TileStatus : MonoBehaviour
{
    public class TileNeighbor
    {
        public List<TileStatus> All { get; private set; }

        /// <summary> Neighboring tiles that this tile can move its population onto </summary>
        public TileStatus North;
        public TileStatus South;
        public TileStatus East;
        public TileStatus West;

        public TileNeighbor(TileStatus north, TileStatus south, TileStatus east, TileStatus west)
        {
            North = north;
            South = south;
            East = east;
            West = west;
            All = new List<TileStatus>() { North, South, East, West }
                .Where(t => t != null).ToList();
        }
    }

    [System.Serializable]
    public class TileUi
    {
        [Tooltip("Shows the exact population count of this tile.")]
        public UnityEngine.UI.Text TextPopulation;

        [Tooltip("A fraction of this image is shown to represent the percent of population this tile houses out of all population a single player owns.")]
        public UnityEngine.UI.Image IconPopulation;

        [Tooltip("Color is changed to show who owns this tile.")]
        public UnityEngine.UI.Image TileBackground;

        [Tooltip("A fraction of this image is shown to represent the percent of weight this tile uses out of all weights in use by a single player.")]
        public UnityEngine.UI.Image IconWeight;
    }

    /// <summary> Where this tile is on the map </summary>
    public PathFind.Point Position { get; private set; }

    /// <summary> Access to all neighboring tiles </summary>
    public TileNeighbor Neighbor { get; private set; }

    public TileUi Ui;

    [Tooltip("Which player currently owns this tile")]
    public PlayerStats OwnedByPlayer;

    [Tooltip("Population on this tile last tick plus population created on this tile.")]
    public int TilePopulation = 0;

    [Tooltip("Population sent to this tile from another tile with the same owner.")]
    public int TileReinforcements;

    public TileCombat Combat;

    // void Awake() { }
    // void Start() { }

    public void Initialize(int x, int y)
    {
        SetPosition(x, y);

        // Prepare to handle combat on this tile
        Combat = new TileCombat();

        // Set population text for neutral tiles. non-neutral tiles overwrite this by TileMap.cs attacking them in Awake(), then Player.cs's first doTicks() resolving combat
        if (OwnedByPlayer == null)
        {
            RefreshIconPopulation();
        }
    }

    // void Update() { }

    public void DefendAdd(PlayerStats attacker, int armySize)
    {
        Combat.AddAttack(OwnedByPlayer, attacker, armySize);
    }

    public void DefendResolve()
    {
        Combat.Resolve(this);
    }

    /// <summary> Called when the tile Button component is clicked </summary>
    public void NextWeight()
    {
        // determine who is clicking
        var clickingPlayer = Player.Manager.ClickingPlayer;

        // register the click
        var weight = TileWeight.Next(clickingPlayer, this);
        weight.UiUpdateClickingPlayer(clickingPlayer);
    }

    /// <summary> What this tile does on every Update tick </summary>
    public void Tick()
    {
        // Tiles only generate population when they become owned
        if (OwnedByPlayer == null)
        {
            return;
        }

        TilePopulation++;
    }

    /// <summary> Population calculation are finished, display the population of this tile </summary>
    public void RefreshIconPopulation()
    {
        // update Ui
        Ui.TextPopulation.text = TilePopulation.ToString();
        Ui.IconPopulation.fillAmount = OwnedByPlayer ?
            (float)TilePopulation / OwnedByPlayer.TotalPopulation // set percent of total population
            : 0; // this tile is unowned, neutral player doesn't have a TotalPopulation
    }

    public void SetPosition(int x, int y)
    {
        Position = new PathFind.Point(x, y);
        (this.transform as RectTransform).anchoredPosition = new Vector2(x * 100, y * 100);
    }

    public void SetNeighbors(TileStatus north, TileStatus south, TileStatus east, TileStatus west)
    {
        Neighbor = new TileNeighbor(north, south, east, west);
    }
}
