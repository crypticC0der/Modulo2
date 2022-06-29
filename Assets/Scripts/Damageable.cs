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

public class HealthBar : MonoBehaviour,Bar{
    public SpriteRenderer backdrop;
    public SpriteRenderer bar;
    public static Sprite[] barSprites;
    public static float[] intervals=new float[]{0.3f,0.4f};
    public static Color[] colors=new Color[]{new Color(12/16f,3/16f,1/32f),new Color(3/16f,13/16f,2/16f),new Color(13/16f,17/32f,1/8f),new Color(1/8f,1/8f,1/8f)};
    float current=1;
    float aim=1;
    float transition=0;
    public void Start(){
        backdrop= new GameObject("healthBar").AddComponent<SpriteRenderer>();
        backdrop.sprite = barSprites[0];
        backdrop.transform.localPosition=new Vector3(0,1);
        bar= new GameObject("health").AddComponent<SpriteRenderer>();
        bar.sprite = barSprites[1];
        bar.transform.SetParent(backdrop.transform);
        bar.transform.localPosition=new Vector3(0,0);
        backdrop.transform.localScale=Vector3.one*2;
        Enable(false);
        if(gameObject.layer==6){
            backdrop.color = colors[3];
        }
    }

    public void Reset(){UpdateValue(1,1);}
    public void Zero(){UpdateValue(0,1);}

    public void Enable(bool b){
        backdrop.enabled=b;
        bar.enabled=b;
    }

    public void Delete(){
        Destroy(bar.gameObject);
        Destroy(backdrop.gameObject);
    }

    public void UpdateValue(float current,float max){
        Enable(true);
        aim=current/max;
        transition=3;
    }

    public Color ColorAt(float v){
        float t = Mathf.InverseLerp(intervals[0],intervals[1],v);
        if(gameObject.layer==6){
            return Color.LerpUnclamped(colors[2],colors[0],t);
        }else{
            return Color.LerpUnclamped(colors[0],colors[1],t);
        }
    }

    public void SetValue(float current,float max){
        Vector3 s = bar.transform.localScale;
        float v = Mathf.Min(current/max,1);
        s.x=v;
        bar.color=ColorAt(v);
        Vector3 p = bar.transform.localPosition;
        p.x=(1-v)/2;
        bar.transform.localScale=s;
        bar.transform.localPosition=p;
    }

    public void Update(){
        backdrop.transform.position=transform.position+Vector3.up;
        if(transition>Time.deltaTime){
            current += (aim-current)/transition;
            SetValue(current,1);
        }else if(transition-Time.deltaTime< -0.5f && transition> -0.5f){
            current=aim;
            SetValue(aim,1);
            Enable(false);
        }
        transition-=Time.deltaTime;

    }

}

public class Damageable : MonoBehaviour{
    public float health;
    public float maxHealth=50;
    public float regen=0; //rate of regen
    public bool regening=false; //means it unconditionally regens
    public float timeSinceDmg=0;
    public DebuffName appliedDebuffs;
    public EntityTypes type=EntityTypes.Other;
    public List<Debuff> debuffs = new List<Debuff>();
    public Bar b;

    protected virtual void Start(){
        regen=maxHealth/10;
        health=maxHealth;
        if(type!=EntityTypes.Player){
            b = (Bar)gameObject.AddComponent<HealthBar>();
        }
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
        b.Delete();
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
            if(timeSinceDmg>1 || regening){
                health+=regen*Time.deltaTime;
                b.UpdateValue(health,maxHealth);
            }
            timeSinceDmg+=Time.deltaTime;
        }else if(health!=maxHealth){
            health=maxHealth;
            b.UpdateValue(1,1);
        }
    }

    public virtual void FixedUpdate(){
        RunDebuffs();
        if(regening||regen!=0){
            Regen();
        }
    }


    public virtual void TakeDamage(DamageData d){
        if(d.sender!=null){
            if(d.dmg>health&&d.sender.type==EntityTypes.Enemy){
                ((EnemyFsm)d.sender).LevelUp();
            }
        }
        timeSinceDmg=0;
        health-=d.dmg;
        if(health<=0){
            health=0;
            Die();
        }
        b.UpdateValue(health,maxHealth);
    }
}

public class DamageData{
    public float dmg;
    public Combatant sender=null;
    public Vector3 direction=Vector3.zero;
    public DamageProperties properties=0;
}

public enum DamageProperties{
    bypassArmor=1
}
