using UnityEngine;
using System.Collections.Generic;

namespace Modulo{
	public class TeslaAttack : AreaAttack
	{
		public override void AddAttack(Combatant c)
		{
			c.AddAttack<TeslaAttack>();
		}

		public static Vector3[] Zig(Vector3 start,Vector3 end){
			Vector3[] r = new Vector3[2];
			Vector3 direction=end-start;
			direction=Random.Range(-0.3f,0.3f)*new Vector3(-direction.y,direction.x);
			Vector3 point = Vector2.LerpUnclamped(start,end,Random.Range(0.2f,0.8f));
			r[0] = (direction+point);
			r[1] = (end);
			return r;
		}


		public override void Update()
		{
			greatestHits.Clear();
			base.Update();
		}

		List<Collider2D> greatestHits=new List<Collider2D>();
		public override Vector3 AtFunc(GameObject o)
		{
			Vector3 d = o.transform.position-center;
			RaycastHit2D[] rh = Physics2D.RaycastAll(center,d,attackRange(),perent.layerMask(false));
			List<Vector3> hitPos = new List<Vector3>();
			Vector3 previous=center;

			void HitTarget(Damageable hit){
				Vector3[] zigs =Zig(previous,hit.transform.position);
				hitPos.AddRange(zigs);
				previous=hit.transform.position;
				DmgOverhead(new DamageData{dmg=damage(),
											direction=zigs[1]-zigs[0],
											properties=DamageProperties.bypassArmor},
							hit);
			}

			if(rh.Length>0){
				hitPos.Add(center);
				for(int i=0;i<rh.Length;i++){
					greatestHits.Add(rh[i].collider);
					Damageable hit = rh[i].collider.GetComponent<Damageable>();
					if(hit!=null){
						HitTarget(hit);
					}
				}
				if(( attackProperties() & SpecialProperties.homing )!=0){
					Collider2D[] hits = Physics2D.OverlapCircleAll(previous,2,perent.layerMask(false));
					int c=0;
					while(hits.Length>c && currentPeirce>0){
						if(!greatestHits.Contains(hits[c])){
							Damageable dmg = hits[c].GetComponent<Damageable>();
							if(dmg!=null){
								HitTarget(dmg);
								currentPeirce--;
							}
							hits = Physics2D.OverlapCircleAll(previous,2,perent.layerMask(false));
							c=0;
						}
						c++;
					}
				}
				basicRay(new Color(0.98f,0.92f,0.68f),new Color(0.98f,0.8f,0.14f),hitPos.ToArray());
			}
			return d;
		}

		public TeslaAttack() : base()
		{
			range = 3;
			timerMax = 0.25f;
			procCoefficent = 0.2f;
			dmg = .4f;
			attackPeirce=3;
		}
	}
}
