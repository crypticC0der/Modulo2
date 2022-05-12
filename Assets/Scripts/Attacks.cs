using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType{
    Ranged,
    Close,
    Med,
    Area
}


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
    abstract public void Update();
    abstract public Collider2D Target();
    abstract public void AtFunc(GameObject g);

    public AttackType t;
    public Combatant perent;
    public float timerMax;
    protected float timer;
    public float range=0;
}

/// <summary>
/// basically this is a hook you can apply to an object woo
/// dmg in this system is just a multiple
/// </summary>
abstract public class Proc : Damager{
    abstract public void OnProc(GameObject g);
    abstract public Proc Go(float dmg,Attack perent);
    public Attack perent;
    public float chance;
    public float dmgMultiplier;
}

/// <summary>
/// this allows you to add the attack to an object
/// </summary>
public class ProcOnCollsion: MonoBehaviour{
    public Proc p;
    public void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.layer != p.perent.perent.gameObject.layer){
            p.OnProc(c.gameObject);
        }
    }

}

public abstract class RangedAttack : Attack{
    public Proc impact;

    public RangedAttack(){
        t=AttackType.Ranged;
    }

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
                AtFunc(c.gameObject);
                timer=timerMax+perent.attackSpeed;
            }
        }
        timer-=Time.deltaTime*perent.attackRate;
    }
}

public abstract class CloseAttack : Attack{
    public CloseAttack(){
        t=AttackType.Close;
    }

    public override Collider2D Target(){
        return Physics2D.OverlapCircle(perent.transform.position,range, ~(1<<6));
    }
    public override void Update(){
        if(timer<=0){
            Collider2D c;
            if(c = Target()){
                AtFunc(c.gameObject);
                timer=timerMax+perent.attackSpeed;
            }
        }
        timer-=Time.deltaTime*perent.attackRate;
    }
}

public class BulletAttack : RangedAttack{
    public override void AtFunc(GameObject o){
        GameObject g = new GameObject();
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("assets/hook");
        g.AddComponent<CircleCollider2D>();
        ProcOnCollsion p = g.AddComponent<ProcOnCollsion>();
        p.p = impact.Go(dmg,this);
    }

    public BulletAttack() : base(){
        range=10;
        timerMax=1;
        procCoefficent=1;
        dmg=10;
        impact = new BulletProc();
    }
}

public class BulletProc : Proc{
    public override void OnProc(GameObject g){
        g.GetComponent<Damageable>().TakeDamage(perent.dmg*dmg);
    }


    public BulletProc(){
        procCoefficent=1;
        chance=1;
        dmgMultiplier=1;
    }

    public override Proc Go(float dmg,Attack perent){
        Proc p=new BulletProc();
        p.dmg=dmg;
        p.perent=perent;
        return p;
    }
}
