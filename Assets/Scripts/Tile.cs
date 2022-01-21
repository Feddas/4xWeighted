using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Single tile cell on the map </summary>
public class Tile : MonoBehaviour
{
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
    public NesScripts.Controls.PathFind.Point Position { get; private set; }

    /// <summary> Access to all neighboring tiles </summary>
    public TileNeighbor Neighbor { get; private set; }

    public TileUi Ui;

    [Tooltip("Which player currently owns this tile")]
    public PlayerStats OwnedByPlayer;

    [Tooltip("Population on this tile last tick plus population created on this tile.")]
    public int TilePopulation = 0;

    [Tooltip("Population sent to this tile from another tile.")]
    public int TilePopulationAdded;

    public TileWeight Weight;

    private RectTransform rectTrans;

    void Awake()
    {
        rectTrans = this.transform as RectTransform;
    }

    void Start()
    {
        if (OwnedByPlayer == null) // TileMap.cs modifies this value by caling AttackWith
        {
            RefreshIconPopulation();
        }
    }

    // void Update() { }

    void OnValidate()
    {
        Weight.UpdateWeight(OwnedByPlayer, this);
    }

    public void Defend(PlayerStats attacker, int armySize)
    {
        TilePopulation -= armySize;
        if (TilePopulation >= 0) // attack was not strong enough to capture tile
        {
            return;
        }

        // remove old owner
        if (OwnedByPlayer != null)
        {
            OwnedByPlayer.OccupiedTiles.Remove(this);
        }

        // set new owner
        if (attacker != null)
        {
            // Set UI colors
            Ui.IconWeight.color = attacker.Color;
            Color darkenStep = new Color(.8f, .8f, .8f, 1); // Weight Icon is in front with brightest color, rest get progressively darker
            Ui.IconPopulation.color = Ui.IconWeight.color * darkenStep;
            Ui.TileBackground.color = Ui.IconPopulation.color * darkenStep;

            // Set tile ownership
            attacker.OccupiedTiles.Add(this);
            OwnedByPlayer = attacker;

            // convert army
            TilePopulation *= -1;
        }
    }

    /// <summary> Called when the tile Button component is clicked </summary>
    public void NextWeight()
    {
        NextWeight(Player.Manager.ClickingPlayer);
    }

    /// <summary> Allows NextWeight to be called by the non-Clicking players, such as AI </summary>
    public void NextWeight(PlayerStats player)
    {
        Weight.NextWeight(player, this);
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

    /// <summary> A different tile has called Weight.UpdateWeight(). This tile needs to show what fraction of the total weight is left for this tile. </summary>
    public void RefreshIconWeight(PlayerStats player)
    {
        Ui.IconWeight.fillAmount = (float)Weight.Current / player.TotalWeights;
    }

    /// <summary> Population calculation are finished, display the population of this tile </summary>
    public void RefreshIconPopulation()
    {
        // add units that were newly added from neighboring tiles to this tile
        this.TilePopulation += this.TilePopulationAdded;
        this.TilePopulationAdded = 0;

        // update Ui
        Ui.TextPopulation.text = TilePopulation.ToString();
        Ui.IconPopulation.fillAmount = OwnedByPlayer ?
            (float)TilePopulation / OwnedByPlayer.TotalPopulation // set percent of total population
            : 0; // this tile is unowned, neutral player doesn't have a TotalPopulation
    }

    public void SetPosition(int x, int y)
    {
        Position = new NesScripts.Controls.PathFind.Point(x, y);
        Neighbor = new TileNeighbor(Position);
        this.rectTrans.anchoredPosition = new Vector2(x * 100, y * 100);
    }
}
