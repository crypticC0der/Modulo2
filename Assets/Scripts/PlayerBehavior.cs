using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Modulo {

public class IsItem : MonoBehaviour {
    public Item item;
}

public class PlayerBehavior : Damageable,PicksComponent {
    Rigidbody2D r;
    public static PlayerBehavior me;

    public int[] componentCount = new int[4];
    public static Deck deck;
    public static Item holding;
    public static UIControl controller;

    public void CollectComponent(ComponentData data) {
        componentCount[data.id] += data.amount;
        Component.UpdateComponentUI((Component.Id)data.id,
                                    componentCount[data.id]);
    }

	public Vector3 Distance(Vector3 point){
        return transform.position - point;
    }

    public override void Regen() {
        if (move.healTime > 1) {
            base.Regen();
        }
    }

    public override void Die() {
        base.Die();
        if (World.orbTransform == transform) {
            World.stable = false;
        }
    }

    protected override void Start() {
        me = this;

        IEnumerator coroutine = World.MapGen();
        StartCoroutine(coroutine);
        // World.MapGen();

        maxHealth = 40;
        type = EntityTypes.Player;
        controller = GetComponent<UIControl>();
        b = controller.bars[0];
        base.Start();
        regen *= 4;
        deck = new StackDeck();
        ItemTemplate itemTemplate =
            new ItemTemplate("wallBase", 1, new float[] { 0, 0, 0, 0, 0, 0 });
        ItemTemplate orbT = new ItemTemplate(
            "orb", 5, new float[] { 0, 0, 0, 0, 0 }, "sacred", ItemTypes.Orb);
        for (int i = 0; i < 5; i++) {
            AddToDeck(itemTemplate.FromTemplate(1, 1));
        }
        AddToDeck(orbT.FromTemplate(0, 0));
        r = GetComponent<Rigidbody2D>();

        for(int i=0;i<4;i++){
            Component.UpdateComponentUI((Component.Id)i,
                                        componentCount[i]);
        }
    }

    public static void TakeFromDeck(int i) {
        if (holding == null) {
            holding = deck.Take(i);
            controller.Render();
        }
    }

    public static void AddToDeck() {
        if (holding != null) {
            deck.Add(holding);
            controller.Render();
            holding = null;
        }
    }

    public void AddToDeck(Item t) {
        if (t.type == ItemTypes.Orb) {
            World.orbTransform = transform;
            priority = Priority.Orb;
        }
        deck.Add(t);
        controller.Render();
    }

    public static void Place() {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // check if space is free
        if (!Physics2D.OverlapCircle(p, 0.3f, ((1 << 3)))) {
            // check if targets arent near
            if (!Physics2D.OverlapCircle(p, 1f, ((1 << 6) + (1 << 0)))) {
                p.z = 0;
                HexCoord hex = HexCoord.NearestHex(p);
                GameObject o = holding.ToGameObject(hex.position());
                if (holding.type == ItemTypes.Orb) {
                    World.orbTransform = o.transform;
                    PlayerBehavior.me.priority = Priority.Combatant;
                    World.SetOrb();
                }
                holding = null;
            }
        }
    }

    public override void FixedUpdate() {
        transform.eulerAngles = new Vector3(0, 0, World.VecToAngle(r.velocity));
        base.FixedUpdate();
    }

    void LateUpdate() { World.Run(); }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name == "orb") {
            AddToDeck(collision.gameObject.GetComponent<IsItem>().item);
            collision.gameObject.GetComponent<Bar>().Delete();
            Destroy(collision.gameObject);
        }
    }
}
}
