using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public class TileUi
    {
        public UnityEngine.UI.Text TextPopulation;
        public UnityEngine.UI.Image TileBackground;
        public UnityEngine.UI.Image IconWeight;
        public UnityEngine.UI.Image IconPopulation;
    }

    public NesScripts.Controls.PathFind.Point Position { get; private set; }
    public TileNeighbor Neighbor { get; private set; }

    public TileUi Ui;

    public PlayerStats OwnedByPlayer;

    [Tooltip("Population on this tile last tick plus population created on this tile.")]
    public int TilePopulation = 0;

    [Tooltip("Population on this tile last tick plus population created on this tile.")]
    public int TilePopulationAdded;

    [Range(0, 2)]
    public int TileWeight;

    private int lastWeight;
    private RectTransform rectTrans;

    void Awake()
    {
        rectTrans = this.transform as RectTransform;
    }
    void Start()
    {
        if (OwnedByPlayer != null)
        {
            // Weight Icon is in front with brightest color, rest get progressively darker
            Color darkenStep = new Color(.8f, .8f, .8f, 1);
            Ui.IconWeight.color = OwnedByPlayer.Color;
            Ui.IconPopulation.color = Ui.IconWeight.color * darkenStep;
            Ui.TileBackground.color = Ui.IconPopulation.color * darkenStep;

            OwnedByPlayer.OccupiedTiles.Add(this);
        }
    }

    void Update() { }

    void OnValidate()
    {
        UpdateWeight();
    }

    public void NextWeight()
    {
        // TODO: #1 apply weight to unoccupied tile. Needs a way to pass PlayerIndex

        TileWeight = (TileWeight + 1) % 3;
        UpdateWeight();
    }

    public void Tick()
    {
        if (OwnedByPlayer == null)
        {
            return;
        }

        TilePopulation++;
    }

    public void UpdateWeight()
    {
        if (OwnedByPlayer == null || lastWeight == TileWeight)
        {
            return;
        }

        // update which tiles have a weight icon
        if (TileWeight == 0)
        {
            Ui.IconWeight.fillAmount = 0;
            OwnedByPlayer.WeightedTiles.Remove(this);
        }
        else if (lastWeight == 0)
        {
            OwnedByPlayer.WeightedTiles.Add(this);
        }

        // change TotalWeights
        OwnedByPlayer.TotalWeights -= lastWeight;
        lastWeight = TileWeight;
        OwnedByPlayer.TotalWeights += TileWeight;

        // update all weighted icons
        foreach (var tile in OwnedByPlayer.WeightedTiles)
        {
            tile.RefreshIconWeight();
        }
    }

    public void RefreshIconWeight()
    {
        Ui.IconWeight.fillAmount = (float)TileWeight / OwnedByPlayer.TotalWeights;
    }

    public void RefreshIconPopulation()
    {
        // add newly added units to this tile
        this.TilePopulation += this.TilePopulationAdded;
        this.TilePopulationAdded = 0;

        // update Ui
        Ui.TextPopulation.text = TilePopulation.ToString();
        Ui.IconPopulation.fillAmount = (float)TilePopulation / OwnedByPlayer.TotalPopulation;
    }

    public void SetPosition(int x, int y)
    {
        Position = new NesScripts.Controls.PathFind.Point(x, y);
        Neighbor = new TileNeighbor(Position);
        this.rectTrans.anchoredPosition = new Vector2(x * 100, y * 100);
    }
}
