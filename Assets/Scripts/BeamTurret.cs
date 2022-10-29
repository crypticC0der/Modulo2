using UnityEngine;
using System.Collections.Generic;

namespace Modulo{
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
				RaycastHit2D[] rh = Physics2D.RaycastAll(perent.transform.position,d,attackRange(),perent.layerMask(false));
				LinkedList<Damageable> damageables = new LinkedList<Damageable>();
				int len=0;
				if(( attackProperties() & SpecialProperties.homing )!=0){
					len=rh.Length;
					for(int i=0;i<rh.Length;i++){
						Damageable hit = rh[i].collider.GetComponent<Damageable>();
						if(hit!=null){
							damageables.AddLast(hit);
						}
					}
				}else{
					len=1;
					Collider2D[] rayCols=new Collider2D[rh.Length];
					for(int i=0;i<rh.Length;i++){
						rayCols[i] = rh[i].collider;
					}
					Collider2D best = BestCollider(rayCols);
					damageables.AddLast(best.GetComponent<Damageable>());
				}
				rdt=basicBeam(new Color(.96f,0.66f,.72f),new Color(1,.5f,.3f),damageables,len,new DamageData{dmg=damage()*2,direction=-d,sender=perent});
			}
		}


		public BeamAttack() : base()
		{
			range = 10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 1;
			disabledProps|=SpecialProperties.predictive & SpecialProperties.returning;
		}
	}
}
