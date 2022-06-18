using UnityEngine;

public class BulletAttack : RangedAttack
{
    public override void AddAttack(Combatant c)
    {
        c.AddAttack<BulletAttack>();
    }

	public override Vector3 AtFunc(GameObject o)
	{
		Vector3 d =o.transform.position-perent.transform.position;
		AtFunc(d);
		return d;
    }
	public override void AtFunc(Vector3 d)
	{
		ProcOnCollsion p = basicBullet(this,"assets/hook");
		p.gameObject.GetComponent<Rigidbody2D>().velocity = (d).normalized*shotSpeed();
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
    public override void OnProc(Damageable d)
    {
		base.OnProc(d);
		d.TakeDamage(dmg,perent.perent,d.transform.position - collider.transform.position);
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
