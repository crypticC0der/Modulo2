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
    public int strength; // a metric  of how strong the enmy
    public float speed=1;
    public float distance;
    public EnemySpawner perent;
    public int level;

    public override void RunAttacks()
    {
        foreach(Attack a in attacks){
            if(a.attackRange()>distance){
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

    public int LevelUp(){
        return ++level;
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
    }

    public override void Die(){
        //give components
        if((EnemyTypes.Boss|EnemyTypes.Strong & enemyType) != 0){
            PlayerBehavior.components[(int)Component.sparkon]+=strength;
        }
        if((EnemyTypes.Fast & enemyType) != 0){
            PlayerBehavior.components[(int)Component.lowDensityMetal]+=strength;
        }
        Debuff d = FindDebuff(DebuffName.Burning);
        if(d!=null){
            PlayerBehavior.components[(int)Component.combustine]+=d.stacks;
        }
        Vector3 dist = PlayerBehavior.controller.transform.position-transform.position;
        if(dist.magnitude<2){
            PlayerBehavior.components[(int)Component.soul]+=strength;
        }
        PlayerBehavior.components[(int)Component.bladeParts]+=strength;

        enemies--;
        //actually die
        base.Die();
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
    }

    public override void TakeDamage(float d,Combatant sender,Vector3 direction){
        if(Vector3.Dot(transform.up,direction.normalized)<0.2f){
            d/=2;
        }
        base.TakeDamage(d,sender,direction);
    }
}

public class QuaterfoilEnemy:EnemyFsm{

    protected override void Start(){
        maxHealth=50;
        base.Start();
        regening=true;
        regen=maxHealth/2;
    }

    public override void Regen(){
        health+=regen*Time.deltaTime;
        if(health>maxHealth*2&&enemies<420){
            health=maxHealth;
            if(perent!=null){
                EnemyFsm e = Clone(transform.position+new Vector3(0.35f,0.35f));
            }else{
                Debug.Log("no spawner defined");
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
        base.Start();
    }

    protected void SkipStart(){
        base.Start();
    }

    public override void TakeDamage(float d,Combatant sender,Vector3 direction){
        //TODO add visual
        if((int)(Vector3.Angle(direction,transform.up)+10)%(360/points) < 30){ //gets if its close to the funny points
            return;
        }
        TakeDamage(d);
    }
}

public class CrossEnemy : StarEnemy{
    protected override void Start(){
        maxHealth=50;
        points=4;
        attackRate=4;
        AddAttack<FlameAttack>();
        specialProperties|=SpecialProperties.crossShot;
        base.SkipStart();
    }
}

public class SquareEnemy : EnemyFsm{
    protected override void Start(){
        maxHealth=200;
        speed=0.5f;
        dmgMultipler*=2;
        base.Start();
    }
}

public class RectangleEnemy : SpinEnemyFsm{
    protected override void Start(){
        maxHealth=200;
        speed=1.5f;
        dmgMultipler*=2;
        base.Start();
    }
}

public class TriangleEnemy : SpinEnemyFsm{
    protected override void Start(){
        maxHealth=50;
        speed=3f;
        dmgMultipler*=2;
        base.Start();
    }
}

public class OctogonEnemy : EnemyFsm{
    protected override void Start(){
        maxHealth=800;
        dmgMultipler*=8;
        speed=0.25f;
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
}

public class CurvilinearEnemy : TriangleEnemy{
    public override void FixedUpdate(){
        Collider2D[] c = Physics2D.OverlapAreaAll(transform.position-transform.up*0.5f-transform.right*2,transform.position-transform.up*4.5f+transform.right*2,1<<gameObject.layer);
        for(int i=0;i<c.Length;i++){
            c[i].GetComponent<EnemyFsm>().speed*=2;
        }
        base.FixedUpdate();
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
        g.GetComponent<Damageable>().TakeDamage(20);
        return Vector3.zero;
    }
    public SpinHit() : base(){
        range=1.2f;
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
