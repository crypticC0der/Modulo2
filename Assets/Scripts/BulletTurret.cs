using UnityEngine;

namespace Modulo{
	public class BulletAttack : RangedAttack
	{
		public override void AddAttack(Combatant c)
		{
			c.AddAttack<BulletAttack>();
		}

		public override Vector3 AtFunc(GameObject o)
		{
			Vector3 v = AttackProjection(o);
			AtFunc(v);
			return v;
		}
		public override void AtFunc(Vector3 d)
		{
			ProcOnCollsion p = basicBullet(this,"assets/hook",(attackProperties() & SpecialProperties.homing)!=0);
			p.gameObject.GetComponent<Rigidbody2D>().velocity = (d).normalized*shotSpeed();
			p.p = impact.Go(damage(), this);
		}


		public BulletAttack() : base()
		{
			range = 10;
			shotSpd=10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 1;
			impact = new BulletProc();
		}
	}

	public class BulletProc : Proc
	{
		public override void OnProc(Damageable d)
		{
			base.OnProc(d);
			d.TakeDamage(new DamageData{dmg=dmg,sender=perent.perent,direction=d.transform.position - collider.position});
		}

		public BulletProc()
		{
			procCoefficent = 1;
			chance = 1;
			dmgMultiplier = 1;
		}

		public override Proc newSelf(){
			return new BulletProc();
		}
	}
}
