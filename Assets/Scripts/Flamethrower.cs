using UnityEngine;

public class FlameAttack : RangedAttack
{
	public override void AtFunc(GameObject o)
	{
		ProcOnCollsion p = basicBullet(this,"assets/hook");
		p.gameObject.GetComponent<Rigidbody2D>().velocity = (o.transform.position - perent.transform.position).normalized*5;
        p.p = impact.Go(damage(), this);
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

public class FlameProc : Proc
{
    public override void OnProc(GameObject g)
    {
		Damageable d = g.GetComponent<Damageable>();
		if(d!=null){
			d.TakeDamage(dmg);
			d.ApplyDebuff<Burning>();
		}
    }


    public FlameProc()
    {
        procCoefficent = 1;
        chance = 1;
        dmgMultiplier = 1;
    }

    public override Proc Go(float d, Attack perent)
    {
		Debug.Log(d);
        Proc p = new FlameProc();
        p.dmg = d*dmgMultiplier;
        p.perent = perent;
        return p;
    }
}
