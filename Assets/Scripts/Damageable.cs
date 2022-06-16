using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EntityTypes{
    Module=0,
    Defence=1,
    Turret=2,
    Player=3,
    Enemy=4,
    Other=5
}

public class Damageable : MonoBehaviour{
    public float health;
    public float maxHealth=50;
    public float regen;
    public float timeSinceDmg=0;
    public DebuffName appliedDebuffs;
    public EntityTypes type=EntityTypes.Other;
    public List<Debuff> debuffs = new List<Debuff>();
    public bool regening=false;

    protected virtual void Start(){
        regen=maxHealth/10;
        health=maxHealth;
    }

    ///<summary>
    ///the versions of this are, one where the debuff is specified by a type and another where its specified by an instance
    ///both have versions where you can specifiy the number of stacks
    ///in ApplyDebuff(newDebuff) it assumes the stacks is newDebuff.stacks
    ///</summary>
    public void ApplyDebuff<D>() where D : Debuff, new(){
        ApplyDebuff<D>(1);
    }

    public void ApplyDebuff<D>(int stacks) where D : Debuff, new(){
        D newDebuff = new D();
        newDebuff.stacks=stacks;
        ApplyDebuff(newDebuff);
    }
    public void ApplyDebuff(Debuff newDebuff){
        //do i have the debuff
        Debuff foundDebuff = FindDebuff(newDebuff.name);
        if(foundDebuff!=null){
            //apply stack
            foundDebuff.stacks+=newDebuff.stacks;
            foundDebuff.timeLeft=newDebuff.timeLeft;
            foundDebuff.onHit();
            return;
        }
        //add debuff
        appliedDebuffs|=newDebuff.name;
        newDebuff.perent=this;
        newDebuff.onHit();
        debuffs.Add(newDebuff);
    }
    public void ApplyDebuff(Debuff newDebuff,int stacks) {
        newDebuff.stacks=stacks;
        ApplyDebuff(newDebuff);
    }

    public Debuff FindDebuff(DebuffName dn){
        if((appliedDebuffs & dn)!=0){
            foreach(Debuff d in debuffs){
                if(d.name==dn){
                    return d;
                }
            }
        }
        return null;

    }

    public virtual void Die(){
        switch(type){
            case EntityTypes.Turret:
                Module m = (Module)GetComponent<IsItem>().item;
                List<Damageable> nearbyTurrets = m.GetNearby(transform.position);
                foreach (Damageable turret in nearbyTurrets){
                    m.onRemove((Turret)turret);
                }
                break;
            default:
                break;
        }

        if((int)type<3){
            int[] p = World.WorldPos(gameObject.transform.position);
            World.RemoveConstruct(p[0],p[1]);
        }
        GameObject.Destroy(gameObject);
    }

    protected virtual void RunDebuffs(){
        int i=0;
        while(i<debuffs.Count){
            if(debuffs[i].onUpdate()){
                //debuff ran succesfully
                i++;
            }else{
                //debuff is over
                debuffs.RemoveAt(i);
            }
        }
    }

    public virtual void Regen(){
        if(health<maxHealth){
            if(timeSinceDmg>1){
                health+=regen*Time.deltaTime;
            }
            timeSinceDmg+=Time.deltaTime;
        }else{
            health=maxHealth;
        }
    }

    public virtual void FixedUpdate(){
        RunDebuffs();
        if(regening){
            Regen();
        }
    }

    public virtual void OnHit(float d){}

    public virtual void TakeDamage(float d,Combatant sender,Vector3 direction){
        if(d>health&&sender.type==EntityTypes.Enemy){
            ((EnemyFsm)sender).LevelUp();
        }
        TakeDamage(d);
    }

    public virtual void TakeDamage(float d){
        timeSinceDmg=0;
        health-=d;
        if(health<=0){
            health=0;
            Die();
        }
        OnHit(d);
    }

}
