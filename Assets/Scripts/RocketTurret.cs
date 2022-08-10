using UnityEngine;

namespace Modulo{
	public class RocketAttack : RangedAttack
	{
		public override void AddAttack(Combatant c)
		{
			c.AddAttack<RocketAttack>();
		}

		public override Vector3 AtFunc(GameObject o)
		{
			Vector3 v = AttackProjection(o);
			AtFunc(v);
			return v;
		}
		public override void AtFunc(Vector3 d)
		{
			ProcOnCollsion p = basicBullet(this,"assets/rocket",(attackProperties() & SpecialProperties.homing)!=0);
			p.gameObject.GetComponent<Rigidbody2D>().velocity = (d).normalized*shotSpeed();
			p.p = impact.Go(damage(), this);
		}


		public RocketAttack() : base()
		{
			range = 10;
			timerMax = 1;
			procCoefficent = 1;
			dmg = 50;
			attackProps|=SpecialProperties.homing;
			impact = new RocketProc();
		}
	}

	public class RocketProc : Proc
	{
		public override void OnProc(Damageable d)
		{
			base.OnProc(d);
			Collider2D[] hits = Physics2D.OverlapCircleAll(d.transform.position,3,perent.perent.layerMask(false));
			foreach(Collider2D col in hits){
				Damageable damageable = col.GetComponent<Damageable>();
				if(damageable){
					damageable.TakeDamage(new DamageData{dmg=dmg,sender=perent.perent,direction=d.transform.position - collider.position,properties=DamageProperties.bypassArmor});
				}
			}
		}

		public RocketProc()
		{
			procCoefficent = 1;
			chance = 1;
			dmgMultiplier = 1;
		}

		public override Proc Go(float d, Attack perent)
		{
			Proc p = new RocketProc();
			p.dmg = d*dmgMultiplier;
			p.perent = perent;
			return p;
		}
	}
}
