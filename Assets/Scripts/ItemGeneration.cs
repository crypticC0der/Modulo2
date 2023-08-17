using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo {
static class TurretCreation {
    [RuntimeInitializeOnLoadMethod]
    static void CreateProjectileTurret() {
        HealthBar.barSprites =
            new Sprite[] { Resources.Load<Sprite>("assets/barOutlineB"),
                           Resources.Load<Sprite>("assets/barInner") };
        Stats s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 1;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;

        new TurretTemplate<BulletAttack>("gunTurret", 5, s,
                                         new Vector2(0,0),
                                         "gunBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.peirce = 3;
        s.shotSpeed = 1;
        new TurretTemplate<FlameAttack>(
            "flameTurret", 5, s, new Vector2(0,0), "flameBase",
            ItemTypes.Turret,
            new List<Module> { new DebuffModule(new Burning()) });

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 2;

        new TurretTemplate<RocketAttack>("rocketTurret", 5, s,
                                         new Vector2(0,0),
                                         "rocketBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<LaserAttack>("laserTurret", 5, s,
                                        new Vector2(0,0),
                                        "laserBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<TeslaAttack>("teslaTurret", 5, s,
                                        new Vector2(0,0),
                                        "teslaBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<BeamAttack>("beamTurret", 5, s,
                                       new Vector2(0,0),
                                       "beamBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<GasAttack>("gasTurret", 5, s,
                                      new Vector2(0,0),
                                      "gasBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<SpikeAttack>("spikeTurret", 5, s,
                                        new Vector2(0,0),
                                        "spikeBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<ArrowAttack>("arrowTurret", 5, s,
                                        new Vector2(0,0),
                                        "areaBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<DaggerAttack>("daggerTurret", 5, s,
                                         new Vector2(0,0),
                                         "daggerBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<TentacleAttack>("tentacleTurret", 5, s,
                                           new Vector2(0,0),
                                           "tentacleBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<ArmAttack>("armAttack", 5, s,
                                      new Vector2(0,0),
                                      "armBase", ItemTypes.Turret);

        s = new Stats();
        s.maxHealth = 30;
        s.HpRegen = 0;
        s.damage = 10;
        s.dmgMultipler = 1;
        s.attackRate = 1;
        s.attackSpeed = 0;
        s.range = 1;
        s.shotSpeed = 1;
        new TurretTemplate<SaberAttack>("saberAttack", 5, s,
                                        new Vector2(0,0),
                                        "saberBase", ItemTypes.Turret);

        new ItemTemplate("wall", 5, new Vector2(0,0), "wallBase",
                         ItemTypes.Defence);
    }
}
}
