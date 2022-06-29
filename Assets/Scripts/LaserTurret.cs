using UnityEngine;

public class LaserAttack : RangedAttack
{
    public override void AddAttack(Combatant c)
    {
        c.AddAttack<LaserAttack>();
    }

	public override Vector3 AtFunc(GameObject o)
	{
		Vector3 v = o.transform.position-perent.transform.position;
		AtFunc(v);
		return v;
    }
	public override void AtFunc(Vector3 d)
	{
		RaycastHit2D[] rh = Physics2D.RaycastAll(perent.transform.position,d,attackRange(),layerMask());
		Vector3 maxDistance=new Vector3(0,0,0);
		for(int i=0;i<rh.Length;i++){
			Damageable hit = rh[i].collider.GetComponent<Damageable>();
			if(hit!=null){
				DmgOverhead(new DamageData{dmg=damage(),direction=-d},hit);
			}
			Vector3 distance = rh[i].transform.position - perent.transform.position;
			if(distance.magnitude>=maxDistance.magnitude){
				maxDistance=distance;
			}
		}
		if(maxDistance.magnitude==0){maxDistance=d;}
		maxDistance+=perent.transform.position;
		basicRay(new Color(1,0.5f,0),new Color(1,0,0),new Vector3[]{perent.transform.position,maxDistance});
    }


    public LaserAttack() : base()
    {
        range = 10;
        timerMax = 0.5f;
        procCoefficent = 1;
        dmg = 50;
    }
}
