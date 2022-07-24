using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
	Ranged,
	Close,
	Random,
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

	public LineRenderer MinimalRay(Color startColor,Color endColor,Vector3[] points){
		GameObject g = new GameObject();
		g.layer=7;
		LineRenderer line = g.AddComponent<LineRenderer>();
		line.material=Resources.Load<Material>("RayMaterial");
		line.positionCount=points.Length;
		line.SetPositions(points);
		line.startColor=startColor;
		line.endColor=endColor;
		line.startWidth=0.05f;
		line.useWorldSpace=true;
		line.numCapVertices=3;
		return line;
	}


	public RayDespawnTracking basicBeam(Color startColor,Color endColor,Damageable[] aims,DamageData d){
		Vector3[] points = new Vector3[aims.Length+1];
		points[0]=perent.transform.position;
		for(int i=0;i<aims.Length;i++){
			points[i+1]=aims[i].transform.position;
		}
		LineRenderer line = MinimalRay(startColor,endColor,points);
		RayDespawnTracking rdt = line.gameObject.AddComponent<RayDespawnTracking>();
		rdt.lineRenderer=line;
		rdt.length= (points[0]-points[points.Length-1]).magnitude;
		rdt.data =d;
		rdt.aims=aims;
		rdt.perent=this;
		return rdt;
	}

	public Despawn basicRay(Color startColor,Color endColor,Vector3[] points){
		LineRenderer line = MinimalRay(startColor,endColor,points);
		RayTracking d = line.gameObject.AddComponent<RayTracking>();
		d.deathTimer=0.25f;
		d.lineRenderer=line;
		d.perent=perent.transform;
		d.FixedUpdate();
		return d;
	}

	public static GasBomb basicGas(Attack attack,string assetPath,bool homing=false,float scale=1){
		GameObject g = minimalBullet<CircleCollider2D>(attack,assetPath,homing,scale);
		GasBomb gb = g.AddComponent<GasBomb>();
        gb.perentLayer=attack.layerMask();
		float spd=attack.shotSpeed();
		gb.initialDensity=(attack.peirce()+5)/10f;
		gb.lossTime=spd*gb.initialDensity*4/attack.attackRange();
		return gb;
	}

	public static ProcOnCollsion basicSpike(Attack attack,string assetPath,bool homing=false,float scale=1){
		GameObject g = minimalBullet<CircleCollider2D>(attack,assetPath,homing,scale,RigidbodyType2D.Static);
		SpikeDespawn d =g.AddComponent<SpikeDespawn>();
		d.deathTimer=60;
		d.spd= attack.shotSpeed();
		ProcOnCollsion p = g.AddComponent<ProcOnCollsion>();
		p.peirce=attack.peirce();
        p.perentLayer=attack.layerMask();
		return p;
	}

	public static ProcOnCollsion basicBullet(Attack attack,string assetPath,bool homing=false,float scale=1){
		GameObject g = minimalBullet<BoxCollider2D>(attack,assetPath,homing,scale);
		g.AddComponent<Despawn>().deathTimer=2*(attack.attackRange()/attack.shotSpeed());
		ProcOnCollsion p = g.AddComponent<ProcOnCollsion>();
		p.peirce=attack.peirce();
        p.perentLayer=attack.layerMask();
		return p;
	}

	public static GameObject minimalBullet<C>(Attack attack,string assetPath,bool homing,float scale,RigidbodyType2D rbt=RigidbodyType2D.Kinematic) where C:Collider2D{
		GameObject g = new GameObject();
		g.layer=7;
		g.transform.position=attack.perent.transform.position;
		g.transform.localScale=new Vector3(scale,scale,1);
		SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>(assetPath);
        g.AddComponent<BoxCollider2D>().isTrigger=true;
		Rigidbody2D r = g.AddComponent<Rigidbody2D>();
		r.bodyType=rbt;
		if(homing){
			HomingBullet b = g.AddComponent<HomingBullet>();
			b.rb=r;
			b.layerMask=attack.layerMask();
			b.spd=attack.shotSpeed();
		}
		r.mass=1;
		return g;
	}

	public int layerMask(){
		return ~((1<<perent.gameObject.layer) | (1<<7));
	}

	public virtual void DmgOverhead(DamageData d,Damageable hit){
		d.sender=perent;
		hit.TakeDamage(d);
		perent.RunProc(procCoefficent,this,d.dmg,hit);
		perent.ApplyDebuffs(procCoefficent,hit);
	}

	abstract public void Update();
	abstract public Vector3 AtFunc(GameObject g);
	abstract public void AddAttack(Combatant c);

	public AttackType t;
	public Combatant perent;
	public float timerMax;
	protected float timer=3;
	public float range = 1;
	public float shotSpd=5;
	public int attackPeirce=0;
	public SpecialProperties attackProps=0;

	public virtual float damage(){return (dmg+perent.dmgPlus)*perent.dmgMultipler;}
	public virtual float attackRate(){return perent.attackSpeed + timerMax/perent.attackRate;}
	public virtual float attackRange(){return perent.range*range;}
	public virtual float shotSpeed(){return perent.shotSpeed*shotSpd;}
	public virtual int peirce(){return perent.peirce+attackPeirce;}
	public virtual SpecialProperties attackProperties(){return attackProps | perent.specialProperties;}
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
	abstract public Proc Go(float d, Attack perent);
	public Attack perent;
	public float chance;
	public float dmgMultiplier;
	public Transform collider;

}


/// <summary>
/// this allows you to add the attack to an object
/// </summary>
public class ProcOnCollsion : MonoBehaviour
{
	public int peirce=0;
	public Proc p;
	public int perentLayer;
	public void OnTriggerEnter2D(Collider2D c)
	{
		if ((1<<c.gameObject.layer & perentLayer)!=0)
		{
			Damageable d = c.GetComponent<Damageable>();
			if(d!=null){
				p.collider=transform;
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

	public virtual Vector3 AttackProjection(GameObject g){
		Vector3 d =g.transform.position-perent.transform.position;
		return d;

	}

	public RangedAttack()
	{
		t = AttackType.Ranged;
	}

	public Collider2D Target()
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
			bool hit=false;
			Vector3 d=Vector2.zero;
			if((attackProperties()& SpecialProperties.random)!=0){
				hit=true;
				float theata = Random.Range(0,2*Mathf.PI);
				float r = (attackRange()-0.7f)*(Mathf.Sqrt(Random.value))+0.7f; //uniform probabilities
				d = new Vector3(r*Mathf.Cos(theata),r*Mathf.Sin(theata));
				AtFunc(d);
			}
			else if ((c = Target()) != null) {
				d = AtFunc(c.gameObject);
				hit=true;
			}
			if(hit){
				if((attackProperties()& SpecialProperties.crossShot)!=0){
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

public abstract class AreaAttack : Attack
{
	public new int peirce()
	{
		int r = base.peirce();
		if ((attackProperties()& SpecialProperties.crossShot) != 0){
			return 4*r;
		}else{
			return r;
		}
    }

    public AreaAttack()
	{
        t = AttackType.Close;
    }
    public Collider2D[] Target()
	{
		return Physics2D.OverlapCircleAll(perent.transform.position, attackRange(), layerMask());
	}
	public override void Update()
	{
		if (timer <= 0)
		{
			Collider2D[] c=Target();
			int p = peirce();
			for(int i=0;i<c.Length&&i<p;i++){
				AtFunc(c[i].gameObject);
			}
			timer = attackRate()+ perent.attackSpeed;
		}
		timer -= Time.deltaTime * perent.attackRate;
	}
}

public abstract class CloseAttack : Attack
{
	public new float damage()
	{
		float d = base.damage();
		if ((attackProperties()& SpecialProperties.crossShot) != 0){
			return 4*d;
		}else{
			return d;
        }
    }

    public CloseAttack()
	{
        t = AttackType.Close;
    }

    public Collider2D Target()
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
