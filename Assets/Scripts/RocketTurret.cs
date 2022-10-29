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
			dmg = 1;
			attackProps|=SpecialProperties.homing;
			impact = new RocketProc();
		}
	}

	public class RocketProc : Proc
	{
		public override void OnProc(Damageable d)
		{
			Effects.Explode(d.transform.position,perent.damage()/10,this,perent.perent.layerMask(false));
		}

		public RocketProc()
		{
			procCoefficent = 1;
			chance = 1;
			dmgMultiplier = 1;
		}

        public override Proc newSelf()
        {
			return new FlameProc();
		}
	}
}
