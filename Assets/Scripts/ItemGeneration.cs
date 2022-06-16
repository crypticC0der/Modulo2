using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class TurretCreation{
    [RuntimeInitializeOnLoadMethod]
    static void CreateProjectileTurret(){
        Debug.Log("gay sex");
        Stats s = new Stats();
        s.maxHealth=150;
        s.HpRegen=0;
        s.damage=0;
        s.dmgMultipler=1;
        s.attackRate=1;
        s.attackSpeed=0;
        s.range=1;

        new TurretTemplate<BulletAttack>(
            "gunTurret",5,s,new float[]{0,0,0,0,0,0},"twal",ItemTypes.Defence
        );

        s = new Stats();
        s.maxHealth=150;
        s.HpRegen=0;
        s.damage=0;
        s.dmgMultipler=1;
        s.attackRate=4;
        s.attackSpeed=0;
        s.range=1;
        s.peirce=3;
        new TurretTemplate<FlameAttack>(
            "flameTurret",5,s,new float[]{0,0,0,0,0,0},"twal",ItemTypes.Defence,new List<Module>{new DebuffModule(new Burning())}
        );

    }
}
