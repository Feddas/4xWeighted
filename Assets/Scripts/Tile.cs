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

    public TileUi Ui;

    /// <summary> Neighboring tiles that this tile can move it's population onto </summary>
    public Tile North;
    public Tile South;
    public Tile East;
    public Tile West;

    public PlayerStats OwnedByPlayer;

    [Tooltip("Population on this tile last tick plus population created on this tile.")]
    public int TilePopulation = 0;

    [Tooltip("Population on this tile last tick plus population created on this tile.")]
    public int TilePopulationAdded;

    [Range(0, 2)]
    public int TileWeight;

    private int lastWeight;
    private NesScripts.Controls.PathFind.Point position;
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

    public void TowardsWeighted()
    {
        if (OwnedByPlayer.WeightedTiles == null
        || OwnedByPlayer.WeightedTiles.Count == 0
        || OwnedByPlayer.WeightedTiles.Contains(this))
        {
            return;
        }

        if (this.position == OwnedByPlayer.WeightedTiles[0].position)
        {   // Don't move any population, we are a weighted tile
            return;
        }

        int xDiff = OwnedByPlayer.WeightedTiles[0].position.x - this.position.x;
        int yDiff = OwnedByPlayer.WeightedTiles[0].position.y - this.position.y;
        int xDiffAbs = Mathf.Abs(xDiff);
        int yDiffAbs = Mathf.Abs(yDiff);

        if (xDiffAbs == yDiffAbs) // send half both ways
        {
            moveTowards(sendHalf, xDiff, this.East, this.West);
            moveTowards(sendAll, yDiff, this.North, this.South);
        }
        else if (xDiffAbs > yDiffAbs) // send all along x
        {
            moveTowards(sendAll, xDiff, this.East, this.West);
        }
        else // send all along y
        {
            moveTowards(sendAll, yDiff, this.North, this.South);
        }
    }

    private void moveTowards(System.Action<Tile> sendAmount, int magnitude, Tile onPositve, Tile onNegative)
    {
        if (magnitude > 0)
        {
            sendAmount(onPositve);
        }
        else if (magnitude < 0)
        {
            sendAmount(onNegative);
        }
        else
        {
            throw new System.Exception("Messed up moveTowards " + magnitude + ", " + onPositve.name + ", " + onNegative);
        }
    }

    private void sendAll(Tile neighboringTile)
    {
        if (neighboringTile == null)
            return;

        neighboringTile.TilePopulationAdded += this.TilePopulation;
        this.TilePopulation = 0;
    }

    private void sendHalf(Tile neighboringTile)
    {
        if (neighboringTile == null)
            return;

        neighboringTile.TilePopulationAdded += this.TilePopulation / 2;
        this.TilePopulation -= this.TilePopulation / 2;
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
        position.Set(x, y);
        this.rectTrans.anchoredPosition = new Vector2(x * 100, y * 100);
    }
}
