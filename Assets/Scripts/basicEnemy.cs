using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyTypes{
    Strong=1,
    Fast=2,
}

public class EnemyFsm: Combatant{
    protected EnemyTypes enemyType;
    public int strength;
    float speed;

    protected override void Start(){
        base.Start();
        type=EntityTypes.Enemy;
    }

    public override void Die(){
        //give components
        if((EnemyTypes.Strong & enemyType) != 0){
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

        //actually die
        base.Die();
    }

}

public class SpinHit : CloseAttack{
    public override void AtFunc(GameObject g){
        g.GetComponent<Damageable>().TakeDamage(500);
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
