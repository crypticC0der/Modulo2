using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsItem : MonoBehaviour{
    public Item item;
}

public enum Component{
    bladeParts,sparkon,soul,organic,lowDensityMetal,combustine
}

public class PlayerBehavior : Damageable{
    public static Deck deck;
    public static Item holding;
    public static UIControl controller;
    public static int[] components=new int[6];

    public override void OnHit(float d){
        base.OnHit(d);
        controller.bars[0].UpdateValue(health,maxHealth);
        components[(int)Component.organic]+=(int)(d/10);
    }

    public override void Regen(){
        base.Regen();
        controller.bars[0].UpdateValue(health,maxHealth);
    }

    public override void Die(){
        base.Die();
        World.clearGrid(false);
        controller.bars[0].SetValue(0,1);
    }

    protected override void Start(){
        maxHealth=200;
        base.Start();
        type=EntityTypes.Player;
        deck = new QueueDeck();
        controller = GetComponent<UIControl>();
        new ItemTemplate("thing",1,new float[]{0,0,0,0,0,0});
        for(int i =0;i<5;i++){
            AddToDeck(ItemRatio.table[1].item.FromTemplate(1,1));
        }
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
        p.z=0;
        GameObject o = holding.ToGameObject(World.NearestHex(p));
        holding=null;
    }
}
