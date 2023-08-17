using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modulo {
public enum ItemTypes { Module = 0, Defence = 1, Turret = 2, Orb = 3 }

public static class ItemHelperFunctions {
    public static void CardClickable(GameObject card, int number = 0) {
        Button bttn = card.AddComponent<Button>();
        bttn.onClick.AddListener(() => PlayerBehavior.TakeFromDeck(number));
        bttn.transition = Selectable.Transition.None;
    }
}

public class Item : ItemTemplate {
    public float power = 1;     // how many components went into it
    public float stability = 1; // the error in the ratio
                                // effects the falloff for modules

    public int StrengthModifier() {
        switch (type) {
        case ItemTypes.Turret:
            return 3;
        case ItemTypes.Defence:
            return 10;
        case ItemTypes.Module:
            return 1;
        }
        return -1;
    }

    public virtual GameObject ToGameObject(Vector3 p) {
        return this.ToGameObject<Damageable>(p);
    }

    public virtual GameObject ToGameObject<D>(Vector3 p)
        where D : Damageable {
        GameObject r = new GameObject(name);
        r.transform.position = p;
        D d = r.AddComponent<D>();
        d.type = (EntityTypes)type;
        SpriteRenderer sp = r.AddComponent<SpriteRenderer>();
        SpriteSize ss = GetSpriteSize();
        sp.sprite = ss.sprite;
        r.transform.localScale = ss.size;
        d.maxHealth = 10 * StrengthModifier();
        if (ItemTypes.Defence == type) {
            d.maxHealth *= power;
        }
        d.regen = d.maxHealth / 10;
        IsItem i = r.AddComponent<IsItem>();
        i.item = this;
        r.AddComponent<PolygonCollider2D>();
        r.layer = 3;

        Rigidbody2D rb = r.AddComponent<Rigidbody2D>();
        rb.bodyType=RigidbodyType2D.Static;

        if (type == ItemTypes.Orb) {
            d.maxHealth = 100;
        } else {
            World.UpdateState(HexCoord.NearestHex(p), NodeState.wall,
                              ChangeStateMethod.On);
        }
        return r;
    }
}
}
