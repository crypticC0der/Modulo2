using UnityEngine;

public enum DebuffName{
	Burning=1,
}

///debuffs are not procs, procs do a thing on hit
///debuffs stay attached to an object for a bit
public abstract class Debuff{
	public DebuffName name;
	public Damageable perent;
	public float timeLeft;
	public int stacks=1;
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
	public new DebuffName name=DebuffName.Burning;
	const int dmg=1;

	public Burning(){
		timeLeft=3;
	}

	public override bool onUpdate(){
		perent.TakeDamage(dmg*stacks*Time.deltaTime);
		return base.onUpdate();
	}
}
