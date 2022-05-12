using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemTypes{
    Module,
    Defence,
    Turret
}

public enum DeckTypes{
    StackDeck,
    QueueDeck,
    ArrayDeck,
    HashDeck
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

    public Item FromTemplate(float p,float s,Item t){
        t.name=name;
        t.weight=weight;
        t.graphicPath=graphicPath;
        t.type=type;
        t.power=p;
        t.stability=s;
        return t;
    }

    public Item FromTemplate(float p,float s){
        Item t = new Item();
        return FromTemplate(p,s,t);
    }

    public void RenderCard(GameObject card){
        //do stuff
        GameObject ico = new GameObject("icon");
        ico.transform.SetParent(card.transform);
        Image img = ico.AddComponent<Image>();
        img.sprite=GetGraphic();
        float widthRatio = card.GetComponent<RectTransform>().sizeDelta.x/100;
        ico.transform.localScale=card.transform.localScale*0.6f*widthRatio;
        ico.transform.localPosition=Vector3.zero;
    }
    public Sprite GetGraphic(){return Resources.Load<Sprite>("cardSprites/" + graphicPath);}
    public SpriteSize GetSpriteSize(){
        SpriteSize r;
        r.sprite = GetGraphic();
        Vector2 v = scale * r.sprite.bounds.size;
        r.size = new Vector3(v.x,v.y,1);
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

    public GameObject ToGameObject(Vector3 p){return this.ToGameObject<Damageable>(p);}
    public GameObject ToGameObject<D>(Vector3 p) where D : Damageable{
        GameObject r =new GameObject(name);
        r.transform.position=p;
        D d = r.AddComponent<D>();
        SpriteRenderer sp = r.AddComponent<SpriteRenderer>();
        SpriteSize ss = GetSpriteSize();
        sp.sprite = ss.sprite;
        r.transform.localScale = ss.size;
        d.maxHealth = 50 *(level)*power*StrengthModifier();
        IsItem i = r.AddComponent<IsItem>();
        i.item=this;
        BoxCollider2D b =r.AddComponent<BoxCollider2D>();
        b.size = (Vector2)(ss.size);
        int[] wop = World.WorldPos(p);
        World.AddConstruct(wop[0],wop[1]);
        return r;
    }
}

//deck moment
public interface Deck{
    public Item Take(int i);
    public bool Add(Item t);
    public void Render(GameObject panel);
}

//oh yeah im calling inventories decks, fuck you
//this is not a homestuck referance
//die
public class StackDeck : Deck{
    private Stack<Item> data;

    public StackDeck(){
        data = new Stack<Item>();
    }

    public Item Take(int i){
        return data.Pop();
    }

    public bool Add(Item t){
        data.Push(t);
        return true;
    }

    public void Render(GameObject panel){
        if (data.Count>0){
            float minwidth=10;
            float offset= (minwidth*((float)data.Count-1))/-2;
            if(minwidth*data.Count+60>420)
            {
                minwidth = (420-60)/((float)data.Count);
                offset= (minwidth*((float)data.Count-1))/-2;
            }
            GameObject o=null;
            for (int i=0;i<data.Count;i++){
                o=  GameObject.Instantiate(Resources.Load<GameObject>("card"));
                o.transform.SetParent(panel.transform);
                o.transform.localScale = Vector3.one;
                o.transform.localPosition = new Vector3(offset+(i*minwidth),-7,0);
            }
            data.Peek().RenderCard(o);
            ItemHelperFunctions.CardClickable(o);
        }
    }
}

public class QueueDeck : Deck{
    private Queue<Item> data;

    public QueueDeck(){
        data = new Queue<Item>();
    }

    public Item Take(int i){
        return data.Dequeue();
    }

    public bool Add(Item t){
        data.Enqueue(t);
        return true;
    }

    public void Render(GameObject panel){
        if (data.Count>0){
            float minwidth=10;
            float offset= (minwidth*((float)data.Count-1))/2;
            if(minwidth*data.Count+60>420)
            {
                minwidth = (420-60)/((float)data.Count);
                offset= (minwidth*((float)data.Count-1))/2;
            }

            GameObject o=null;
            for (int i=0;i<data.Count;i++){
                o=  GameObject.Instantiate(Resources.Load<GameObject>("card"));
                o.transform.SetParent(panel.transform);
                o.transform.localScale = Vector3.one;
                o.transform.localPosition = new Vector3(offset-(i*minwidth),-7,0);
            }
            data.Peek().RenderCard(o);
            ItemHelperFunctions.CardClickable(o);
        }
    }
}

public class ArrayDeck : Deck{
    private const int count=10;
    private Item[] data;

    public ArrayDeck(){
        data = new Item[count];
    }

    public Item Take(int i){
        Item r=data[i];
        data[i] = null;
        return r;
    }

    public bool Add(Item t){
        int i=0;
        for(i=0;i<count;i++){
            if(data[i]== null){
                data[i] = t;
                return true;
            }
        }
        return false;
    }

    public void Render(GameObject panel){
        for (int i=0;i<count;i+=2){
            GameObject o=GameObject.Instantiate(Resources.Load<GameObject>("duoCard"));
            o.transform.SetParent(panel.transform);
            o.transform.localScale = Vector3.one;
            for (int j=0;j<2;j++){
                GameObject c = o.transform.GetChild(j).gameObject;
                if(data[i+j]!=null){
                    data[i+j].RenderCard(c);
                    ItemHelperFunctions.CardClickable(c,j+i);
                }
            }
            o.transform.localPosition=new Vector3(-100+ (25*(i)),-7,0);
        }
    }
}

public class HashDeck : Deck{
    private Item[] data;
    private const int count=30;

    public int Hash(Item t){
        int r=0;
        foreach(char c in t.name){
            r+=((int)(c)-64); //get alphabet value for the charicter a=1 b=2 ...
        }
        return (r % count); //fit it into the hash table
    }

    public HashDeck(){
        data = new Item[count];
    }

    /// <summary>
    /// the input i doesnt need to be hashed
    /// </summary>
    public Item Take(int i){
        Item r=data[i];
        data[i]=null;
        return r;
    }

    /// <summary>
    /// this doesnt use i, but it hashes t
    /// </summary>
    public bool Add(Item t){
        int hash = Hash(t);
        if(data[hash] == null){
            data[hash]=t;
            return true;
        }
        return false;
    }

    public void Render(GameObject panel){
        for (int i=0;i<count;i+=3){
            GameObject o=GameObject.Instantiate(Resources.Load<GameObject>("tricard"));
            o.transform.SetParent(panel.transform);
            o.transform.localScale = Vector3.one;
            for (int j=0;j<3;j++){
                GameObject c = o.transform.GetChild(j).gameObject;
                if(data[i+j]!=null){
                    data[i+j].RenderCard(c);
                    ItemHelperFunctions.CardClickable(c,i+j);
                }
            }
            o.transform.localPosition=new Vector3(-157.5f + (35*(i/3)),-7,0);
        }
    }
}

/*
** left out for balance reasons**
/// <summary>
/// i have no fucking idea if this works, this is some dumb fucking code<br/>
/// its like half as long as all the other decks combined<br/>
/// it also may be somewhat broken, balance wise and code wise
/// </summary>
public class TreeDeck : Deck{
    class TreeNode{
        public Item d;
        public TreeNode left;
        public TreeNode right;
        public TreeNode p;
        public TreeNode(TreeNode perent){
            p=perent;
        }
    }

    TreeNode head;
    int layer=0;
    List<int> progress;

    public TreeDeck(){
       head = new TreeNode(null);
       layer=0;
       progress=new List<int>();
       progress.Add(0);
    }

    TreeNode Traverse(int layer,int position){
        TreeNode r = head;
        int i=1<<(layer-1);
        for (int j=0;j<layer;j++){
            if ((position & i>>j) !=0){
                r=r.right;
            }else{r=r.left;}
        }
        return r;
    }

    /// <summary>
    /// the first 16 bits represent the position wheras the next 16 bits represent the layer
    /// yes this isnt optimal i know
    /// </summary>
    public Item Take(int i){
        int layer = i&(~65535);
        int position = i&(65535);
        return Traverse(layer,position).d;
    }

    public bool Add(int i,Item t){
        TreeNode n = Traverse(layer,progress[layer]);
        n.left=new TreeNode(n);
        n.right=new TreeNode(n);
        n.d=t;
        progress[layer]++;
        if(progress[layer]==1<<layer){
            layer++;
            if(progress.Count<=layer){
                progress.Add(0);
            }
        }
        return true;
    }
}
*/
