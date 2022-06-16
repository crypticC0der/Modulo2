using UnityEngine;

public class FlameAttack : RangedAttack
{
	public override Vector3 AtFunc(GameObject o)
	{
		Vector3 d =o.transform.position-perent.transform.position;
		AtFunc(d);
		return d;
    }
	public override void AtFunc(Vector3 d)
	{
		ProcOnCollsion p = basicBullet(this,"assets/hook");
		p.gameObject.GetComponent<Rigidbody2D>().velocity = (d).normalized*5;
        p.p = impact.Go(damage(), this);
    }

    public override void AddAttack(Combatant c)
    {
        c.AddAttack<BulletAttack>();
    }

    public FlameAttack() : base()
    {
        range = 6;
        timerMax = 0.5f;
        procCoefficent = 1;
        dmg = 2;
        impact = new FlameProc();
    }
}

public class BurnProc: Proc{
	public BurnProc(){
		procCoefficent=0;
		dmgMultiplier=0;
		chance=1;
	}

	public override Proc Go(float d, Attack perent){
		Proc p = new BurnProc();
		p.perent=perent;
		return p;
	}

	public override void OnProc(Damageable d){
		// i am not using base. onproc as the coefficent is 0
		d.ApplyDebuff<Burning>();
	}
}

public class FlameProc : Proc
{
    public override void OnProc(Damageable d)
    {
		base.OnProc(d);
		d.TakeDamage(dmg,perent.perent,d.transform.position - collider.transform.position);
    }


    public FlameProc()
    {
        procCoefficent = 0.2f;
        chance = 1;
        dmgMultiplier = 1;
    }

    public override Proc Go(float d, Attack perent)
    {
        Proc p = new FlameProc();
        p.dmg = d*dmgMultiplier;
        p.perent = perent;
        return p;
    }
}
