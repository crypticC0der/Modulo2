using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFsm: Combatant{
    float speed;
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
        health=1000000;
    }
}
