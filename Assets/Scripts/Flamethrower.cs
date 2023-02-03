using UnityEngine;

namespace Modulo {
public class FlameAttack : RangedAttack {
    public override Vector3 AtFunc(GameObject o) {
        Vector3 v = AttackProjection(o);
        AtFunc(v);
        return v;
    }
    public override void AtFunc(Vector3 d) {
        ProcOnCollsion p =
            basicBullet(this, "assets/hook",
                        (attackProperties() & SpecialProperties.homing) != 0);
        p.gameObject.GetComponent<Rigidbody2D>().velocity =
            (d).normalized * shotSpeed();
        p.p = impact.Go(damage(), this);
    }

    public override void AddAttack(Combatant c) { c.AddAttack<BulletAttack>(); }

    public FlameAttack() : base() {
        range = 6;
        timerMax = 0.03f;
        procCoefficent = 1;
        dmg = .08f;
        impact = new FlameProc();
    }
}

public class FlameProc : Proc {
    public override void OnProc(Damageable d) {
        base.OnProc(d);
        d.TakeDamage(new DamageData { dmg = dmg, sender = perent.perent,
                                      direction = d.transform.position -
                                                  collider.position });
    }

    public FlameProc() {
        procCoefficent = 0.1f;
        chance = 1;
        dmgMultiplier = 1;
    }

    public override Proc newSelf() { return new FlameProc(); }
}
}
