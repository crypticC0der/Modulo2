using UnityEngine;

public class SpikeAttack : RangedAttack
{
    public override void AddAttack(Combatant c)
    {
        c.AddAttack<SpikeAttack>();
    }

	public override Vector3 AtFunc(GameObject o)
	{
		return Vector3.zero;
    }
	public override void AtFunc(Vector3 d)
	{
		ProcOnCollsion p  = basicSpike(this,"assets/spike",d,(attackProperties() & SpecialProperties.homing)!=0);
        p.p = impact.Go(damage(), this);
    }


    public SpikeAttack() : base()
    {
        range = 3;
        timerMax = 1f;
        procCoefficent = 0.2f;
		attackPeirce=2;
        dmg = 10;
		attackProps|=SpecialProperties.random;
        impact = new SpikeProc();
    }
}

public class SpikeProc : Proc
{
    public override void OnProc(Damageable d)
    {
		base.OnProc(d);
		d.TakeDamage(new DamageData{dmg=dmg,sender=perent.perent,direction=d.transform.position - collider.position});
    }

    public SpikeProc()
    {
        procCoefficent = 1;
        chance = 1;
        dmgMultiplier = 1;
    }

    public override Proc Go(float d, Attack perent)
    {
        Proc p = new SpikeProc();
        p.dmg = d*dmgMultiplier;
        p.perent = perent;
        return p;
    }
}
