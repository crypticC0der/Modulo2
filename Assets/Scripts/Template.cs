using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TurretTemplate<A> : ItemTemplate where A:Attack,new(){
    public Stats baseStats;
    public List<Module> baseModules=new List<Module>();


    public TurretTemplate(string itemName,float itemWeight,Stats basicStats,float[] itemRatio,string pathToGraphic=null,ItemTypes itemType=ItemTypes.Turret,List<Module> basicModules=null,int itemScale=1) : base(itemName,itemWeight,itemRatio,pathToGraphic,itemType,itemScale){
        baseStats = basicStats;
        if(basicModules!=null){
            baseModules = basicModules;
        }
    }

    public TurretTemplate():base(){}
    public override Item FromTemplate(float p,float s){
        TurretItem<A> t = (TurretItem<A>)(base.FromTemplate(p,s,new TurretItem<A>()));
        t.baseStats = baseStats;
        t.baseModules=baseModules;
        return t;
    }
}

//you need a module template for every type of module
public abstract class ModuleTemplate : ItemTemplate {
    public ModuleType t;

    public ModuleTemplate(string itemName,float itemWeight,ModuleType type,float[] itemRatio,string pathToGraphic=null,int itemScale=1) : base(itemName,itemWeight,itemRatio,pathToGraphic,ItemTypes.Turret,itemScale){
        t=type;
    }

    public ModuleTemplate():base(){}
    public Item FromTemplate(float p,float s,Module m){
        Module mod = (Module)(base.FromTemplate(p,s,m));
        mod.t=t;
        return mod;
    }
}

public class StatModuleTemplate : ModuleTemplate{

    public Stats changes;
    public StatModuleTemplate(string itemName,float itemWeight,Stats statChanges,float[] itemRatio,string pathToGraphic=null,int itemScale=1) : base(itemName,itemWeight,ModuleType.uncommon,itemRatio,pathToGraphic,itemScale){
        changes=statChanges;
    }

    public StatModuleTemplate():base(){}
    public override Item FromTemplate(float p,float s){
        StatModule mod = (StatModule)(base.FromTemplate(p,s,new StatModule(changes)));
        return mod;
    }
}

public class DebuffModuleTemplate : ModuleTemplate{
    public Debuff d;

    public DebuffModuleTemplate():base(){}
    public DebuffModuleTemplate(string itemName,float itemWeight,Debuff debuff,float[] itemRatio,string pathToGraphic=null,int itemScale=1) : base(itemName,itemWeight,ModuleType.uncommon,itemRatio,pathToGraphic,itemScale){
        d=debuff;
    }
    public override Item FromTemplate(float p, float s)
    {
        DebuffModule mod = (DebuffModule)(base.FromTemplate(p,s,new DebuffModule(d)));
        return base.FromTemplate(p, s);
    }

}

public class ItemTemplate{
    public string name;
    public float weight;
    public string graphicPath;
    public ItemTypes type;
    public float scale=1;

    public ItemTemplate(string itemName,float itemWeight,float[] itemRatio,string pathToGraphic=null,ItemTypes itemType=ItemTypes.Defence,int itemScale=1){
        if(pathToGraphic==null){pathToGraphic=itemName;}
        name=itemName;
        weight=itemWeight;
        graphicPath=pathToGraphic;
        type=itemType;
        scale=itemScale;
        ItemRatio.table.Add(new ItemRatio{
                item=this,
                ratio=itemRatio
            });
    }

    public ItemTemplate(){}

    public virtual Item FromTemplate(float p,float s,Item t){
        t.name=name;
        t.weight=weight;
        t.graphicPath=graphicPath;
        t.type=type;
        t.power=p;
        t.stability=s;
        return t;
    }

    public virtual Item FromTemplate(float p,float s){
        Item t = new Item();
        return FromTemplate(p,s,t);
    }

    public void RenderCard(GameObject card){
        //do stuff
        GameObject ico = new GameObject("icon");
        ico.transform.SetParent(card.transform);
        Image img = ico.AddComponent<Image>();
        SpriteSize s = GetSpriteSize();
        img.sprite=s.sprite;
        img.transform.localScale=s.size;
        float widthRatio = card.GetComponent<RectTransform>().sizeDelta.x/100;
        ico.transform.localScale=card.transform.localScale*0.6f*widthRatio;
        ico.transform.localPosition=Vector3.zero;
    }
    public Sprite GetGraphic(){
        Sprite s =Resources.Load<Sprite>("cardSprites/" + graphicPath);
        if(s==null){
            s = Resources.Load<Sprite>("assets/"+graphicPath);
        }
        return s;
    }

    public SpriteSize GetSpriteSize(){
        SpriteSize r;
        r.sprite = GetGraphic();
        Vector2 v = r.sprite.bounds.size;
        r.size = new Vector3(scale/v.x,scale/v.y,1);
        r.size.y*=2f/Mathf.Sqrt(3);
        return r;
    }

    //redo
    static readonly int[] IngRatios={1,3,5,10,1,10};
    //TODO iterative process
    public static Item Closest(float[] ingredients){
        const int penalty=100;

        float s=0;
        for(int i=0;i<6;i++){
            s+=ingredients[i];
        }
        ItemTemplate r=null;float d=0;
        float sum=0;
        float stability=0;

        //aim is to minimise dist
        //sum is ingredients x ing ratio
        //distance is sqrt(sum(dist^2 * ratio)) (the manhattan distance if each ingredient corresponded to a axis on a 6 dimentional plane)
        //stability is sqrt(sum(dist^2 * ratio ^2)) the distance with extra fun stuff
        foreach (ItemRatio ir in ItemRatio.table){
            float currentDist=0;
            float currentSum=0;
            float currentStability=0;
            for(int i=0;i<6;i++){
                float dIng = ingredients[i]-ir.ratio[i]*s;
                if(ingredients[i]==0 && ir.ratio[i] != 0){dIng=Mathf.Max(dIng,penalty);}
                currentStability+=dIng*dIng*IngRatios[i]; //TODO these two lines will need to be balanced iterativley
                currentSum+=ingredients[i]*IngRatios[i];
                currentDist+=dIng*dIng*IngRatios[i]*IngRatios[i];
            }
            if(currentDist<d||r==null){
                d=currentDist;
                r=ir.item;
                sum=currentSum;
                stability=currentStability;
            }
        }
        return r.FromTemplate(sum,stability);
    }
}
