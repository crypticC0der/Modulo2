
using UnityEngine;
using System.Collections.Generic;
namespace Modulo {

[System.Serializable]
public enum MeleeState { Attacking, Returning, Waiting, Ready, MidAttack }

public class WeaponHitter : MonoBehaviour {
    public Attack perent;
    public int perentLayer;
    public void OnHit(GameObject g, bool stay) {
        if ((1 << g.gameObject.layer & perentLayer) != 0) {
            Damageable d = g.GetComponent<Damageable>();
            if (d != null) {
                if (stay) {
                    DamageData data = new DamageData {
                        dmg = perent.damage() * perent.perent.attackRate *
                              Time.deltaTime / perent.attackRate(),
                        sender = perent.perent
                    };
                    perent.DmgOverhead(data, d, Time.deltaTime);
                } else {
                    DamageData data = new DamageData { dmg = perent.damage(),
                                                       sender = perent.perent };
                    perent.DmgOverhead(data, d);
                }
            }
        }
    }
}

public class WeaponStay : WeaponHitter {
    public void OnTriggerStay2D(Collider2D c) { OnHit(c.gameObject, true); }
}

public class WeaponHit : WeaponHitter {
    public void OnTriggerEnter2D(Collider2D c) { OnHit(c.gameObject, false); }
}

public class WeaponParticle : WeaponHitter {
    public void OnParticleCollision(GameObject g) { OnHit(g, true); }
}

[System.Serializable]
public abstract class Weapon {
    public int number;
    public Vector2 start;
    public MeleeAttack perent;
    public MeleeState s = MeleeState.Waiting;
    public float t = 0;
    public float chargingPercent = 0;
    public float rapidFireShotCount = 0;
    [SerializeReference]
    public GameObject self;
    public abstract Collider2D Target(bool smart);
    public Vector3 baseAngle;

    protected bool chargeChange = false;
    public float timeStep() {
        float step = Time.deltaTime;

        if ((perent.attackProperties() & SpecialProperties.spdUp) != 0) {
            if (chargingPercent > 0 && s == MeleeState.Attacking ||
                s == MeleeState.MidAttack) {
                step /= Mathf.Lerp(1, 0.2f, chargingPercent);
                if (!chargeChange) {
                    chargingPercent -= 2 * Time.deltaTime / 5;
                    chargeChange = true;
                }
            }
        }
        if ((perent.attackProperties() & SpecialProperties.rapidFire) != 0) {
            if (rapidFireShotCount >= 3 && s == MeleeState.Returning) {
                step /= 6;
            } else {
                step *= 4;
            }
        }

        return Mathf.Min(step, Time.deltaTime * 5);
    }

    public virtual Vector3 AttackProjection(Rigidbody2D r) {
        Vector3 d = r.transform.position;
        if ((perent.attackProperties() & SpecialProperties.predictive) != 0) {
            Vector3 distance = (r.transform.position - self.transform.position);
            float time = distance.magnitude / perent.shotSpeed();
            Vector3 v = r.velocity;
            if (Vector2.Dot(distance.normalized, v.normalized) > -0.5) {
                d += v * time;
            }
        }
        return d;
    }

    protected Collider2D c;
    public virtual void Update() {
        chargeChange = false;
        if (s == MeleeState.Waiting || s == MeleeState.Ready) {
            if (t <= 0) {
                s = MeleeState.Ready;
                if (chargingPercent < 1) {
                    chargingPercent += Time.deltaTime / 5;
                }
                if (c != null) {
                    s = MeleeState.Attacking;
                    t = perent.attackRate(rapidFireShotCount, chargingPercent) +
                        perent.perent.attackSpeed;
                    rapidFireShotCount = (rapidFireShotCount + 1) % 4;
                }
            } else if (chargingPercent > 0) {
                chargingPercent -= 2 * Time.deltaTime / 5;
            }
            t -= Time.deltaTime * perent.perent.attackRate;
        }
    }
}

public class Dagger : Weapon {

    public Vector3 target;

    public override Collider2D Target(bool smart) {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            self.transform.position, perent.attackRange(),
            perent.perent.layerMask(true));
        float best = -1;
        Collider2D bcol = null;
        foreach (Collider2D c in cols) {
            Vector2 distance = c.transform.position - self.transform.position;
            float dot = Vector2.Dot(distance.normalized, self.transform.up);
            if (dot > best &&
                (c.transform.position - self.transform.position).magnitude >
                    (perent.shotSpeed() * timeStep() * .1 / Time.deltaTime)) {
                bcol = c;
                best = dot;
            }
        }
        if (best > 0.5 || !smart) {
            return bcol;
        }
        return null;
    }

    public override void Update() {
        chargeChange = false;
        if (s == MeleeState.Attacking) {
            Vector3 distance = target - self.transform.position;
            self.transform.eulerAngles =
                new Vector3(0, 0, World.VecToAngle(distance));
            if (distance.magnitude > perent.shotSpeed() * timeStep()) {
                self.transform.position +=
                    distance.normalized * perent.shotSpeed() * timeStep();
            } else {
                if ((perent.attackProperties() & SpecialProperties.homing) !=
                    0) {
                    s = MeleeState.Waiting;
                } else {
                    self.transform.position = target;
                    s = MeleeState.Returning;
                }
            }
        } else if (s == MeleeState.Returning) {
            Vector3 distance = (Vector3)start - self.transform.position;
            self.transform.eulerAngles =
                new Vector3(0, 0, 180 + World.VecToAngle(distance));
            if (distance.magnitude > perent.shotSpeed() * timeStep()) {
                self.transform.position += (Vector3)distance.normalized *
                                           perent.shotSpeed() * timeStep();
            } else {
                self.transform.position = start;
                s = MeleeState.Waiting;
            }
        }

        if (s == MeleeState.Waiting || s == MeleeState.Ready) {
            if (!((Vector2)(self.transform.position) != start &&
                  ((perent.attackProperties() & SpecialProperties.homing) !=
                   0))) {
                self.transform.eulerAngles = baseAngle;
            }
            if (t <= 0) {
                s = MeleeState.Ready;
                if (chargingPercent < 1) {
                    chargingPercent += Time.deltaTime / 5;
                }
                Collider2D c = null;
                // smart is when you arent at home or arent homing
                if (!perent.hasProperties(SpecialProperties.random)) {
                    c = Target(!((Vector2)(self.transform.position) != start &&
                                 ((perent.attackProperties() &
                                   SpecialProperties.homing) != 0)));
                }
                if (c != null ||
                    perent.hasProperties(SpecialProperties.random)) {
                    if (perent.hasProperties(SpecialProperties.random)) {
                        target = Attack.RandomInCircle(perent.attackRange()) +
                                 perent.perent.transform.position;
                    } else {
                        target = AttackProjection(c.attachedRigidbody);
                    }
                    s = MeleeState.Attacking;
                    t = perent.attackRate(rapidFireShotCount, chargingPercent) +
                        perent.perent.attackSpeed;
                    rapidFireShotCount = (rapidFireShotCount + 1) % 4;
                }
            } else if (chargingPercent > 0) {
                chargingPercent -= 2 * Time.deltaTime / 5;
            }
            t -= Time.deltaTime * perent.perent.attackRate;
        }

        if (s == MeleeState.Ready &&
            ((Vector2)(self.transform.position) != start &&
             ((perent.attackProperties() & SpecialProperties.homing) != 0))) {
            // if ready and homing and hasnt returned
            s = MeleeState.Returning;
        }
    }
}

public class Tentacle : Weapon {
    const float aimedSegLength = .3f;
    const int segMultiplier = 6;

    public LineRenderer r;
    public int segments = 12;
    int numPoints = 13;
    Vector3[] points;
    public Vector2[] aim;

    Vector3 Move(Vector2 pos, Vector2 want) {
        float speed = (perent.shotSpeed()) * timeStep();
        Vector2 d = want - pos;
        if (d.magnitude < speed) {
            return want;
        } else {
            return pos + d.normalized * speed;
        }
    }

    void ManageSegments() {
        Vector2 start = self.transform.position;
        float segLen = perent.attackRange() / segments;
        for (int i = 0; i < aim.Length; i++) {
            Vector2 end = aim[i];
            int minMax = (i + 1) * segments / aim.Length;
            int maxMin = i * segments / aim.Length;
            // backwards
            points[minMax] = end;
            for (int j = 1; j < segments / aim.Length; j++) {
                Vector2 d = points[minMax - j] - points[minMax - j + 1];
                d = d.normalized * segLen;
                points[minMax - j] = (Vector2)points[minMax - j + 1] + d;
            }

            points[i * segments / aim.Length] = start;
            for (int j = 0; j < segments / aim.Length; j++) {
                Vector2 d = points[maxMin + j + 1] - points[maxMin + j];
                d = d.normalized * segLen;
                points[maxMin + j + 1] = (Vector2)points[maxMin + j] + d;
            }
            start = points[minMax];
        }
    }

    public Tentacle() {
        points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++) {
            points[i] = new Vector3(0, 0, 0);
        }
        GameObject obj = new GameObject("tentacle");
        obj.layer = 7;
        self = obj;
        r = self.AddComponent<LineRenderer>();
        r.positionCount = numPoints;
        r.startWidth = 0.075f;
        r.endWidth = 0.05f;
        r.useWorldSpace = true;
        r.numCapVertices = 3;
        r.numCornerVertices = 3;
        r.startColor = MeshGen.MeshGens.ColorFromHex(0x7cff01ff);
        r.endColor = MeshGen.MeshGens.ColorFromHex(0x102000ff);
        r.material = Resources.Load<Material>("RayMaterial");
    }

    void CheckSegments() {
        float segLen = perent.attackRange() / segments;
        int suggested = segments;
        if (segLen > aimedSegLength) {
            suggested = segments + 6;
        } else if (segments != 6) {
            suggested = (segments - 6);
        }

        float newLen = perent.attackRange() / suggested;
        if (Mathf.Abs(segLen - aimedSegLength) >
            Mathf.Abs(newLen - aimedSegLength)) {
            points = new Vector3[suggested + 1];
            for (int i = 0; i < (suggested + 1); i++) {
                float f = (float)i / (suggested);
                points[i] = PointOnLine(f);
            }

            segments = suggested;
            numPoints = suggested + 1;

            r.positionCount = numPoints;
        }
    }

    Vector3 PointOnLine(float f) {
        if (f == 1) {
            return r.GetPosition(segments);
        } else {
            f *= segments;
            int i = (int)f;
            Vector3 v1 = r.GetPosition(i);
            Vector3 v2 = r.GetPosition(i + 1); // lol get coined lmao lol
            return Vector2.LerpUnclamped(v1, v2, f - i);
        }
    }

    void SetAim(Vector2 v, int idx) {
        Debug.Log(idx);
        aim[idx] = Move(r.GetPosition((1 + idx) * segments / aim.Length), v);
    }

    void ValidateTarget(ref Collider2D target, Vector2 prevpos) {
        if (target != null) {
            Vector2 d = (Vector2)target.transform.position - prevpos;
            float segLen = perent.attackRange() / aim.Length;
            if (d.magnitude > segLen * 1.4f) {
                ((TentacleAttack)perent).hitTargets.Remove(target);
                target = null;
            } else {
                ((TentacleAttack)perent).hitTargets.Add(target);
            }
        }
    }

    Collider2D GetTarget(Vector2 center, float radius) {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            center, radius, perent.perent.layerMask(true));
        for (int i = 0; i < cols.Length; i++) {
            if (!((TentacleAttack)perent).hitTargets.Contains(cols[i])) {
                ((TentacleAttack)perent).hitTargets.Add(cols[i]);
                return cols[i];
            }
        }
        return null;
    }

    float dmgTimer = 0;
    public void DealDamage() {
        Vector3[] positions = new Vector3[numPoints];
        r.GetPositions(positions);
        DamageData dataScaled =
            new DamageData { dmg = perent.damage() * dmgTimer *
                                   perent.perent.attackRate /
                                   perent.attackRate(),
                             sender = perent.perent };
        Globals.DealDmgAlongLine(
            positions, 0, perent.perent.layerMask(false), (Damageable d) => {
                perent.DmgOverhead(dataScaled, d, dmgTimer);
            }, perent.peirce(), positions.Length);
    }

    public void CollisionUpdate() {
        if (perent.hasProperties(SpecialProperties.random)) {
            int total = 1;
            if (perent.hasProperties(SpecialProperties.returning)) {
                total += 1;
            };
            if (perent.hasProperties(SpecialProperties.homing)) {
                total += 1;
            };
            randAims = new Vector3[total];
            randAims[total - 1] = Attack.RandomInCircle(perent.attackRange()) +
                                  self.transform.position;
            for (int i = 0; i < total - 1; i++) {
                randAims[i] = Attack.RandomInCircle(perent.attackRange() / 3) +
                              r.GetPosition((1 + i) * segments / aim.Length);
            }
        } else {
            int add = 0;
            if (perent.hasProperties(SpecialProperties.returning)) {
                add = 1;
            };

            int endWay = 1;
            if ((perent.attackProperties() & SpecialProperties.homing) != 0) {
                endWay++;
            }

            Vector2 prevPos = self.transform.position;
            if (midHit != null &&
                perent.hasProperties(SpecialProperties.homing)) {
                prevPos = midHit.transform.position;
                ValidateTarget(ref midHit, self.transform.position);
            }
            ValidateTarget(ref endHit, prevPos);

            if (endHit == null) {
                if ((self.transform.position - r.GetPosition(segments))
                        .magnitude > 1) {
                    float t = (endWay + add - 1) / (endWay + add);
                    Vector2 center = r.GetPosition((int)(t * segments));
                    endHit = GetTarget(center,
                                       perent.attackRange() / (endWay + add));
                } else {
                    endHit = GetTarget(self.transform.position,
                                       perent.attackRange());
                }
            }
            if (midHit == null) {
                float t = 1 / (endWay + add);
                Vector2 center = r.GetPosition((int)(t * segments));
                midHit =
                    GetTarget(center, perent.attackRange() / (endWay + add));
            }
        }
    }

    public virtual Vector3 AttackProjection(GameObject g) {
        Vector3 d = g.transform.position;
        if ((perent.attackProperties() & SpecialProperties.predictive) != 0) {
            Rigidbody2D r = g.GetComponent<Rigidbody2D>();
            Vector3 end = this.r.GetPosition(numPoints - 1);
            Vector3 distance = (r.transform.position - end);
            float time = distance.magnitude / perent.shotSpeed();
            Vector3 v = r.velocity;
            if (Vector2.Dot(distance.normalized, v.normalized) > -0.5) {
                d += v * time;
            }
        }
        return d;
    }

    public Collider2D endHit;
    public Collider2D midHit;
    public float randomTimer;
    public Vector3[] randAims = new Vector3[3];
    public override void Update() {
        Vector3 offset = new Vector2(0, 0);
        if (perent.hasProperties(SpecialProperties.random)) {
            aim = new Vector2[randAims.Length];
            for (int i = 0; i < randAims.Length; i++) {
                SetAim(randAims[i], i);
            }

        } else {

            int add = 0;
            if (perent.hasProperties(SpecialProperties.returning)) {
                add = 1;
            };
            // i see you laughing at this code
            // a billion if statements sucks mega hard
            // yes i fucking know!!!!, but like otherwise this will be a mess
            // and i dont fucking care fuck off and die shitheel
            if (perent.hasProperties(SpecialProperties.homing)) {
                if (midHit != null && endHit != null) {
                    aim = new Vector2[2 + add];
                    SetAim(AttackProjection(midHit.gameObject), 0);
                    SetAim(AttackProjection(endHit.gameObject), 1);
                } else if (midHit == null && endHit != null) {
                    aim = new Vector2[1 + add];
                    SetAim(AttackProjection(endHit.gameObject), 0);
                } else if (midHit != null && endHit == null) {
                    aim = new Vector2[2 + add];
                    SetAim(AttackProjection(midHit.gameObject), 0);
                    SetAim(self.transform.position - offset, 1);
                } else if (midHit == null && endHit == null) {
                    aim = new Vector2[1 + add];
                    SetAim(self.transform.position - offset, 0);
                }
            } else {
                if (endHit == null) {
                    aim = new Vector2[1 + add];
                    SetAim(self.transform.position - offset, 0);
                } else {
                    aim = new Vector2[1 + add];
                    SetAim(AttackProjection(endHit.gameObject), 0);
                }
            }
            if (perent.hasProperties(SpecialProperties.returning)) {
                aim[aim.Length - 1] = self.transform.position - offset;
            }
        }
        self.transform.position = perent.perent.transform.position + offset;

        CheckSegments();
        ManageSegments();
        r.SetPositions(points);

        dmgTimer += Time.deltaTime;
        if (dmgTimer > 0.05f) {
            DealDamage();
            dmgTimer = 0;
        }
    }

    public override Collider2D Target(bool smart) {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            self.transform.position, perent.attackRange(),
            perent.perent.layerMask(true));
        float best = -1;
        Collider2D bcol = null;
        foreach (Collider2D c in cols) {
            Vector2 distance = c.transform.position - self.transform.position;
            float dot = Vector2.Dot(distance.normalized, self.transform.up);
            if (dot > best &&
                (c.transform.position - self.transform.position).magnitude >
                    (perent.shotSpeed() * timeStep() * .1 / Time.deltaTime)) {
                bcol = c;
                best = dot;
            }
        }
        if (best > 0.5 || !smart) {
            return bcol;
        }
        return null;
    }
}

public class SwordHand : Weapon {
    public LineRenderer r;
    Vector3[] points;
    public Vector3 end;
    public Vector3 center;

    public override Collider2D Target(bool smart) {
        outOfRangeTimer = 0;
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            perent.perent.transform.position, perent.attackRange(),
            perent.perent.layerMask(true));
        foreach (Collider2D col in cols) {
            if (!((ArmAttack)perent).hitTargets.Contains(col) && col != c) {
                ((ArmAttack)perent).hitTargets.Add(col);
                return col;
            }
        }
        return null;
    }

    float swordLength() { return 1 * perent.attackRange() / 3; }

    float armLength() { return 2 * perent.attackRange() / 3; }

    void ManageSegments() {
        Vector2 start = center;
        float segLen = (armLength()) / 2;
        Vector2 end = this.end;
        // backwards
        points[2] = end;

        // middle joint first pass
        Vector2 d = points[1] - points[2];
        d = d.normalized * segLen;
        points[1] = (Vector2)points[2] + d;

        // set joint one to start
        points[0] = start;
        // set end and middle joints
        for (int j = 0; j < 2; j++) {
            d = points[j + 1] - points[j];
            d = d.normalized * segLen;
            points[j + 1] = (Vector2)points[j] + d;
        }

        r.SetPositions(points);
    }

    bool AdjustSwordAngle(float angl, float step = 1) {
        float swingSpeed = 36 * perent.shotSpeed() * timeStep() * step;
        float angleChange = angl - self.transform.eulerAngles.z;
        if (angleChange >= 360) {
            angleChange -= 360;
        }
        if (angleChange < 0) {
            angleChange += 360;
        }
        Vector3 angle = self.transform.eulerAngles;
        float absChang = Mathf.Abs(angleChange);
        if (absChang > 180) {
            absChang = 360 - absChang;
        }
        if (absChang > swingSpeed) {
            if (angleChange < 180) {
                angle.z += swingSpeed;
            } else {
                angle.z -= swingSpeed;
            }
            self.transform.eulerAngles = angle;
            return true;
        } else {
            angle.z = angl;
            self.transform.eulerAngles = angle;
            return false;
        }
        return false;
    }

    public Vector3 previousEnd = Vector3.zero;
    void ManageSword() {
        if (previousEnd != Vector3.zero) {
            rb.AddForceAtPosition((previousEnd - points[2]) / Time.deltaTime,
                                  points[2]);
        }
    }

    public float aimAngle;
    public float aimDelta = 1000;
    void SwingSword() {
        rb.angularVelocity = 0;
        if (!AdjustSwordAngle(aimAngle)) {
            Cooldown();
        } else {
            float newAimDelta =
                Mathf.Abs(aimAngle - self.transform.eulerAngles.z);
            if (newAimDelta > 180) {
                newAimDelta = 360 - newAimDelta;
            }
            if (newAimDelta > aimDelta || newAimDelta == 0) {
                Cooldown();
            }
            aimDelta = newAimDelta;
        }
    }

    float MoveSpeed() { return (perent.shotSpeed()) * timeStep() / 10; }

    Vector3 Move(Vector2 pos, Vector2 want, float step = 1) {
        float speed = MoveSpeed() * step;
        Vector2 d = want - pos;
        if (d.magnitude < speed) {
            return want;
        } else {
            return pos + d.normalized * speed;
        }
    }

    Rigidbody2D rb;
    public SwordHand() {
        aimAngle = 0;
        points = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            points[i] = new Vector3(0, 0, 0);
        }
        self = new GameObject("sword");
        self.layer = 7;
        SpriteRenderer sr = self.AddComponent<SpriteRenderer>();
        sr.sprite = ItemTemplate.GetGraphic("sordy");
        r = self.AddComponent<LineRenderer>();
        r.positionCount = 3;
        r.numCapVertices = 3;
        r.numCornerVertices = 3;
        r.startWidth = 0.07f;
        r.useWorldSpace = true;
        r.endColor = MeshGen.MeshGens.ColorFromHex(0xf2a400ff);
        r.startColor = MeshGen.MeshGens.ColorFromHex(0x815602ff);
        r.material = Resources.Load<Material>("RayMaterial");
        rb = self.AddComponent<Rigidbody2D>();
        Collider2D c = self.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        rb.gravityScale = 0;
        rb.angularDrag = 0.5f;

        // self.transform.eulerAngles=new Vector3(0,0,360*Random.value);
    }

    public void Cooldown() {
        if (perent.hasProperties(SpecialProperties.returning) &&
            initialAngle != -1) {
            aimDelta = 1000;
            aimAngle = initialAngle;
            initialAngle = -1;
        } else {
            s = MeleeState.Waiting;
        }
    }

    public void CollisionUpdate() {
        if (c == null) {
            c = Target(false);
        } else if ((c.transform.position - center).magnitude >
                       1.5 * perent.attackRange() ||
                   outOfRangeTimer > 1f) {
            c = Target(false);
        } else {
            ((ArmAttack)perent).hitTargets.Add(c);
        }
    }

    public Vector3 Target() {
        if (perent.hasProperties(SpecialProperties.random)) {
            float swingSpeed = 4 * perent.shotSpeed();
            float rad = Mathf.PI * Time.time * swingSpeed / 180;

            if (perent.perent.totalShots() >= 18) {
                rad += 2 * Mathf.PI * ((float)(number) / perent.weapons.Count);
            } else {
                float funnel = number % perent.perent.funnelShots;
                float cross = number / perent.perent.funnelShots;
                rad += (cross * 2 * Mathf.PI / perent.perent.crossShots) -
                       (Mathf.PI * (perent.perent.funnelShots - 1) /
                        Attack.maxShots);
                rad += funnel * 2 * Mathf.PI / Attack.maxShots;
            }

            float r = armLength();
            if (perent.hasProperties(SpecialProperties.returning)) {
                r *= (Mathf.Sin(
                          0.25f * Mathf.PI *
                          (Time.time / perent.attackRate(rapidFireShotCount,
                                                         chargingPercent)) *
                          perent.perent.attackRate) +
                      1) /
                     2;
            }
            Vector3 v = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad)) * (r);
            v += center;
            return v;
        } else if (c != null) {
            return c.transform.position;
        } else {
            return perent.perent.transform.position;
        }
    }

    public void MoveCenter() {
        if ((center - perent.perent.transform.position).magnitude <
                perent.attackRange() &&
            perent.hasProperties(SpecialProperties.homing)) {
            if (c != null) {
                center = Move(center, c.transform.position, 0.2f);
            } else {
                center = Move(center, perent.perent.transform.position, 0.4f);
            }
        }
    }

    public float outOfRangeTimer = 0;
    float initialAngle;
    public override void Update() {
        self.transform.localScale = new Vector3(1, swordLength() / 3, 1);
        if (perent.hasProperties(SpecialProperties.random)) {
            end = Target();
            Vector3 rot = self.transform.eulerAngles;
            rot.z = 180 *
                    (Time.time /
                     perent.attackRate(rapidFireShotCount, chargingPercent)) *
                    perent.perent.attackRate;
            self.transform.eulerAngles = rot;
            MoveCenter();
        } else {
            if (c == null &&
                (s == MeleeState.MidAttack || s == MeleeState.Attacking)) {
                Cooldown();
            }
            base.Update();
            switch (s) {
            case MeleeState.Waiting:
                if (c != null &&
                    perent.hasProperties(SpecialProperties.predictive)) {
                    end = Move(end, c.transform.position);
                    ManageSword();
                    MoveCenter();
                }
                break;
            case MeleeState.Ready:
                end = Move(end, perent.perent.transform.position);
                ManageSword();
                MoveCenter();
                break;
            case MeleeState.Attacking:
                if ((perent.perent.transform.position - c.transform.position)
                        .magnitude > perent.attackRange() * 5f / 6) {
                    outOfRangeTimer += Time.deltaTime;
                }
                if ((c.transform.position - points[2]).magnitude <
                        swordLength() / 2 ||
                    perent.hasProperties(SpecialProperties.predictive) ||
                    (outOfRangeTimer > 0.5f &&
                     (c.transform.position - points[2]).magnitude <
                         swordLength())) {
                    float change =
                        World.VecToAngle(c.transform.position - points[2]) -
                        self.transform.eulerAngles.z;
                    if (change > 180) {
                        change -= 360;
                    }
                    if (change <= -180) {
                        change += 360;
                    }

                    if (change > 0) {
                        aimAngle = self.transform.eulerAngles.z + 90;
                    } else {
                        aimAngle = self.transform.eulerAngles.z - 90;
                    }

                    if (aimAngle > 360) {
                        aimAngle = aimAngle - 360;
                    }
                    if (aimAngle < 0) {
                        aimAngle = aimAngle + 360;
                    }

                    initialAngle = self.transform.eulerAngles.z;
                    aimDelta = 1000;
                    s = MeleeState.MidAttack;
                } else {
                    end = Move(end, c.transform.position);
                    ManageSword();
                    MoveCenter();
                }
                break;
            case MeleeState.MidAttack:
                outOfRangeTimer = 0;
                if (perent.hasProperties(SpecialProperties.predictive) &&
                    (c.transform.position - points[2]).magnitude >
                        swordLength() / 2) {
                    end = Move(end, c.transform.position);
                    // self.transform.position=points[2];
                }
                SwingSword();
                break;
            }
        }

        previousEnd = points[2];
        ManageSegments();
        self.transform.position = points[2];
        Vector3 v = new Vector3(0, 0, 0);
        float rad = self.transform.eulerAngles.z * Mathf.PI / 180;
        float len = swordLength() / 2;
        v.x = Mathf.Sin(-rad) * len;
        v.y = Mathf.Cos(rad) * len;
        self.transform.position = points[2] + v;
        rb.velocity = Vector2.zero;
    }
}

public class Beam : Weapon {
    const float offset = 0.3f;
    public Vector3 center;
    public float beamRotation;

    public override Collider2D Target(bool smart) {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            perent.perent.transform.position, perent.attackRange(),
            perent.perent.layerMask(true));
        foreach (Collider2D col in cols) {
            if (!((SaberAttack)perent).hitTargets.Contains(col) && col != c) {
                ((SaberAttack)perent).hitTargets.Add(col);
                return col;
            }
        }
        return null;
    }

    float swingSpeed() { return 24 * perent.shotSpeed(); }

    bool AdjustBeamAngle(float angl, float step = 1) {
        float swingSpd = swingSpeed() * timeStep() * step;
        float angleChange = angl - beamRotation;
        if (angleChange >= 360) {
            angleChange -= 360;
        }
        if (angleChange < 0) {
            angleChange += 360;
        }
        float angle = beamRotation;
        float absChang = Mathf.Abs(angleChange);
        if (absChang > 180) {
            absChang = 360 - absChang;
        }
        if (perent.hasProperties(SpecialProperties.predictive)) {
            swingSpd *= 1 + Mathf.InverseLerp(0, 180, absChang) / 2;
            Debug.Log(1 + Mathf.InverseLerp(0, 180, absChang) / 2);
        }
        if (absChang > swingSpd) {
            if (angleChange < 180) {
                angle += swingSpd;
            } else {
                angle -= swingSpd;
            }
            beamRotation = angle;
            return true;
        } else {
            angle = angl;
            beamRotation = angle;
            return false;
        }
        return false;
    }

    float MoveSpeed() { return (perent.shotSpeed()) * timeStep() / 10; }

    Vector3 Move(Vector2 pos, Vector2 want, float step = 1) {
        float speed = MoveSpeed() * step;
        Vector2 d = want - pos;
        if (d.magnitude < speed) {
            return want;
        } else {
            return pos + d.normalized * speed;
        }
    }

    public void MoveCenter() {
        if ((center - perent.perent.transform.position).magnitude <
                perent.attackRange() &&
            perent.hasProperties(SpecialProperties.homing)) {
            if (c != null) {
                center = Move(center, c.transform.position, 0.2f);
            } else {
                center = Move(center, perent.perent.transform.position, 0.4f);
            }
        }
    }
    float radOffset() {
        float rad = 0;
        if (perent.perent.totalShots() >= 18) {
            rad += 2 * Mathf.PI * ((float)(number) / perent.weapons.Count);
        } else {
            float funnel = number % perent.perent.funnelShots;
            float cross = number / perent.perent.funnelShots;
            rad +=
                (cross * 2 * Mathf.PI / perent.perent.crossShots) -
                (Mathf.PI * (perent.perent.funnelShots - 1) / Attack.maxShots);
            rad += funnel * 2 * Mathf.PI / Attack.maxShots;
        }
        return rad;
    }

    public override void Update() {
        if (perent.hasProperties(SpecialProperties.random)) {
            beamRotation = Time.time * swingSpeed() / Mathf.PI;
            if (perent.hasProperties(SpecialProperties.homing)) {
                float d = perent.attackRange();
                if (perent.hasProperties(SpecialProperties.returning)) {
                    d *=
                        (Mathf.Sin(Mathf.PI * Time.time * swingSpeed() / 1800) +
                         1) /
                        2;
                }
                float r =
                    Mathf.PI * Time.time * swingSpeed() / 1800 + radOffset();
                Vector3 v = new Vector3(Mathf.Sin(-r) * d, Mathf.Cos(r) * d);
                center = perent.perent.transform.position + v;
            }
        } else {
            if (number == 0) {
                if (c != null) {
                    Vector3 aim;
                    AdjustBeamAngle(
                        World.VecToAngle(c.transform.position - center));
                } else {
                    if (chargingPercent < 1) {
                        chargingPercent += Time.deltaTime / 5;
                    }
                }
            }
            if (perent.hasProperties(SpecialProperties.homing)) {
                MoveCenter();
            }
        }
        ManageBeam();
    }

    public void CollisionUpdate() {
        if (c == null) {
            c = Target(false);
        } else if ((c.transform.position - center).magnitude >
                   1.5 * perent.attackRange()) {
            c = Target(false);
        } else {
            ((SaberAttack)perent).hitTargets.Add(c);
        }
    }

    ParticleSystem.MainModule main;
    public ParticleSystem.CollisionModule collisionModule;
    const float initVel = 10;
    public Beam() {
        beamRotation = 0;
        self = new GameObject("beam");
        self.layer = 7;
        GameObject child = new GameObject();
        child.transform.SetParent(self.transform);
        ParticleSystem ps = child.AddComponent<ParticleSystem>();

        main = ps.main;
        main.startLifetime = 1;
        main.startSpeed = 10;
        main.startSize = 0.8f;
        main.startSize3D = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        collisionModule = ps.collision;
        collisionModule.enabled = true;
        collisionModule.type = ParticleSystemCollisionType.World;
        collisionModule.mode = ParticleSystemCollisionMode.Collision2D;
        collisionModule.bounce = 0.1f;
        collisionModule.sendCollisionMessages = true;

        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = 15;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(0.4f, 0.4f, 0.4f);

        ParticleSystem.LimitVelocityOverLifetimeModule limitVelocity =
            ps.limitVelocityOverLifetime;
        limitVelocity.enabled = true;
        ParticleSystem.MinMaxCurve curve = limitVelocity.limit;
        curve.mode = ParticleSystemCurveMode.Curve;
        curve.curve =
            new AnimationCurve(new Keyframe(0, initVel, -initVel, -initVel),
                               new Keyframe(1, 0, -initVel, -initVel));
        limitVelocity.limit = curve;
        limitVelocity.dampen = 1;

        ParticleSystem.InheritVelocityModule inheritVelocity =
            ps.inheritVelocity;
        inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
        inheritVelocity.enabled = true;

        ParticleSystem.ColorOverLifetimeModule colour = ps.colorOverLifetime;
        colour.enabled = true;
        colour.color =
            new ParticleSystem.MinMaxGradient(Effects.BurnGradient());

        ParticleSystemRenderer pr =
            child.GetComponent<ParticleSystemRenderer>();
        pr.material = Resources.Load<Material>("particle material");

        Rigidbody2D rb = self.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        // BoxCollider2D c = self.AddComponent<BoxCollider2D>();
        // c.size=new Vector2(1,5);
        // c.isTrigger=true;
    }

    public float BeamLength() {
        if (!perent.hasProperties(SpecialProperties.homing)) {
            return perent.attackRange() - offset;
        } else {
            return perent.attackRange();
        }
    }

    public void ManageBeam() {
        Vector3 p = center;
        float d = perent.attackRange() / 2;
        if (!perent.hasProperties(SpecialProperties.homing)) {
            d += offset / 2;
        }
        float r = Mathf.PI * ((Beam)perent.weapons[0]).beamRotation / 180 +
                  radOffset();
        ;
        Vector3 v = new Vector3(Mathf.Sin(-r) * d, Mathf.Cos(r) * d);
        p += v;
        self.transform.position = p;
        Transform child = self.transform.GetChild(0);
        if (perent.hasProperties(SpecialProperties.polar)) {
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        } else {
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
        main.startLifetime = (2f / initVel) * (BeamLength() - 0.35f);
        child.localPosition = new Vector3(0, -BeamLength() / 2);
        child.transform.localEulerAngles = new Vector3(-90, 0, 0);
        // self.transform.localScale=new Vector3(1,BeamLength()/5,1);
        self.transform.eulerAngles = new Vector3(0, 0, r * 180 / Mathf.PI);
    }
}
}
