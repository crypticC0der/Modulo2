using UnityEngine;
using System.Collections.Generic;

namespace Modulo{
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
			List<Vector3> v3 = new List<Vector3>();
			Vector3 lastHit=perent.transform.position;
			v3.Add(lastHit);
			float tlen=0;
			void DealWithHit(Collider2D c){
				Damageable hit = c.GetComponent<Damageable>();
				Vector3 d =c.transform.position-lastHit;
				if(hit!=null){
					DmgOverhead(new DamageData{dmg=damage(),direction=d},hit);
				}
				v3.Add(c.transform.position);
				lastHit=c.transform.position;
				tlen+=d.magnitude;
				if(tlen>attackRange()){
					basicRay(new Color(1,0.5f,0),new Color(1,0,0),v3.ToArray());
					return;
				}
			}
			if(( attackProperties() & SpecialProperties.homing )==0){
				RaycastHit2D[] rh = Physics2D.RaycastAll(perent.transform.position,d,attackRange(),perent.layerMask(false));
				for(int i=0;i<rh.Length;i++){
					DealWithHit(rh[i].collider);
				}
			}else{
				Collider2D[] cols = Physics2D.OverlapCircleAll(perent.transform.position,attackRange(),perent.layerMask(false));
				for(int i=0;i<cols.Length;i++){
					Vector3 dir = cols[i].transform.position-perent.transform.position;
					if(Vector2.Dot(dir,d)>0.5f){
						DealWithHit(cols[i]);
					}
				}
			}
			if(v3.Count==1){
				v3.Add(d*attackRange());
			}
			basicRay(new Color(1,0.5f,0),new Color(1,0,0),v3.ToArray());
		}


		public LaserAttack() : base()
		{
			range = 10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 1;
			disabledProps|=SpecialProperties.predictive & SpecialProperties.returning;
		}
	}
}
