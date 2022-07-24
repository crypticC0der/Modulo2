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
			rdt=basicBeam(new Color(.96f,0.66f,.72f),new Color(1,.5f,.3f),damageables,new DamageData{dmg=damage()*2,direction=-d,sender=perent});
		}
    }


    public BeamAttack() : base()
    {
        range = 10;
        timerMax = 0.5f;
        procCoefficent = 1;
        dmg = 50;
    }
}
