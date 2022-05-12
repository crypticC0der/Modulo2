using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFsm: Combatant{
    float speed;
}

public class SpinHit : CloseAttack{
    public override void AtFunc(GameObject g){
        Debug.Log("kys");
    }
    public SpinHit() : base(){
        range=3;
        timerMax=1;
    }
}

public class SpinWheel : EnemyFsm{
    public void Start(){
        AddAttack<SpinHit>();
    }
}
