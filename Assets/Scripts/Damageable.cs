using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo {

public enum EntityTypes {
    Module = 0,
    Defence = 1,
    Turret = 2,
    Orb = 3,
    Player = 4,
    Enemy = 5,
    Other = 6
}

public class HealthBar : MonoBehaviour, Bar {
    public SpriteRenderer backdrop;
    public SpriteRenderer bar;
    public static Sprite[] barSprites;
    public static float[] intervals = new float[] { 0.3f, 0.4f };
    public static Color[] colors =
        new Color[] { new Color(12 / 16f, 3 / 16f, 1 / 32f),
                      new Color(3 / 16f, 13 / 16f, 2 / 16f),
                      new Color(13 / 16f, 17 / 32f, 1 / 8f),
                      new Color(1 / 8f, 1 / 8f, 1 / 8f) };
    float current = 1;
    float aim = 1;
    float transition = 0;
    public void Start() {
        backdrop = new GameObject("healthBar").AddComponent<SpriteRenderer>();
        backdrop.sprite = barSprites[0];
        backdrop.transform.localPosition = new Vector3(0, 1, -1);
        bar = new GameObject("health").AddComponent<SpriteRenderer>();
        bar.sprite = barSprites[1];
        bar.transform.SetParent(backdrop.transform);
        bar.transform.localPosition = new Vector3(0, 0);
        backdrop.transform.localScale = Vector3.one * 2;
        Enable(false);
        if (gameObject.layer == 6) {
            backdrop.color = colors[3];
        }
    }

    public void Reset() { UpdateValue(1, 1); }
    public void Zero() { UpdateValue(0, 1); }

    public void Enable(bool b) {
        backdrop.enabled = b;
        bar.enabled = b;
    }

    public void Delete() {
        Destroy(bar.gameObject);
        Destroy(backdrop.gameObject);
    }

    public void UpdateValue(float current, float max) {
        Enable(true);
        aim = current / max;
        transition = 3;
    }

    public Color ColorAt(float v) {
        float t = Mathf.InverseLerp(intervals[0], intervals[1], v);
        if (gameObject.layer == 6) {
            return Color.LerpUnclamped(colors[2], colors[0], t);
        } else {
            return Color.LerpUnclamped(colors[0], colors[1], t);
        }
    }

    public void SetValue(float current, float max) {
        Vector3 s = bar.transform.localScale;
        float v = Mathf.Clamp(current / max, 0,1);
        s.x = v;
        bar.color = ColorAt(v);
        Vector3 p = bar.transform.localPosition;
        p.x = (1 - v) / 2;
        bar.transform.localScale = s;
        bar.transform.localPosition = p;
    }

    public void Update() {
        backdrop.transform.position =
            transform.position + Vector3.up - 5 * Vector3.forward;
        if (transition > Time.deltaTime) {
            current += (aim - current) / transition;
            SetValue(current, 1);
        } else if (transition - Time.deltaTime < -0.5f && transition > -0.5f) {
            current = aim;
            SetValue(aim, 1);
            Enable(false);
        }
        transition -= Time.deltaTime;
    }
}

public enum Priority {
    Orb = 4,
    Turret = 3,
    Combatant = 2,
    Module = 1,
    Other = 0
}

public interface HasMask{
    GameObject gameObject {get;}
    public int slm() {
        // dont hit self or bullet, everythign else is fair game
        return ~((1 << gameObject.layer) | (1 << 7));
    }
}

public class Damageable : MonoBehaviour,HasMask {
    public static string[] EntityNames =
        new string[7] { "Module", "Defence", "Turret", "Orb",
                        "Player", "Enemy",   "Other" };
    public float health=0;
    public float maxHealth = 10;
    public float regen = 0;       // rate of regen
    public bool regening = false; // means it unconditionally regens
    public float timeSinceDmg = 0;
    public DebuffName appliedDebuffs;
    public EntityTypes type = EntityTypes.Other;
    [SerializeReference]
    public List<Debuff> debuffs = new List<Debuff>();
    public Bar b;
    public Priority priority;

    public int simpleLayerMask()=>(this as HasMask).slm();

    public float UniversalSpeedCalculator(float speed) {
        if (HasDebuff(DebuffName.Stunned)) {
            return 0;
        } else {
            float s = speed;
            Debuff slow;
            if ((slow = FindDebuff(DebuffName.Slowed)) != null) {
                s /= 2 * slow.stacks;
            }
            return s;
        }
    }

    public static implicit operator bool(Damageable d) => d!=null && d.health!=0;

    protected virtual void Start() {
        switch (type) {
        case EntityTypes.Module:
            priority = Priority.Module;
            break;
        case EntityTypes.Turret:
            priority = Priority.Turret;
            break;
        case EntityTypes.Orb:
            priority = Priority.Orb;
            break;
        case EntityTypes.Player:
        case EntityTypes.Enemy:
            priority = Priority.Combatant;
            break;
        default:
            priority = Priority.Other;
            break;
        }

        regen = maxHealth / 10;
        health = maxHealth;
        if (type != EntityTypes.Player) {
            b = (Bar)gameObject.AddComponent<HealthBar>();
        }
    }

    ///< summary>
    /// the versions of this are, one where the debuff is specified by a type
    /// and another where its specified by an instance both have versions where
    /// you can specifiy the number of stacks in ApplyDebuff(newDebuff) it
    /// assumes the stacks is newDebuff.stacks
    ///</summary>
    public void ApplyDebuff<D>(Combatant applier, float stacks)
        where D : Debuff, new() {
        D newDebuff = new D();
        if (newDebuff.mode == DebuffMode.STACKING) {
            newDebuff.stacks = stacks;
        } else {
            newDebuff.timeLeft *= stacks;
        }
        ApplyDebuff(newDebuff, applier);
    }
    public void ApplyDebuff(Debuff newDebuff, Combatant applier) {
        // do i have the debuff
        Debuff foundDebuff = FindDebuff(newDebuff.name);
        if (foundDebuff != null) {
            // apply stack
            foundDebuff.stacks += newDebuff.stacks;
            foundDebuff.timeLeft = newDebuff.timeLeft;
            foundDebuff.onHit(applier);
            return;
        } else {
            // add debuff
            appliedDebuffs |= newDebuff.name;
            newDebuff.perent = this;
            newDebuff.onHit(applier);
            debuffs.Add(newDebuff);
        }
    }
    public void ApplyDebuff(Debuff newDebuff, Combatant applier, float stacks) {
        newDebuff.stacks = stacks;
        ApplyDebuff(newDebuff, applier);
    }

    public Debuff FindDebuff(DebuffName dn) {
        if ((appliedDebuffs & dn) != 0) {
            foreach (Debuff d in debuffs) {
                if (d.name == dn) {
                    return d;
                }
            }
        }
        return null;
    }

    public bool HasDebuff(DebuffName dn) { return (appliedDebuffs & dn) != 0; }

    public virtual void Die() {
        b.Delete();

        foreach (Debuff d in debuffs) {
            d.onEnd();
        }

        switch (type) {
        case EntityTypes.Module:
            IsItem it = GetComponent<IsItem>();
            Module m = (Module)GetComponent<IsItem>().item;
            List<Damageable> nearbyTurrets = m.GetNearby(transform.position);
            foreach (Damageable turret in nearbyTurrets) {
                m.onRemove((Turret)turret, it);
            }
            break;
        default:
            break;
        }

        if (type == EntityTypes.Orb) {
            Debug.Log("GameOver");
        }

        if ((int)type < 3) {
            IsItem it = GetComponent<IsItem>();
            Component.SpawnComponent(Component.Id.Blue,
                                          (int)Mathf.Floor(it.item.power),
                                          transform.position);
            World.UpdateState(HexCoord.NearestHex(transform.position),
                              NodeState.wall, ChangeStateMethod.Off);

            if (type == EntityTypes.Turret) {
                float maxRange = 0;
                foreach (Attack a in ((Combatant)(this)).attacks) {
                    float r = a.attackRange();
                    if (r > maxRange) {
                        maxRange = r;
                    }
                }
                World.UpdateState(HexCoord.NearestHex(transform.position),
                                  NodeState.targeted, ChangeStateMethod.Off,
                                  maxRange);
            }

        }
        GameObject.Destroy(gameObject);
    }

    protected virtual void RunDebuffs() {
        int i = 0;
        while (i < debuffs.Count) {
            if (debuffs[i].onUpdate()) {
                // debuff ran succesfully
                i++;
            } else {
                // debuff is over
                debuffs.RemoveAt(i);
            }
        }
    }

    public virtual void Regen() {
        if (health < maxHealth) {
            if (timeSinceDmg > 1 || regening) {
                health += regen * Time.deltaTime;
                b.UpdateValue(health, maxHealth);
            }
            timeSinceDmg += Time.deltaTime;
        } else if (health != maxHealth) {
            health = maxHealth;
            b.UpdateValue(1, 1);
        }
    }

    public virtual void FixedUpdate() {
        RunDebuffs();
        if (regening || regen != 0) {
            Regen();
        }
    }

    public virtual void TakeDamage(DamageData d) {
        // if (d.sender != null) {
        //     if (d.dmg > health && d.sender.type == EntityTypes.Enemy) {
        //         ((EnemyFsm)d.sender).LevelUp();
        //     }
        // }
        if(health<=0){return;}
        timeSinceDmg = 0;
        health -= d.dmg;
        if (health <= 0) {
            health = 0;
            Die();
        }
        b.UpdateValue(health, maxHealth);
    }
}

public class DamageData {
    public float dmg;
    public Combatant sender = null;
    public Vector3 direction = Vector3.zero;
    public DamageProperties properties = 0;
}

public enum DamageProperties { bypassArmor = 1 }
}
