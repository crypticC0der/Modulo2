using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IsItem : MonoBehaviour{
    public Item item;
}

public enum Component{
    bladeParts,sparkon,soul,organic,lowDensityMetal,combustine
}

public class ComponentData : MonoBehaviour{
    public Component component;
    public float time=0.5f;
    public int amount;

    private void FixedUpdate(){
        time-=Time.deltaTime;
        if(time<0&&PlayerBehavior.me!=null){
            Vector3 d = PlayerBehavior.me.transform.position-transform.position;
            if(d.magnitude>0.5f){
                transform.position+=Force(d)*Time.deltaTime;
            }else{
                PlayerBehavior.me.AddToComponents(component,amount);
                Destroy(gameObject);
            }
        }
    }

    public Vector3 Force(Vector3 d){
        float f = 30/(d.magnitude*d.magnitude+5);
        return d.normalized*f;
    }

}

public class PlayerBehavior : Damageable{
    Rigidbody2D r;
    public static PlayerBehavior me;
    public static string[] componentNames = new string[]{"Blade Parts","Sparkon","Soul","Organic","Low Density Metal","Combustine"};
    public static Deck deck;
    public static Item holding;
    public static UIControl controller;
    private static int[] components=new int[6];
    public static Sprite[] componentSprites=new Sprite[6];
    [SerializeField]
    private TMP_Text [] textMeshes;

    public void AddToComponents(Component component, int amount){
        components[(int)component]+=amount;
        textMeshes[(int)component].text=components[(int)component].ToString();
    }

    public override void TakeDamage(DamageData d){
        int components = (int)(d.dmg/10);
        float remander = (d.dmg/10) - (float)components;
        if(Random.value<remander){
            components++;
        }
        Debug.Log(components);
        PlayerBehavior.SpawnComponent(Component.organic,components,transform.position);
        base.TakeDamage(d);
    }

    public static void SpawnComponent(Component c,int amount,Vector3 position){
        for(int e=3;e>=0;e--){
            int p = (int)Mathf.Pow(10,e);
            while(p<=amount){
                amount-=p;
                CreateComponent(c,p,position+new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f)),(e+1)/4f);
            }
        }
    }

    private static GameObject CreateComponent(Component c,int amount,Vector3 position,float size){
        GameObject o = new GameObject(componentNames[(int)c]);
        ComponentData d = o.AddComponent<ComponentData>();
        d.component=c;
        d.amount=amount;
        d.transform.localScale= new Vector3(size,size);
        d.transform.position=position;
        o.AddComponent<SpriteRenderer>().sprite=componentSprites[(int)c];
        return o;
    }

    public override void Regen(){
        base.Regen();
    }

    public override void Die(){
        base.Die();
        World.clearGrid(false);
    }

    protected override void Start(){
        me=this;
        for(int i=0;i<6;i++){
            componentSprites[i]=Resources.Load<Sprite>("assets/bloon");
        }
        maxHealth=200;
        type=EntityTypes.Player;
        controller = GetComponent<UIControl>();
        b=controller.bars[0];
        base.Start();
        deck = new StackDeck();
        ItemTemplate itemTemplate = new ItemTemplate("wallBase",1,new float[]{0,0,0,0,0,0});
        for(int i =0;i<5;i++){
            AddToDeck(itemTemplate.FromTemplate(1,1));
        }
        r=GetComponent<Rigidbody2D>();
    }

    public static void TakeFromDeck(int i){
        if(holding==null){
            holding = deck.Take(i);
            controller.Render();
        }
    }

    public static void AddToDeck(){
        if(holding!=null){
            deck.Add(holding);
            controller.Render();
            holding=null;
        }
    }

    public static void AddToDeck(Item t){
        deck.Add(t);
        controller.Render();
    }

    public static void Place(){
        Vector3 p= Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //check if space is free
        if(!Physics2D.OverlapCircle(p,0.5f,((1<<3)))){
            //check if targets arent near
            if(!Physics2D.OverlapCircle(p,1f,((1<<6) + (1<<0)))){
            p.z=0;
            GameObject o = holding.ToGameObject(World.NearestHex(p));
            holding=null;
            }
        }
    }

    public override void FixedUpdate(){
        if(r.velocity.y!=0){
            float angle;
            angle = -180*Mathf.Atan(r.velocity.x/r.velocity.y)/Mathf.PI;
            if(r.velocity.y<0){
               angle+=180;
            }
            transform.eulerAngles=new Vector3(0,0,angle);
        }
        base.FixedUpdate();
    }
}
