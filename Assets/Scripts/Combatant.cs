using UnityEngine;
using System.Collections.Generic;

namespace Modulo {
[System.Serializable]
public enum SpecialProperties {
    none = 0,
    random = 1,
    homing = 2,
    predictive = 4,
    rapidFire = 8,
    returning = 16,
    spdUp = 32,
    polar = 64
}

/// <summary>
/// this basically is a combatable entity that is still this is the basis for
/// both a turret and an enemy you use this to create classes that can fight and
/// have attacks
/// </summary>
public class Combatant : Damageable {
    [SerializeReference]
    public List<Proc> procs = new List<Proc>();
    [SerializeReference]
    public List<Debuff> toApply = new List<Debuff>();
    [SerializeReference]
    public List<Attack> attacks = new List<Attack>();
    public float baseDamage = 10;
    public float dmgMultipler = 1;
    public float attackRate = 1;
    public float attackSpeed = 0;
    public float shotSpeed = 1; // only applicable with 2 things
    public float maxRange = 0;
    public float range = 1;
    public int peirce = 0;
    public int funnelShots = 1;
    public int crossShots = 1;
    public float damage() { return baseDamage * dmgMultipler; }
    public int totalShots() {
        int t = funnelShots * crossShots;
        if (t > 18) {
            return 18;
        } else {
            return t;
        }
    }
    public SpecialProperties specialProperties;

    // targeting works like so
    // player should only get hit in crossfire, or by enemies if theres nothing
    // more interesting if the player is holding the orb the enemies will very
    // much target the player though the only time this gets tricky is with area
    // and "melee" attacks with area attacks, they will only trigger if things
    // that arent players are in it as well so the player will get hit, but only
    // in crossfire
    public int layerMask(bool targeting) {
        int mask = simpleLayerMask();
        if (targeting && type != EntityTypes.Enemy) {
            mask &= ~(1 << 0); // dont aim at player if ur a thingy
        }
        return mask;
    }

    public new void LeftClick(ClickEventHandler e) {
        List<AreaAttack> attackList = new List<AreaAttack>();
        Debug.Log(e);
        foreach (Attack a in attacks) {
            if (a.t == AttackType.Area &&
                (a.attackProperties() & SpecialProperties.predictive) != 0 &&
                (a.attackProperties() & SpecialProperties.homing) == 0) {
                // show funny target
                attackList.Add((AreaAttack)(a));
            }
        }
        if (attackList.Count > 0) {
            Debug.Log(attackList.Count);
            GameObject target = new GameObject();
            SpriteRenderer r = target.AddComponent<SpriteRenderer>();
            r.sprite = Item.GetGraphic("target");
            target.transform.position = attackList[0].center;

            void nextClick(Vector3 worldPoint) {
                worldPoint.z = 0;
                foreach (AreaAttack a in attackList) {
                    a.center = worldPoint;
                }
                target.transform.position = worldPoint;
                attackList.Clear();
                Despawn d = target.AddComponent<Despawn>();
                d.deathTimer = 0.4f;
            }
            e.todoList[0].Enqueue(nextClick);
        }
    }
    public virtual void RemoveStats(Stats changes, float multiplier = 1) {
        maxHealth -= changes.maxHealth * multiplier;
        regen -= changes.HpRegen * multiplier;
        baseDamage -= changes.damage * multiplier;
        dmgMultipler -= changes.dmgMultipler * multiplier;
        attackSpeed -= changes.attackSpeed * multiplier;
        attackRate -= changes.attackRate * multiplier;
        shotSpeed -= changes.shotSpeed * multiplier;
        range -= changes.range * multiplier;
        peirce -= (int)(changes.peirce * multiplier);
        funnelShots += (int)(changes.funnelShots * multiplier);
        crossShots += (int)(changes.crossShots * multiplier);
        if (changes.range != 0 && type == EntityTypes.Turret) {
            ValidateRange(true);
        }
    }

    public virtual void ApplyStats(Stats changes, float multiplier = 1) {
        maxHealth += changes.maxHealth * multiplier;
        regen += changes.HpRegen * multiplier;
        baseDamage += changes.damage * multiplier;
        dmgMultipler += changes.dmgMultipler * multiplier;
        attackSpeed += changes.attackSpeed * multiplier;
        attackRate += changes.attackRate * multiplier;
        shotSpeed -= changes.shotSpeed * multiplier;
        range += changes.range * multiplier;
        peirce += (int)(changes.peirce * multiplier);
        funnelShots -= (int)(changes.funnelShots * multiplier);
        crossShots -= (int)(changes.crossShots * multiplier);
        if (changes.range != 0 && type == EntityTypes.Turret) {
            ValidateRange(false);
        }
    }

    // reset is when you are shrinking the range
    public void ValidateRange(bool reset = false) {
        HexCoord pos = HexCoord.NearestHex(transform.position);
        if (reset) {
            World.UpdateState(pos, NodeState.targeted, ChangeStateMethod.Off,
                              maxRange);
        }
        maxRange = 0;
        foreach (Attack a in attacks) {
            if (a.attackRange() > maxRange) {
                maxRange = a.attackRange();
            }
        }
        World.UpdateState(pos, NodeState.targeted, ChangeStateMethod.On,
                          maxRange);
    }

    public void AddAttack<A>()
        where A : Attack, new() {
        Attack a = new A();
        attacks.Add(a);
        a.perent = this;

        if (a.attackRange() > maxRange && type == EntityTypes.Turret) {
            maxRange = a.attackRange();
            HexCoord pos = HexCoord.NearestHex(transform.position);
            World.UpdateState(pos, NodeState.targeted, ChangeStateMethod.On,
                              maxRange);
        }
    }

    public void AddProc<P>()
        where P : Proc, new() { procs.Add(new P()); }

    public void ApplyDebuffs(float procCoefficent, Damageable d) {
        foreach (Debuff de in toApply) {
            float chance = procCoefficent * de.chance;
            if (de.modulePerent != null) {
                chance *= de.modulePerent.power;
                if (de.perent.type == EntityTypes.Turret) {
                    chance *= Module.DistanceMultiplier((Turret)de.perent,
                                                        de.modulePerent.me);
                }
            }
            if (Random.value < chance) {
                de.Apply(d, this, Mathf.Max(1, chance));
            }
        }
    }

    public void RunProc(float procCoefficent, Attack att, float dmg,
                        Damageable hit) {
        foreach (Proc p in procs) {
            if (Random.value < procCoefficent * p.chance) {
                (p.Go(dmg, att)).OnProc(hit);
            }
        }
    }

    public virtual void RunAttacks() {
        foreach (Attack a in attacks) {
            a.Update();
        }
    }

    public override void FixedUpdate() {
        base.FixedUpdate();
        RunAttacks();
    }

    public bool hasProperties(SpecialProperties p) {
        return (specialProperties & p) != 0;
    }
}
}
