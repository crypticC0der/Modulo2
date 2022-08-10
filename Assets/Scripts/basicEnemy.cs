using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyTypes{
    Strong=1,
    Fast=2,
    Armored=4,
    WeakPoint=8,
    Regen=16,
    Boss=32,
}

public class EnemyFsm: Combatant{
    public static int enemies=0;
    protected EnemyTypes enemyType=0;
    public int strength=6; // a metric  of how strong the enmy
    public float speed=1;
    public float speedBonus=1;
    public float distance;
    public EnemySpawner perent;
    public int level=0;
    public int baseStrength;
    public Stats baseStats;

    public override void RunAttacks()
    {
        foreach(Attack a in attacks){
            if(a.attackRange()*8>distance){
                a.Update();
            }
        }
    }

    public EnemyFsm Clone(Vector3 p){
        return perent.Spawn(p,level);
    }
    public EnemyFsm Clone(){
        return perent.Spawn(transform.position,level);
    }

    public void LevelUpStats(){
        strength=baseStrength+level*baseStrength;
        speed=baseStats.speed+(level/10f)*baseStats.speed;
        maxHealth=baseStats.maxHealth+(level/5)*baseStats.maxHealth;
        dmgPlus=baseStats.damage+(level)*baseStats.damage;
        dmgMultipler=baseStats.dmgMultipler+(level/10)*baseStats.dmgMultipler;
        health=maxHealth;
    }

    public int LevelUp(int levels){
        level+=levels;
        LevelUpStats();
        return level;
    }
    public int LevelUp(){
        ++level;
        LevelUpStats();
        return level;
    }

    public override void RemoveStats(Stats changes){
        speed-=changes.speed;
        base.RemoveStats(changes);
    }

    public override void ApplyStats(Stats changes){
        speed+=changes.speed;
        base.ApplyStats(changes);
    }

    protected override void Start(){
        base.Start();
        regen=0;
        regening=false;
        type=EntityTypes.Enemy;
        enemies++;
        AddAttack<SpinHit>();
        baseStats=new Stats();
        baseStats.shotSpeed=shotSpeed;
        baseStats.damage=dmgPlus;
        baseStats.dmgMultipler=dmgMultipler;
        baseStats.range=range;
        baseStats.maxHealth=maxHealth;
        baseStats.attackSpeed=attackSpeed;
        baseStats.peirce=peirce;
        baseStats.attackRate=attackRate;
        baseStats.speed=speed;
        baseStrength=strength;
    }

    public override void Die(){
        //give components
        if((EnemyTypes.Boss|EnemyTypes.Strong & enemyType) != 0){
            PlayerBehavior.SpawnComponent(Component.sparkon,strength,transform.position);
        }
        if((EnemyTypes.Fast & enemyType) != 0){
            PlayerBehavior.SpawnComponent(Component.lowDensityMetal,strength,transform.position);
        }
        Debuff d = FindDebuff(DebuffName.Burning);
        if(d!=null){
            PlayerBehavior.SpawnComponent(Component.combustine,d.stacks,transform.position);
        }
        if(PlayerBehavior.controller!=null){
            Vector3 dist = PlayerBehavior.controller.transform.position-transform.position;
            if(dist.magnitude<2){
                PlayerBehavior.SpawnComponent(Component.soul,strength,transform.position);
            }
        }
        PlayerBehavior.SpawnComponent(Component.bladeParts,strength,transform.position);

        enemies--;
        //actually die
        base.Die();
    }

    public override void FixedUpdate(){
        speedBonus=1;
        base.FixedUpdate();
    }
}

public class SpinEnemyFsm:EnemyFsm{
    Rigidbody2D r;
    protected override void Start(){
        r=GetComponent<Rigidbody2D>();
        base.Start();
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

public class SemicircleEnemy: SpinEnemyFsm{
    protected override void Start(){
        maxHealth=50;
        base.Start();
        enemyType|=EnemyTypes.WeakPoint;
        strength=4;
    }

    public override void TakeDamage(DamageData d){
        if(d.direction!=Vector3.zero &&
           (d.properties&DamageProperties.bypassArmor)==0){
            if(Vector3.Dot(transform.up,d.direction.normalized)<0.2f){
                d.dmg/=2;
            }
        }
        base.TakeDamage(d);
    }
}

public class QuaterfoilEnemy:EnemyFsm{

    protected override void Start(){
        maxHealth=100; //2x regular health
        base.Start();
        health/=2;
        regening=true;
        regen=maxHealth/4;
        enemyType|=EnemyTypes.Regen;
        strength=4;
    }

    public override void Regen(){
        health+=regen*Time.deltaTime;
        if(health>=maxHealth&&enemies<420){
            health=maxHealth/2;
            if(perent!=null){
                EnemyFsm e = Clone(transform.position+new Vector3(0.35f,0.35f));
            }
            transform.position-= new Vector3(0.35f,0.35f);
        }
    }
}

public class StarEnemy : SpinEnemyFsm{
    public int points;
    protected override void Start(){
        maxHealth=100;
        points=5;
        enemyType|=EnemyTypes.WeakPoint;
        strength=7;
        base.Start();
    }

    protected void SkipStart(){
        base.Start();
    }

    public override void TakeDamage(DamageData d){
        //TODO add visual
        if(d.direction.x!=0&&d.direction.y!=0&&
           (d.properties&DamageProperties.bypassArmor)==0){
            float angle;
            angle = -180*Mathf.Atan(d.direction.x/d.direction.y)/Mathf.PI;
            if(d.direction.y<0){
            angle+=180;
            }
            if((int)(angle+10)%(360/points) < 20){ //gets if its close to the funny points
                return;
            }
        }
        base.TakeDamage(d);
    }
}

public class CrossEnemy : StarEnemy{
    protected override void Start(){
        maxHealth=50;
        points=4;
        attackRate=4;
        AddAttack<FlameAttack>();
        crossShots=4;
        // specialProperties|=SpecialProperties.crossShot;
        enemyType|=EnemyTypes.WeakPoint;
        strength=20;
        base.SkipStart();
    }
}

public class SquareEnemy : EnemyFsm{
    protected override void Start(){
        maxHealth=200;
        speed=0.5f;
        dmgMultipler*=2;
        strength=4;
        enemyType|=EnemyTypes.Strong;
        base.Start();
    }
}

public class RectangleEnemy : SpinEnemyFsm{
    protected override void Start(){
        maxHealth=200;
        speed=1.5f;
        dmgMultipler*=2;
        enemyType|=EnemyTypes.Strong;
        strength=6;
        base.Start();
    }
}

public class TriangleEnemy : SpinEnemyFsm{
    protected override void Start(){
        maxHealth=50;
        speed=3f;
        dmgMultipler*=2;
        enemyType|=EnemyTypes.Fast;
        strength=3;
        base.Start();
    }
}

public class OctogonEnemy : EnemyFsm{

    protected override void Start(){
        maxHealth=800;
        dmgMultipler*=8;
        speed=0.25f;
        enemyType|=EnemyTypes.Boss | EnemyTypes.Strong;
        strength=20;
        base.Start();
    }
}

public class DiamondEnemy : TriangleEnemy{
    public override void Die()
    {
        if(perent!=null){
            Vector3 offset = transform.up;
            for(int i=0;i<2;i++){
                EnemyFsm e = perent.Spawn(transform.position+offset/2,level,Shapes.triangle);
                e.transform.localScale=new Vector3(0.5f,0.5f);
                e.GetComponent<Rigidbody2D>().AddForce(offset,ForceMode2D.Impulse);
                offset*=-1;
            }
        }
        base.Die();
    }

    protected override void Start(){
        strength=6;
        base.Start();
    }
}

public class CurvilinearEnemy : TriangleEnemy{
    public override void FixedUpdate(){
        Collider2D[] c = Physics2D.OverlapAreaAll(transform.position-transform.up*0.5f-transform.right*2,transform.position-transform.up*4.5f+transform.right*2,1<<gameObject.layer);
        for(int i=0;i<c.Length;i++){
            c[i].GetComponent<EnemyFsm>().speedBonus=2;
        }
        base.FixedUpdate();
    }
    protected override void Start(){
        strength=6;
        base.Start();
    }
}


public class CircleEnemy : EnemyFsm{
    protected override void Start(){
        maxHealth=50;
        base.Start();
    }
}

public class SpinHit : CloseAttack{
    public override void AddAttack(Combatant c)
    {
        c.AddAttack<SpinHit>();
    }

    public override Vector3 AtFunc(GameObject g){
        g.GetComponent<Damageable>().TakeDamage(new DamageData{dmg=20,sender=perent,direction=g.transform.position-perent.transform.position});
        return Vector3.zero;
    }
    public SpinHit() : base(){
        range=.8f;
        timerMax=1;
    }
}

public class SpinWheel : EnemyFsm{
    public void Start(){
        AddAttack<SpinHit>();
        health=200;
        strength=6;
    }
}
