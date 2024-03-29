using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Modulo {
public class TurretTemplate<A> : ItemTemplate
    where A : Attack, new() {
    public Stats baseStats;
    public List<Module> baseModules = new List<Module>();

    public TurretTemplate(string itemName, float itemWeight, Stats basicStats,
                          Vector2 itemRatio, string pathToGraphic = null,
                          ItemTypes itemType = ItemTypes.Turret,
                          List<Module> basicModules = null, int itemScale = 1)
        : base(itemName, itemWeight, itemRatio, pathToGraphic, itemType,
               itemScale) {
        baseStats = basicStats;
        if (basicModules != null) {
            baseModules = basicModules;
        }
    }

    public TurretTemplate() : base() {}
    public override Item FromTemplate(float p, float s) {
        TurretItem<A> t =
            (TurretItem<A>)(base.FromTemplate(p, s, new TurretItem<A>()));
        t.baseStats = baseStats;
        t.baseModules = baseModules;
        return t;
    }
}

// you need a module template for every type of module
public abstract class ModuleTemplate : ItemTemplate {
    public ModuleType t;

    public ModuleTemplate(string itemName, float itemWeight, ModuleType type,
                          Vector2 itemRatio, string pathToGraphic = null,
                          int itemScale = 1)
        : base(itemName, itemWeight, itemRatio, pathToGraphic, ItemTypes.Turret,
               itemScale) {
        t = type;
    }

    public ModuleTemplate() : base() {}
    public Item FromTemplate(float p, float s, Module m) {
        Module mod = (Module)(base.FromTemplate(p, s, m));
        mod.t = t;
        return mod;
    }
}

public class StatModuleTemplate : ModuleTemplate {

    public Stats changes;
    public StatModuleTemplate(string itemName, float itemWeight,
                              Stats statChanges, Vector2 itemRatio,
                              string pathToGraphic = null, int itemScale = 1)
        : base(itemName, itemWeight, ModuleType.uncommon, itemRatio,
               pathToGraphic, itemScale) {
        changes = statChanges;
    }

    public StatModuleTemplate() : base() {}
    public override Item FromTemplate(float p, float s) {
        StatModule mod =
            (StatModule)(base.FromTemplate(p, s, new StatModule(changes)));
        return mod;
    }
}

public class DebuffModuleTemplate : ModuleTemplate {
    public Debuff d;

    public DebuffModuleTemplate() : base() {}
    public DebuffModuleTemplate(string itemName, float itemWeight,
                                Debuff debuff, Vector2 itemRatio,
                                string pathToGraphic = null, int itemScale = 1)
        : base(itemName, itemWeight, ModuleType.uncommon, itemRatio,
               pathToGraphic, itemScale) {
        d = debuff;
    }
    public override Item FromTemplate(float p, float s) {
        DebuffModule mod =
            (DebuffModule)(base.FromTemplate(p, s, new DebuffModule(d)));
        return base.FromTemplate(p, s);
    }
}

public class ItemTemplate {
    public string name;
    public float weight;
    public string graphicPath;
    public ItemTypes type;
    public float scale = 1;

    public ItemTemplate(string itemName, float itemWeight, Vector2 itemRatio,
                        string pathToGraphic = null,
                        ItemTypes itemType = ItemTypes.Defence,
                        int itemScale = 1) {
        if (pathToGraphic == null) {
            pathToGraphic = itemName;
        }
        name = itemName;
        weight = itemWeight;
        graphicPath = pathToGraphic;
        type = itemType;
        scale = itemScale;
        if (itemType != ItemTypes.Orb) {
            ItemRatio.table.Add(
                new ItemRatio { item = this, ratio = itemRatio });
        }
    }

    public ItemTemplate() {}

    public virtual Item FromTemplate(float p, float s, Item t) {
        t.name = name;
        t.weight = weight;
        t.graphicPath = graphicPath;
        t.type = type;
        t.power = p;
        t.stability = s;
        return t;
    }

    public virtual Item FromTemplate(float p, float s) {
        Item t = new Item();
        return FromTemplate(p, s, t);
    }

    public void RenderCard(GameObject card) {
        // do stuff
        GameObject ico = new GameObject("icon");
        ico.transform.SetParent(card.transform);
        Image img = ico.AddComponent<Image>();
        SpriteSize s = GetSpriteSize();
        img.sprite = s.sprite;
        img.transform.localScale = s.size;
        float widthRatio = card.GetComponent<RectTransform>().sizeDelta.x / 100;
        ico.transform.localScale =
            card.transform.localScale * 0.6f * widthRatio;
        ico.transform.localPosition = Vector3.zero;
    }

    public static Sprite GetGraphic(string graphicPath) {
        Sprite s = Resources.Load<Sprite>("cardSprites/" + graphicPath);
        if (s == null) {
            s = Resources.Load<Sprite>("assets/" + graphicPath);
        }
        return s;
    }

    public Sprite GetGraphic() {
        return GetGraphic(graphicPath);
    }

    public static SpriteSize GetSpriteSize(string graphicPath,float scale=1) {
        SpriteSize r;
        r.sprite = GetGraphic(graphicPath);
        Vector2 v = r.sprite.bounds.size;
        r.size = new Vector3(scale / v.x, scale / v.y, 1);
        r.size.y *= 2f / Mathf.Sqrt(3);
        return r;
    }

    public SpriteSize GetSpriteSize() {
        return GetSpriteSize(graphicPath,scale);
    }

    public static Item Closest(Vector2 ratio) {
        float distance=99999999999999;
        return null;
    }
}
}
