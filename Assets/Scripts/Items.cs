using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemTypes{
    Module=0,
    Defence=1,
    Turret=2
}

public static class ItemHelperFunctions{
    public static void CardClickable(GameObject card,int number=0){
        Button bttn = card.AddComponent<Button>();
        bttn.onClick.AddListener(() => PlayerBehavior.TakeFromDeck(number));
        bttn.transition=Selectable.Transition.None;
    }
}

public class ItemRatio{
    public static List<ItemRatio> table=new List<ItemRatio>();
    public ItemTemplate item;
    public float[] ratio;

    public void GenerateItems(){

    }
}

public class Item : ItemTemplate{
    public float power=1; //how many components went into it
    public float stability=1; //how likely it is to mutate
    public float level=1;

    public int StrengthModifier(){
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

    public virtual GameObject ToGameObject(Vector3 p){return this.ToGameObject<Damageable>(p);}
    public virtual GameObject ToGameObject<D>(Vector3 p) where D : Damageable{
        GameObject r =new GameObject(name);
        r.transform.position=p;
        D d = r.AddComponent<D>();
        d.type=(EntityTypes)type;
        SpriteRenderer sp = r.AddComponent<SpriteRenderer>();
        SpriteSize ss  = GetSpriteSize();
        sp.sprite = ss.sprite;
        r.transform.localScale=ss.size;
        d.maxHealth = 50 *(level)*power*StrengthModifier();
        d.regen=d.maxHealth/10;
        IsItem i = r.AddComponent<IsItem>();
        i.item=this;
        r.AddComponent<PolygonCollider2D>();
        int[] wop = World.WorldPos(p);
        World.ChangeState(wop[0],wop[1],NodeState.wall,false,World.ChangeStateMethod.On);
        r.layer=3;
        return r;
    }
}
