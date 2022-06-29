using UnityEngine;

public class TeslaAttack : AreaAttack
{
    public override void AddAttack(Combatant c)
    {
        c.AddAttack<TeslaAttack>();
    }

	public override Vector3 AtFunc(GameObject o)
	{
		Vector3 d = o.transform.position-perent.transform.position;
		RaycastHit2D[] rh = Physics2D.RaycastAll(perent.transform.position,d,attackRange(),layerMask());
		if(rh.Length>0){
			Vector3[] hitPos = new Vector3[2*rh.Length+1];
			for(int i=0;i<rh.Length;i++){
				hitPos[2*i+2]=rh[i].transform.position;
				Vector3 direction=rh[i].transform.position-perent.transform.position;
				direction=Random.Range(-0.3f,0.3f)*new Vector3(-direction.y,direction.x);
				Vector3 point = Vector2.LerpUnclamped(perent.transform.position,rh[i].transform.position,Random.Range(0.2f,0.8f));
				hitPos[2*i+1]=direction+point;
				Damageable hit = rh[i].collider.GetComponent<Damageable>();
				if(hit!=null){
					DmgOverhead(new DamageData{dmg=damage(),
											   direction=hitPos[2*i+2]-hitPos[2*i+1],
											   properties=DamageProperties.bypassArmor},
								hit);
				}
			}
			Debug.Log(hitPos.Length);
			hitPos[0]=perent.transform.position;
			basicRay(new Color(0.98f,0.92f,0.68f),new Color(0.98f,0.8f,0.14f),hitPos);
		}
		return d;
    }

    public TeslaAttack() : base()
    {
        range = 3;
        timerMax = 0.25f;
        procCoefficent = 0.2f;
        dmg = 20;
		attackPeirce=3;
    }
}
