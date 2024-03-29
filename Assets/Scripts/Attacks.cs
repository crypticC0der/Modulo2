using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo {
public enum AttackType { Ranged, Close, Melee, Area }

public interface ModuleEffect {
    public void ApplySpawnStats(float power, float stability) {}
}

/// <summary>
/// damager i barely know her
/// (lamo)
/// </summary>
[System.Serializable]
public abstract class Damager {
    public float procCoefficent;
    public float dmg;
    public abstract void DmgOverhead(DamageData d, Damageable hit, float t = 1);
}

/// <summary>
/// this is basically a contained fsm that can deal damage and target enemies
/// independantly
/// </summary>
[System.Serializable]
public abstract class Attack : Damager {
    public static Vector3 RandomInCircle(float range) {
        float theata = Random.Range(0, 2 * Mathf.PI);
        float r = (range - 0.7f) * (Mathf.Sqrt(Random.value)) +
                  0.7f; // uniform probabilities
        Vector3 d = new Vector3(r * Mathf.Cos(theata), r * Mathf.Sin(theata));
        return d;
    }
    public static LineRenderer MinimalRay(Color startColor, Color endColor,
                                   Vector3[] points) {
        GameObject g = new GameObject();
        g.layer = 7;
        LineRenderer line = g.AddComponent<LineRenderer>();
        line.material = Resources.Load<Material>("RayMaterial");
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.startColor = startColor;
        line.endColor = endColor;
        line.startWidth = 0.05f;
        line.useWorldSpace = true;
        line.numCapVertices = 3;
        return line;
    }

    public RayDespawnTracking basicBeam(Color startColor, Color endColor,
                                        LinkedList<Damageable> aims,
                                        int listLen, DamageData d) {
        Vector3[] points = new Vector3[listLen + 1];
        points[0] = perent.transform.position;
        int i = 0;
        foreach (Damageable aim in aims) {
            points[i + 1] = aim.transform.position;
            i++;
        }
        LineRenderer line = MinimalRay(startColor, endColor, points);
        RayDespawnTracking rdt =
            line.gameObject.AddComponent<RayDespawnTracking>();
        rdt.lineRenderer = line;
        rdt.length = (points[0] - points[points.Length - 1]).magnitude;
        rdt.data = d;
        rdt.aims = aims;
        rdt.perent = this;
        return rdt;
    }

    public Despawn basicRay(Color startColor, Color endColor,
                            Vector3[] points) {
        LineRenderer line = MinimalRay(startColor, endColor, points);
        RayTracking d = line.gameObject.AddComponent<RayTracking>();
        d.deathTimer = 0.25f;
        d.lineRenderer = line;
        d.perent = this;
        d.FixedUpdate();
        return d;
    }

    public static GasBomb basicGas(Attack attack, string assetPath,
                                   bool homing = false, float scale = 1) {
        GameObject g =
            minimalBullet<CircleCollider2D>(attack, assetPath, scale);
        GasBomb gb = g.AddComponent<GasBomb>();
        gb.perentLayer = attack.perent.layerMask(false);
        float spd = attack.shotSpeed();
        gb.initialDensity = (attack.peirce() + 5) / 10f;
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        gb.lossTime = spd * gb.initialDensity * 4 / attack.attackRange();
        if ((attack.attackProperties() & SpecialProperties.returning) != 0) {
            ReturningShot ret = g.AddComponent<ReturningShot>();
            ret.r = r;
            ret.perent = attack;
        }
        if (homing) {
            SimpleHoming b = g.AddComponent<SimpleHoming>();
            b.rb = g.GetComponent<Rigidbody2D>();
            b.layerMask = attack.perent.layerMask(false);
        }
        return gb;
    }

    public static ProcOnCollsion basicSpike(Attack attack, string assetPath,
                                            Vector3 v, bool homing = false,
                                            float scale = 1) {
        RigidbodyType2D type = RigidbodyType2D.Static;
        if ((attack.attackProperties() &
             (SpecialProperties.returning | SpecialProperties.homing)) != 0) {
            type = RigidbodyType2D.Kinematic;
        }
        GameObject g =
            minimalBullet<CircleCollider2D>(attack, assetPath, scale, type);
        ProcOnCollsion p = g.AddComponent<ProcOnCollsion>();
        p.peirce = attack.peirce();
        p.perentLayer = attack.perent.layerMask(false);
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        Despawn d = g.AddComponent<Despawn>();
        d.deathTimer = 60;
        if ((attack.attackProperties() & SpecialProperties.returning) != 0) {
            ReturningShot ret = g.AddComponent<ReturningShot>();
            ret.r = r;
            ret.perent = attack;
            ret.r.velocity = v.normalized * attack.shotSpeed();
        } else {
            SpikeMove m = g.AddComponent<SpikeMove>();
            m.spd = attack.shotSpeed();
        }
        if (homing) {
            SimpleHoming b = g.AddComponent<SimpleHoming>();
            b.rb = r;
            b.layerMask = attack.perent.layerMask(false);
        }
        return p;
    }

    public static ProcOnCollsion basicBullet(Attack attack, string assetPath,
                                             bool homing = false,
                                             float scale = 1) {
        GameObject g = minimalBullet<BoxCollider2D>(attack, assetPath, scale);
        Despawn de = g.AddComponent<Despawn>();
        de.deathTimer = 2 * (attack.attackRange() / attack.shotSpeed());
        ProcOnCollsion p = g.AddComponent<ProcOnCollsion>();
        p.peirce = attack.peirce();
        p.perentLayer = attack.perent.layerMask(false);
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        if ((attack.attackProperties() & SpecialProperties.returning) != 0) {
            ReturningShot ret = g.AddComponent<ReturningShot>();
            ret.r = r;
            ret.perent = attack;
            de.deathTimer *= Mathf.PI / 2;
        }
        if (homing) {
            HomingBullet b = g.AddComponent<HomingBullet>();
            b.rb = r;
            b.layerMask = attack.perent.layerMask(false);
            b.spd = attack.shotSpeed();
        }
        return p;
    }

    public static GameObject
    minimalBullet<C>(Attack attack, string assetPath, float scale,
                     RigidbodyType2D rbt = RigidbodyType2D.Kinematic)
        where C : Collider2D {
        GameObject g = new GameObject();
        g.layer = 7;
        g.transform.position = attack.perent.transform.position;
        g.transform.localScale = new Vector3(scale, scale, 1);
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>(assetPath);
        g.AddComponent<C>().isTrigger = true;
        Rigidbody2D r = g.AddComponent<Rigidbody2D>();
        r.bodyType = rbt;
        r.mass = 1;
        if ((attack.attackProperties() & SpecialProperties.polar) != 0) {
            PolarMove pm = g.AddComponent<PolarMove>();
            pm.perent = attack.perent.transform;
            pm.rb = r;
        }
        return g;
    }

    public override void DmgOverhead(DamageData d, Damageable hit,
                                     float t = 1) {
        d.sender = perent;
        hit.TakeDamage(d);
        perent.RunProc(procCoefficent * t, this, d.dmg, hit);
        perent.ApplyDebuffs(procCoefficent * t, hit);
    }

    public static bool ValidLineOfSight(GameObject a,GameObject b){
        RaycastHit2D[] rh=
            Physics2D.RaycastAll(a.transform.position,
                                b.transform.position - a.transform.position);
        foreach(RaycastHit2D hit in rh){
            if(hit.collider.gameObject!=a && hit.collider.gameObject!=b){
                return false;
            }
        }
        return true;
    }

    protected Collider2D BestCollider(Collider2D[] col,
                                      bool checkLineOfSight = false) {
        Collider2D goodCol = null;
        Priority best = 0;
        float dist = 9999;
        foreach (Collider2D c in col) {
            Damageable d = c.GetComponent<Damageable>();
            if (d != null) {
                float cdist = (c.transform.position - perent.transform.position)
                                  .magnitude;
                bool canSee = true;
                if (checkLineOfSight) {
                    canSee = ValidLineOfSight(perent.gameObject,c.gameObject);
                }
                bool betterCollider = canSee &&
                    (d.priority > best ||
                     d.priority == best && cdist < dist);
                if (betterCollider) {
                    goodCol = c;
                    best = d.priority;
                    dist = cdist;
                }
            }
        }
        return goodCol;
    }

    abstract public void Update();
    abstract public Vector3 AtFunc(GameObject g);
    abstract public void AddAttack(Combatant c);

    public AttackType t;
    public Combatant perent;
    public float timerMax;
    protected float timer = 3;
    public float range = 1;
    public float shotSpd = 5;
    public int attackPeirce = 0;
    public SpecialProperties attackProps = 0;
    public SpecialProperties disabledProps = 0;

    protected float chargingPercent = 0;
    protected float rapidFireShotCount = 0;

    public virtual float damage() { return (dmg)*perent.damage(); }
    public virtual float attackRate() {
        return attackRate(rapidFireShotCount, chargingPercent);
    }
    public virtual float attackRate(float rapid, float charge) {
        float r = perent.attackSpeed + timerMax;
        if ((attackProperties() & SpecialProperties.rapidFire) != 0) {
            if (rapid >= 3) {
                r *= 3;
            } else {
                r /= 4;
            }
        }
        if ((attackProperties() & SpecialProperties.spdUp) != 0) {
            r *= Mathf.Lerp(1, 0.2f, charge);
        }
        return Mathf.Max(r, 0.03f);
    }
    public virtual float attackRange() { return perent.range * range; }
    public virtual float shotSpeed() { return perent.shotSpeed * shotSpd; }
    public virtual int peirce() { return perent.peirce + attackPeirce; }
    public virtual SpecialProperties attackProperties() {
        return (attackProps | perent.specialProperties) & (~disabledProps);
    }

    public bool hasProperties(SpecialProperties p) {
        return (attackProperties() & p) != 0;
    }

    public const int maxShots = 18;
}

/// <summary>
/// basically this is a hook you can apply to an object woo
/// dmg in this system is just a dmg*dmgMultiplier
/// </summary>
[System.Serializable]
abstract public class Proc : Damager {
    ///< summary>
    /// runs the proc function on a gameobject
    ///</summary>
    public virtual void OnProc(Damageable d) {
        perent.perent.RunProc(procCoefficent, perent, dmg, d);
        perent.perent.ApplyDebuffs(procCoefficent, d);
    }

    ///< summary>
    /// makes a new proc for an attack
    ///</summary>
    public virtual Proc Go(float d, Attack perent) {
        Proc p = newSelf();
        p.dmg = d * dmgMultiplier;
        p.perent = perent;
        return p;
    }

    public override void DmgOverhead(DamageData d, Damageable hit,
                                     float t = 1) {
        d.sender = perent.perent;
        hit.TakeDamage(d);
        perent.perent.RunProc(procCoefficent * t, perent, d.dmg, hit);
        perent.perent.ApplyDebuffs(procCoefficent * t, hit);
    }

    public abstract Proc newSelf();
    public Attack perent;
    public float chance;
    public float dmgMultiplier;
    public Transform collider;
}

/// <summary>
/// this allows you to add the attack to an object
/// </summary>
public class ProcOnCollsion : MonoBehaviour {
    public int peirce = 0;
    public Proc p;
    public int perentLayer;
    public void OnTriggerEnter2D(Collider2D c) {
        if ((1 << c.gameObject.layer & perentLayer) != 0) {
            Damageable d = c.GetComponent<Damageable>();
            if (d != null) {
                p.collider = transform;
                p.OnProc(d);
                peirce--;
                if (peirce < 0) {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}

public abstract class RangedAttack : Attack {
    public Proc impact;
    abstract public void AtFunc(Vector3 p);

    public virtual Vector3 AttackProjection(GameObject g) {
        Vector2 d = g.transform.position - perent.transform.position;
        if ((attackProperties() & SpecialProperties.predictive) != 0) {
            float time = d.magnitude / shotSpeed();
            Vector2 v = g.GetComponent<Rigidbody2D>().velocity;
            if (Vector2.Dot(d.normalized, v.normalized) > -0.5) {
                d = d + v * time;
            }
        }
        return d;
    }

    public RangedAttack() { t = AttackType.Ranged; }

    public Collider2D Target() {
        Collider2D[] o = Physics2D.OverlapCircleAll(
            perent.transform.position, attackRange(), perent.layerMask(true));
        return BestCollider(o,true);
    }

    public override void Update() {
        if (timer <= 0) {
            if (chargingPercent < 1) {
                chargingPercent += Time.deltaTime / 5;
            }
            Collider2D c;
            bool hit = false;
            Vector3 d = Vector2.zero;
            if ((attackProperties() & SpecialProperties.random) != 0) {
                hit = true;
                d = RandomInCircle(attackRange());
            } else if ((c = Target()) != null) {
                d = AttackProjection(c.gameObject);
                hit = true;
            }
            if (hit) {
                float angle;
                angle = Mathf.Atan(d.x / d.y);
                if (d.y < 0) {
                    angle += Mathf.PI;
                }
                int nShots = perent.totalShots();
                float initialMulti = perent.dmgMultipler;
                int initialCross = perent.crossShots;
                int initialFunnel = perent.funnelShots;
                if (nShots >= maxShots) {
                    perent.dmgMultipler *= nShots / maxShots;
                    perent.crossShots = 1;
                    perent.funnelShots = maxShots;
                }
                for (int i = 0; i < perent.crossShots; i++) {
                    float r = angle + (i * 2 * Mathf.PI / perent.crossShots) -
                              (Mathf.PI * (perent.funnelShots - 1) / maxShots);
                    for (int j = 0; j < perent.funnelShots; j++) {
                        Vector2 direction =
                            new Vector2(Mathf.Sin(r), Mathf.Cos(r)) *
                            d.magnitude;
                        AtFunc(direction);
                        r += 2 * Mathf.PI / maxShots;
                    }
                }
                perent.dmgMultipler = initialMulti;
                perent.crossShots = initialCross;
                perent.funnelShots = initialFunnel;
                rapidFireShotCount = (rapidFireShotCount + 1) % 4;
                timer = attackRate() + perent.attackSpeed;
            }
        } else if (chargingPercent > 0) {
            chargingPercent -= 2 * Time.deltaTime / 5;
        }
        timer -= Time.deltaTime * perent.attackRate;
    }
}

public abstract class AreaAttack : Attack {
    public Vector3 center;

    public new int peirce() { return base.peirce() * perent.totalShots();
}

    public AreaAttack() {
        t = AttackType.Area;
        attackProps |= SpecialProperties.predictive;
    }
    public Collider2D[] Target() {
        if ((attackProperties() & SpecialProperties.homing) != 0) {
            // set center
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                perent.transform.position, 10 * attackRange(),
                perent.layerMask(false));
            int tries = 1;
            if ((attackProperties() & SpecialProperties.predictive) == 0) {
                tries = 3;
            }
            int besthits = 0;
            Vector3 bestpos = perent.transform.position;
            for (int i = 0; i < tries; i++) {
                Vector3 p = hits[i].transform.position;
                Collider2D[] inRange = Physics2D.OverlapCircleAll(
                    p, attackRange(), perent.layerMask(false));
                if (inRange.Length >= besthits) {
                    besthits = inRange.Length;
                    bestpos = p;
                }
            }
            center = bestpos;
        } else if ((attackProperties() & SpecialProperties.predictive) == 0) {
            center = perent.transform.position;
        }
        if ((attackProperties() & SpecialProperties.random) != 0) {
            center += RandomInCircle(5);
        }
        // predicive center is sorted out
        return Physics2D.OverlapCircleAll(center, attackRange(),
                                        perent.layerMask(false));
    }

    public virtual void SingleAtFunc() {}

    public int currentPeirce;
    public override void Update() {
        if (timer <= 0) {
            if (chargingPercent < 1) {
                chargingPercent += Time.deltaTime / 5;
            }
            Collider2D[] c = Target();
            currentPeirce = peirce();
            bool notPlayer =
                (attackProperties() & SpecialProperties.random) != 0;
            for (int i = 0; i < c.Length; i++) {
                if (c[i].gameObject.layer != 0) {
                    notPlayer = true;
                    break;
                }
            }
            if (notPlayer) {
                SingleAtFunc();
                for (int i = 0; i < c.Length && 0 < currentPeirce; i++) {
                    AtFunc(c[i].gameObject);
                    currentPeirce--;
                }
            }
            if (c.Length > 0 || !notPlayer) {
                rapidFireShotCount = (rapidFireShotCount + 1) % 4;
            }
            timer = attackRate() + perent.attackSpeed;
        } else if (chargingPercent > 0) {
            chargingPercent -= 2 * Time.deltaTime / 5;
        }
        timer -= Time.deltaTime * perent.attackRate;
    }
}

public abstract class MeleeAttack : Attack {
    [SerializeReference]
    public List<Weapon> weapons;
    public override Vector3 AtFunc(GameObject o) { return Vector3.zero; }

    protected abstract Weapon basicWeapon();

    public MeleeAttack() {
        t = AttackType.Melee;
        weapons = new List<Weapon>();
    }
    public Collider2D Target() { return null; }

    protected float GetDirection() {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            perent.transform.position, attackRange(), perent.layerMask(true));
        float sum = 0;
        foreach (Collider2D c in cols) {
            Vector2 direction =
                c.transform.position - perent.transform.position;
            float dot = Vector2.Dot(direction.normalized,
                                    weapons[0].self.transform.right);
            if (dot != 0) {
                sum += 1 / dot;
            }
        }
        if (sum > 0) {
            return 1;
        } else if (sum < 0) {
            return -1;
        } else {
            return 0;
        }
    }

    public bool increase = false;
    public override void Update() {
        increase = false;
        while (weapons.Count < perent.totalShots()) {
            Weapon w;
            weapons.Add(w = basicWeapon());
            w.number = weapons.Count - 1;
            increase = true;
        }
        while (weapons.Count > perent.totalShots()) {
            GameObject.Destroy(weapons[weapons.Count - 1].self.gameObject);
            weapons.Remove(weapons[weapons.Count - 1]);
        }

        foreach (Weapon c in weapons) {
            c.Update();
        }
    }
}

// depricated
public abstract class CloseAttack : Attack {
    public new float damage() { return base.damage() * perent.totalShots();
    }

    public CloseAttack() {
        Debug.Log("depricated,DNU, see melee\nAttacks.cs:461");
        t = AttackType.Close;
    }

    public Collider2D Target() {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            perent.transform.position, attackRange(), perent.layerMask(false));
        return BestCollider(cols);
    }

    public override void Update() {
        if (timer <= 0) {
            if (chargingPercent < 1) {
                chargingPercent += Time.deltaTime / 5;
            }
            Collider2D c;
            if ((c = Target()) != null) {
                AtFunc(c.gameObject);
                timer = attackRate() + perent.attackSpeed;
                rapidFireShotCount = (rapidFireShotCount + 1) % 4;
            }
        } else if (chargingPercent > 0) {
            chargingPercent -= 2 * Time.deltaTime / 5;
        }
        timer -= Time.deltaTime * perent.attackRate;
    }
}
}
