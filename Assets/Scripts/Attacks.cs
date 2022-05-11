using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum AttackType{
    Ranged,
    Close,
    Med,
    Area
}

public delegate void AtFunc(GameObject g);

/// <summary>
/// damager i barely know her
/// (lamo)
/// </summary>
public abstract class Damager{
    public float procCoefficent;
    public float dmg;
}

/// <summary>
/// this is basically a contained fsm that can deal damage and target enemies independantly
/// </summary>
public abstract class Attack : Damager{
    AttackType t;
    public Combatant perent;
    abstract public void Update();
    abstract public Collider2D Target();
    public AtFunc Hit;
    public float timerMax;
    protected float timer;
    public float range=0;
}

/// <summary>
/// basically this is a hook you can apply to an object woo
/// </summary>
public class Proc : Damager{
    public AtFunc p;
    public Attack perent;
}

/// <summary>
/// this allows you to add the attack to an object
/// </summary>
public class ProcOnCollsion: MonoBehaviour{
    public Proc p;
    public void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.layer != p.perent.perent.gameObject.layer){
            p.p(c.gameObject);
        }
    }

}

public class RangedAttack : Attack{
    public Proc impact;
    public override Collider2D Target(){
        Collider2D[] o = Physics2D.OverlapCircleAll(perent.transform.position,range);
        foreach(Collider2D c in o){
            if(Physics2D.Raycast(perent.transform.position,c.transform.position-perent.transform.position)
               && c.gameObject!=perent.gameObject){
               return c;
            }
        }
        return null;
    }
    public override void Update(){
        if(timer<=0){
            Collider2D c;
            if((c = Target()) != null){
                Hit(c.gameObject);
                timer=timerMax+perent.attackSpeed;
            }
        }
        timer-=Time.deltaTime*perent.attackRate;
    }
}

public class CloseAttack : Attack{
    public override Collider2D Target(){
        return Physics2D.OverlapCircle(perent.transform.position,range, ~(1<<6));
    }
    public override void Update(){
        if(timer<=0){
            Collider2D c;
            if(c = Target()){
                Hit(c.gameObject);
                timer=timerMax+perent.attackSpeed;
            }
        }
        timer-=Time.deltaTime*perent.attackRate;
    }
}
