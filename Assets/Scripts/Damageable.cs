using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour{
    public float health=100;
    public float maxHealth=100;
    public float regen=1;

    public void FixedUpdate(){
        health+=regen*Time.deltaTime;
    }

    public void TakeDamage(float d){
        health-=d;
        if(health<0){GameObject.Destroy(gameObject);}
    }
}

/// <summary>
/// this basically is a combatable entity that is still this is the basis for both a turret and an enemy
/// you use this to create classes that can fight and have attacks
/// </summary>
public class Combatant : Damageable{
    public List<Proc> procs = new List<Proc>();
    public List<Attack> attacks = new List<Attack>();
    public float dmgPlus=0;
    public float dmgMultipler=1;
    public float attackRate=1;
    public float attackSpeed=0;

    public void AddAttack<A>() where A:Attack,new(){
        Attack a = new A();
        attacks.Add(a);
        a.perent=this;
    }

    public void AddProc<P>() where P:Proc,new(){
        procs.Add(new P());
    }

    public void RunProc(float procCoefficent,Attack att,float dmg,GameObject hit){
        foreach(Proc p in procs){
			if(Random.value<procCoefficent*p.chance){
                (p.Go(dmg,att)).OnProc(hit);
            }
        }
    }

    public void FixedUpdate(){
        base.FixedUpdate();
        foreach (Attack a in attacks){
            a.Update();
        }
    }
}
