using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo {
public struct Stats {
    public float maxHealth;
    public float HpRegen;
    public float damage;
    public float dmgMultipler;
    public float attackRate;
    public float attackSpeed;
    public float shotSpeed;
    public float range;
    public float speed;
    public int peirce;
    public int funnelShots;
    public int crossShots;

    public int nonZero() {
        // this is really shit im so sorry
        int nzs = 0;
        ApplyOverAll((ref float x) => {
            if (x != 0) {
                nzs++;
            }
        });
        return nzs;
    }

    delegate void OnStat(ref float stat);

    void ApplyOverAll(OnStat func) {
        func(ref maxHealth);
        func(ref HpRegen);
        func(ref damage);
        func(ref dmgMultipler);
        func(ref attackRate);
        func(ref attackSpeed);
        func(ref shotSpeed);
        func(ref range);
        func(ref speed);

        float tempVal = peirce;
        func(ref tempVal);
        peirce = (int)Mathf.Round(tempVal);

        tempVal = funnelShots;
        func(ref tempVal);
        funnelShots = (int)Mathf.Round(tempVal);

        tempVal = crossShots;
        func(ref tempVal);
        crossShots = (int)Mathf.Round(tempVal);
    }

    public void Multiply(float m, bool adjust = false) {
        if (adjust) {
            int nz = nonZero();
            if (nz > 0) {
                m = Mathf.Pow(m, 1 / nz);
            }
        }
        ApplyOverAll((ref float x) => { x *= m; });
    }
}

public class Turret : Combatant {
    public float stability;
    public new void Die() { base.Die();
    Effects.Explode(transform.position, 1, null, layerMask(false));
}

public new void Start() { base.Start();
// this.AddProc<PushOut>();
this.toApply.Add(new Slowed());
}
}

public class TurretItem<A> : Item
    where A : Attack, new() {
    public Stats baseStats;
    public List<Module> baseModules;

    public override GameObject ToGameObject(Vector3 p) {
        float multiplier = Mathf.Pow(power, 1 / 3);
        GameObject o = base.ToGameObject<Turret>(p);
        Turret t = o.GetComponent<Turret>();
        t.AddAttack<A>();
        t.maxHealth = baseStats.maxHealth;
        t.regen = baseStats.HpRegen;
        t.baseDamage = baseStats.damage;
        t.dmgMultipler = baseStats.dmgMultipler * multiplier;
        t.attackSpeed = baseStats.attackSpeed;
        t.attackRate = baseStats.attackRate * multiplier;
        t.shotSpeed = baseStats.shotSpeed;
        t.range = baseStats.range;
        t.peirce = baseStats.peirce;
        t.stability = stability;
        foreach (Module m in baseModules) {
            m.onApply(t, 1);
        }
        return o;
    }
}
}
