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
    public float range;
}

public enum ModuleType{
    uncommon,
	exotic,
	rare,
	epic
};

public abstract class Module : Item{
    ModuleType t;
    Vector2 s;
    public abstract void onApply(Turret perent);
    public abstract void onRemove(Turret perent);
}

public class StatModule : Module{
    public Stats changes;

    public override void onApply(Turret perent){
        perent.maxHealth+=changes.maxHealth;
        perent.regen+=changes.HpRegen;
        perent.dmgPlus+=changes.damage;
        perent.dmgMultipler+=changes.dmgMultipler;
        perent.attackSpeed+=changes.attackSpeed;
        perent.attackRate+=changes.attackRate;
    }

    public override void onRemove(Turret perent){
        perent.maxHealth-=changes.maxHealth;
        perent.regen-=changes.HpRegen;
        perent.dmgPlus-=changes.damage;
        perent.dmgMultipler-=changes.dmgMultipler;
        perent.attackSpeed-=changes.attackSpeed;
        perent.attackRate-=changes.attackRate;
    }

    public StatModule(Stats s){
        changes = s;
    }
}

public class Turret : Combatant{
}

public class TurretItem : Item{
    public Attack baseAttack;
    public Stats baseStats;

    public GameObject ToGameObject(Vector3 p){
        GameObject o = base.ToGameObject<Turret>(p);
        Turret t = o.GetComponent<Turret>();
        t.AddAttack(baseAttack);
        t.maxHealth=baseStats.maxHealth;
        t.regen=baseStats.HpRegen;
        t.dmgPlus=baseStats.damage;
        t.dmgMultipler=baseStats.dmgMultipler;
        t.attackSpeed=baseStats.attackSpeed;
        t.attackRate=baseStats.attackRate;
        return o;
    }
}

public class TurretTemplate : ItemTemplate{
    public Attack baseAttack;
    public Stats baseStats;

    public TurretTemplate(string n,float w,Attack bA,Stats bS,float[] r,string p=null,ItemTypes t=ItemTypes.Turret,int s=1) : base(n,w,r,p,t,s){
        baseAttack = bA;
        baseStats = bS;
    }

    public TurretTemplate():base(){}
    public TurretItem FromTemplate(float p,float s){
        TurretItem t = (TurretItem)(base.FromTemplate(p,s,new TurretItem()));
        t.baseAttack=baseAttack;
        t.baseStats = baseStats;
        return t;
    }
}

public class ProjectileTurret : Turret{
    public ProjectileTurret(){
        RangedAttack r = new RangedAttack();
        r.range=100;
        r.timerMax=1;
        r.Hit = (GameObject o) => {
            GameObject
        };
    }
}
