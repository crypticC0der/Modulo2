using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class TurretCreation{
    [RuntimeInitializeOnLoadMethod]
    static void CreateProjectileTurret(){
        HealthBar.barSprites = new Sprite[]{Resources.Load<Sprite>("assets/barOutlineB"),Resources.Load<Sprite>("assets/barInner")};
        Stats s = new Stats();
        s.maxHealth=150;
        s.HpRegen=0;
        s.damage=0;
        s.dmgMultipler=1;
        s.attackRate=1;
        s.attackSpeed=0;
        s.range=1;
        s.shotSpeed=1;

        new TurretTemplate<BulletAttack>(
            "gunTurret",5,s,new float[]{0,0,0,0,0,0},"gunBase",ItemTypes.Defence
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
        s.shotSpeed=1;
        new TurretTemplate<FlameAttack>(
            "flameTurret",5,s,new float[]{0,0,0,0,0,0},"flameBase",ItemTypes.Defence,new List<Module>{new DebuffModule(new Burning())}
        );

        s = new Stats();
        s.maxHealth=150;
        s.HpRegen=0;
        s.damage=0;
        s.dmgMultipler=1;
        s.attackRate=1;
        s.attackSpeed=0;
        s.range=1;
        s.shotSpeed=2;

        new TurretTemplate<RocketAttack>(
            "rocketTurret",5,s,new float[]{0,0,0,0,0,0},"rocketBase",ItemTypes.Defence
        );
    }
}
