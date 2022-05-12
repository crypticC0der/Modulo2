using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class TurretCreation{
    [RuntimeInitializeOnLoadMethod]
    static void CreateProjectileTurret(){
        Stats s = new Stats();
        s.maxHealth=150;
        s.HpRegen=0;
        s.damage=50;
        s.dmgMultipler=1;
        s.attackRate=1;
        s.attackSpeed=0;
        s.range=1;

        TurretTemplate<BulletAttack> tt = new TurretTemplate<BulletAttack>(
            "gunTurret",5,s,new float[]{0,0,0,0,0,0},"twal",ItemTypes.Defence
        );
    }
}
