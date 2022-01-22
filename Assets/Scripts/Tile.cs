using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary> Single tile cell on the map </summary>
public class Tile : MonoBehaviour
{
    private readonly Color darkenStep = new Color(.8f, .8f, .8f, 1); // Weight Icon is in front with brightest color, rest get progressively darker

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

    /// <summary> An attack pending on this tile </summary>
    public class Attack
    {
        [Tooltip("Who owns the attack")]
        public PlayerStats Owner;

        [Tooltip("Size of the attack")]
        public int Population;

        public Attack(PlayerStats owner, int armySize)
        {
            Owner = owner;
            Population = armySize;
        }
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

    [Tooltip("Population sent to this tile from another tile with the same owner.")]
    public int TileReinforcements;

    public TileWeight Weight;

    /// <summary> All attacks batched and pending to be resolved on this tile </summary>
    private List<Attack> pendingAttacks = new List<Attack>();
    private RectTransform rectTrans;

    void Awake()
    {
        rectTrans = this.transform as RectTransform;
    }

    void Start()
    {
        // Set population text for neutral tiles. non-neutral tiles are handled by TileMap.cs flagging them in Awake(), then Player.cs's first doTicks()
        if (OwnedByPlayer == null)
        {
            RefreshIconPopulation();
        }
    }

    // void Update() { }

    void OnValidate()
    {
        Weight.UpdateWeight(OwnedByPlayer, this);
    }

    public void DefendAdd(PlayerStats attacker, int armySize)
    {
        if (attacker != OwnedByPlayer)
        {
            pendingAttacks.Add(new Attack(attacker, armySize));
        }
    }

    public void DefendResolve()
    {
        // Add reinforcements newly added from neighboring tiles to this tile
        this.TilePopulation += this.TileReinforcements;
        this.TileReinforcements = 0;

        // nothing to defend against
        if (pendingAttacks == null || pendingAttacks.Count == 0)
        {
            return;
        }

        // remove biggestAttacker from batchedAttacks
        pendingAttacks = pendingAttacks.GroupBy(a => a.Owner)
            .Select(group => new Attack(group.Key, group.Sum(row => row.Population))) // add together attacks coming from the same owner
            .ToList();
        int maxAttack = pendingAttacks.Max(a => a.Population);
        var biggestAttacker = pendingAttacks.First(a => a.Population == maxAttack);
        pendingAttacks.Remove(biggestAttacker);

        // All attackers evenly attack one another. The only troop count that matters is that of the biggestAttacker.
        // Example: if FinalAttacker = 100, Second = 80, Third = 1 then AttackSize = 100 - 80/2 - 1/2 = 100 - 40 - 0 = 60
        // Example: if FinalAttacker = 100, Second = 98, Third = 98 then AttackSize = 100 - 49 - 49 = 2
        foreach (var attack in pendingAttacks)
        {
            biggestAttacker.Population -= attack.Population / pendingAttacks.Count;
        }

        // What's left of the biggest attack is put up against this tiles defenses
        TilePopulation -= biggestAttacker.Population;
        if (TilePopulation < 0)
        {
            transferOwner(biggestAttacker.Owner);
        }

        // clean up
        pendingAttacks.Clear();
    }

    /// <summary> Transfers the ownership of a tile from its current owner to ownerNew </summary>
    private void transferOwner(PlayerStats ownerNew)
    {
        // remove old owner
        if (OwnedByPlayer != null)
        {
            OwnedByPlayer.OccupiedTiles.Remove(this);
        }

        // set tile ownership
        Color newColor = Color.white;
        OwnedByPlayer = ownerNew; // null value means the tile is now neutral
        if (ownerNew != null) // update player
        {
            ownerNew.OccupiedTiles.Add(this);
            newColor = ownerNew.Color;
        }
        TilePopulation *= -1; // convert army

        // Set UI colors
        Ui.IconWeight.color = newColor;
        Ui.IconPopulation.color = newColor * darkenStep;
        Ui.TileBackground.color = Ui.IconPopulation.color * darkenStep;
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
