using UnityEngine;

namespace Modulo{
	public class GasAttack : RangedAttack
	{
		public override void AddAttack(Combatant c)
		{
			c.AddAttack<GasAttack>();
		}

		public override Vector3 AtFunc(GameObject o)
		{
			Vector3 v = AttackProjection(o);
			AtFunc(v);
			return v;
		}
		public override void AtFunc(Vector3 d)
		{
			GasBomb p = basicGas(this,"assets/gas",(attackProperties() & SpecialProperties.homing)!=0);
			p.gameObject.GetComponent<Rigidbody2D>().velocity = (d).normalized*shotSpeed();
			p.perent = impact.Go(damage(), this);
		}


		public GasAttack() : base()
		{
			range = 10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 50;
			impact = new GasProc();
		}
	}

	public class GasProc : Proc
	{
		public override void OnProc(Damageable d)
		{
			base.OnProc(d);
			d.TakeDamage(new DamageData{dmg=dmg,sender=perent.perent,direction=d.transform.position - collider.position});
		}

		public GasProc()
		{
			procCoefficent = 1;
			chance = 1;
			dmgMultiplier = 1;
		}

		public override Proc Go(float d, Attack perent)
		{
			Proc p = new GasProc();
			p.dmg = d*dmgMultiplier;
			p.perent = perent;
			return p;
		}
	}
}
