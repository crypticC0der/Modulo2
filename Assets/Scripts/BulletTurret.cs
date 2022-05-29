using UnityEngine;

public class BulletAttack : RangedAttack
{
	public override void AtFunc(GameObject o)
	{
		ProcOnCollsion p = basicBullet(this,"assets/hook");
		p.gameObject.GetComponent<Rigidbody2D>().velocity = (o.transform.position - perent.transform.position).normalized*5;
        p.p = impact.Go(damage(), this);
    }

    public BulletAttack() : base()
    {
        range = 10;
        timerMax = 0.5f;
        procCoefficent = 1;
        dmg = 50;
        impact = new BulletProc();
    }
}

public class BulletProc : Proc
{
    public override void OnProc(GameObject g)
    {
		Damageable d = g.GetComponent<Damageable>();
		if(d!=null){
			d.TakeDamage(dmg);
		}
    }


    public BulletProc()
    {
        procCoefficent = 1;
        chance = 1;
        dmgMultiplier = 1;
    }

    public override Proc Go(float d, Attack perent)
    {
        Proc p = new BulletProc();
        p.dmg = d*dmgMultiplier;
        p.perent = perent;
        return p;
    }
}
