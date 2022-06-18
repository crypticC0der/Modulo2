using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Stats{
    public float maxHealth;
    public float HpRegen;
    public float damage;
    public float dmgMultipler;
    public float attackRate;
    public float attackSpeed;
    public float shotSpeed;
    public float range;
    public float speed;
    public int peirce;
}

public class Turret : Combatant{
    public new void Die(){
        base.Die();
        Effects.Explode(transform.position,1);
    }
}

public class TurretItem<A> : Item where A:Attack,new(){
    public Stats baseStats;
    public List<Module> baseModules;

    public override GameObject ToGameObject(Vector3 p){
        GameObject o = base.ToGameObject<Turret>(p);
        Turret t = o.GetComponent<Turret>();
        t.AddAttack<A>();
        t.maxHealth=baseStats.maxHealth;
        t.regen=baseStats.HpRegen;
        t.dmgPlus=baseStats.damage;
        t.dmgMultipler=baseStats.dmgMultipler;
        t.attackSpeed=baseStats.attackSpeed;
        t.attackRate=baseStats.attackRate;
        t.shotSpeed=baseStats.shotSpeed;
        t.range =baseStats.range;
        t.peirce=baseStats.peirce;
        foreach(Module m in baseModules){
            m.onApply(t);
        }
        return o;
    }
}
