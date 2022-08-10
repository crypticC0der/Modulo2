using UnityEngine;

namespace Modulo{
	public enum DebuffName{
		Burning=1,
	}

	///<summary>
	///debuffs are not procs, procs do a thing on hit
	///debuffs stay attached to an object for a bit
	///debuffs also dont care about the damage the attack did
	///</summary>
	public abstract class Debuff{
		public DebuffName name;
		public Damageable perent;
		public float timeLeft;
		public int stacks=1;
		public float chance=1;

		public abstract void Apply(Damageable d);
		public virtual bool onUpdate(){
			timeLeft-=Time.deltaTime;
			if(timeLeft<0){
				onEnd();
				return false;
			}
			return true;
		}

		public void onHit(){}
		public void onEnd(){
			perent.appliedDebuffs^=name;
		}
	}

	public class Burning:Debuff{
		const int dmg=30;

		public override void Apply(Damageable d){
			d.ApplyDebuff<Burning>();
		}

		public Burning(){
			timeLeft=3;
			name=DebuffName.Burning;
		}

		public override bool onUpdate(){
			perent.TakeDamage(new DamageData{dmg=dmg*stacks*Time.deltaTime});
			return base.onUpdate();
		}
	}
}
