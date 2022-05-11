using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFsm: Combatant{
    float speed;
}

public class SpinWheel : EnemyFsm{
    public void Start(){
        CloseAttack spinHit = new CloseAttack();
        spinHit.Hit = (GameObject g) => {Debug.Log("kys");};
        spinHit.range=3;
        spinHit.timerMax=1;
        AddAttack(spinHit);
    }
}
