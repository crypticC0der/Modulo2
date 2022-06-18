using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
	Ranged,
	Close,
	Med,
	Area
}


/// <summary>
/// damager i barely know her
/// (lamo)
/// </summary>
public abstract class Damager
{
	public float procCoefficent;
	public float dmg;
}

/// <summary>
/// this is basically a contained fsm that can deal damage and target enemies independantly
/// </summary>
public abstract class Attack : Damager
{
	public static ProcOnCollsion basicBullet(Attack attack,string assetPath){
		GameObject g = new GameObject();
		g.layer=7;
		g.transform.position=attack.perent.transform.position;
		SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>(assetPath);
        g.AddComponent<CircleCollider2D>().isTrigger=true;
		Rigidbody2D r = g.AddComponent<Rigidbody2D>();
		r.bodyType=RigidbodyType2D.Kinematic;
		r.mass=1;
		g.AddComponent<Despawn>().deathTimer=3*(attack.attackRange()/10);
		ProcOnCollsion p = g.AddComponent<ProcOnCollsion>();
        p.perentLayer=attack.perent.gameObject.layer;
		return p;
	}

	protected int layerMask(){
		return ~((1<<perent.gameObject.layer) | (1<<7));
	}

	abstract public void Update();
	abstract public Collider2D Target();
	abstract public Vector3 AtFunc(GameObject g);
	abstract public void AddAttack(Combatant c);

	public AttackType t;
	public Combatant perent;
	public float timerMax;
	protected float timer=1;
	public float range = 1;
	public float shotSpd=5;

	public float damage(){return (dmg+perent.dmgPlus)*perent.dmgMultipler;}
	public float attackRate(){return perent.attackSpeed + timerMax/perent.attackRate;}
	public float attackRange(){return perent.range*range;}
	public float shotSpeed(){return perent.shotSpeed+shotSpd;}
}

/// <summary>
/// basically this is a hook you can apply to an object woo
/// dmg in this system is just a dmg*dmgMultiplier
/// </summary>
abstract public class Proc : Damager
{
	///<summary>
	///runs the proc function on a gameobject
	///</summary>
	public virtual void OnProc(Damageable d){
		perent.perent.RunProc(procCoefficent,perent,dmg,d);
		perent.perent.ApplyDebuffs(procCoefficent,d);
	}

	///<summary>
	///makes a new proc for an attack
	///</summary>
	abstract public Proc Go(float dmg, Attack perent);
	public Attack perent;
	public float chance;
	public float dmgMultiplier;
	public ProcOnCollsion collider;

}
/// <summary>
/// makes a object destroy itself after a couple seconds
/// </summary>
public class Despawn: MonoBehaviour
{
	public float deathTimer=0;
	public void FixedUpdate()
	{
		deathTimer-=Time.deltaTime;
		if(deathTimer<0){
			Destroy(gameObject);
		}
	}

}


/// <summary>
/// this allows you to add the attack to an object
/// </summary>
public class ProcOnCollsion : MonoBehaviour
{
	public int peirce;
	public Proc p;
	public int perentLayer;
	public void OnTriggerEnter2D(Collider2D c)
	{
		if (c.gameObject.layer != perentLayer && c.gameObject.layer!=this.gameObject.layer)
		{
			Damageable d = c.GetComponent<Damageable>();
			if(d!=null){
				p.collider=this;
				p.OnProc(d);
				peirce--;
				if(peirce<0){
					Destroy(this.gameObject);
				}
			}
		}
	}

}

public abstract class RangedAttack : Attack
{
	public Proc impact;
	abstract public void AtFunc(Vector3 p);

	public RangedAttack()
	{
		t = AttackType.Ranged;
	}

	public override Collider2D Target()
	{
		Collider2D[] o = Physics2D.OverlapCircleAll(perent.transform.position, attackRange(),layerMask());
		foreach (Collider2D c in o)
		{
			if (Physics2D.Raycast(perent.transform.position, c.transform.position - perent.transform.position)
				&& c.gameObject != perent.gameObject)
			{
				return c;
			}
		}
		return null;
	}
	public override void Update()
	{
		if (timer <= 0)
		{
			Collider2D c;
			if ((c = Target()) != null)
			{
				Vector3 d = AtFunc(c.gameObject);
				if((perent.specialProperties & SpecialProperties.crossShot)!=0){
					float t;
					for(int i=0;i<3;i++){
						t=-d.y;
						d.y=d.x;
						d.x=t;
						AtFunc(d);
					}
				}
				timer = attackRate()+ perent.attackSpeed;
			}
		}
		timer -= Time.deltaTime * perent.attackRate;
	}
}

public abstract class CloseAttack : Attack
{
	public new float damage()
	{
		if ((perent.specialProperties & SpecialProperties.crossShot) != 0){
			return 4*(dmg + perent.dmgPlus) * perent.dmgMultipler;
		}else{
			return (dmg + perent.dmgPlus) * perent.dmgMultipler;
        }
    }

    public CloseAttack()
	{
        t = AttackType.Close;
    }

    public override Collider2D Target()
	{
		return Physics2D.OverlapCircle(perent.transform.position, attackRange(), layerMask());
	}
	public override void Update()
	{
		if (timer <= 0)
		{
			Collider2D c;
			if ((c = Target()) !=null)
			{
				AtFunc(c.gameObject);
				timer = attackRate()+ perent.attackSpeed;
			}
		}
		timer -= Time.deltaTime * perent.attackRate;
	}
}
