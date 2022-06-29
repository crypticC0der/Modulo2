using UnityEngine;

public class BeamAttack : RangedAttack
{
	RayDespawnTracking rdt;
    public override void AddAttack(Combatant c)
    {
        c.AddAttack<BeamAttack>();
    }

	public override Vector3 AtFunc(GameObject o)
	{
		Vector3 v = o.transform.position-perent.transform.position;
		AtFunc(v);
		return v;
    }
	public override void AtFunc(Vector3 d)
	{
		//TODO
		if(!rdt){
			RaycastHit2D[] rh = Physics2D.RaycastAll(perent.transform.position,d,attackRange(),layerMask());
			Damageable[] damageables = new Damageable[rh.Length];
			for(int i=0;i<rh.Length;i++){
				Damageable hit = rh[i].collider.GetComponent<Damageable>();
				if(hit!=null){
					damageables[i]=hit;
				}
			}
			rdt=basicBeam(new Color(1,0.5f,0),new Color(1,0,0),damageables,new DamageData{dmg=damage(),direction=-d,sender=perent});
		}
    }


    public BeamAttack() : base()
    {
        range = 10;
        timerMax = 0.5f;
        procCoefficent = 2;
        dmg = 100;
    }
}
