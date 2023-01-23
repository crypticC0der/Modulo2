using UnityEngine;
using System.Collections;
namespace Modulo {
	public class OnExplode : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			//radius = 2 @ 10x, 4 @ 100x
			float r = Mathf.Pow(2,Mathf.Log10(0.3f + dmg/10));
			r = Mathf.Min(Mathf.Max(r,0.5f),4);
			Effects.Explode(d.transform.position,Mathf.Max(dmg/10,0.3f),this,perent.perent.layerMask(false),r);
		}

		public OnExplode(){
			procCoefficent=0;
			chance=0.5f;
			dmgMultiplier=1f;
		}

		public override Proc newSelf(){
			return new OnExplode();
		}
	}

	public class PullIn : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			float r = 1;
			Collider2D[] c = Physics2D.OverlapCircleAll(d.transform.position,r,perent.perent.layerMask(false));
			foreach(Collider2D col in c){
				if(col.gameObject!=d.gameObject){
					Damageable D = col.GetComponent<Damageable>();
					Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
					if(D && rb){
						float magnitude = -perent.perent.damage()/1.5f;
						Vector2 direction = col.transform.position-d.transform.position;
						direction = direction.normalized*magnitude;
						rb.AddForce(direction,ForceMode2D.Impulse);
					}
				}
			}

		}

		public PullIn(){
			procCoefficent=0;
			chance=0.3f;
			dmgMultiplier=1f;
		}

		public override Proc newSelf(){
			return new PullIn();
		}
	}

	public class LaserBurst : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			float r = 1;
			int lasers = Random.Range(2,4);
			Collider2D[] c = Physics2D.OverlapCircleAll(d.transform.position,r,perent.perent.layerMask(false));
			int i=0;
			int hits=0;
			while(hits<lasers && i < c.Length){
				if(c[i].gameObject!=d.gameObject){
					Damageable D = c[i].GetComponent<Damageable>();
					if(D){
						Vector3[] points = new Vector3[3];
						points[0]=d.transform.position;
						points[1] = TeslaAttack.Zig(d.transform.position,c[i].transform.position)[0];
						points[2]=c[i].transform.position;
						DmgOverhead(new DamageData{dmg=perent.perent.damage()*dmgMultiplier,
												   direction=points[2]-points[1],
												   properties=DamageProperties.bypassArmor},
									D);
						LineRenderer lr = perent.MinimalRay(new Color(0.98f,0.92f,0.68f),new Color(0.98f,0.8f,0.14f),points);
						Despawn despawn = lr.gameObject.AddComponent<Despawn>();
						despawn.deathTimer=0.25f;
						hits++;
					}
				}
				i++;
			}

		}

		public LaserBurst(){
			procCoefficent=0.2f;
			chance=0.3f;
			dmgMultiplier=.7f;
		}

		public override Proc newSelf(){
			return new LaserBurst();
		}
	}

	public class PoisonTrail : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			d.ApplyDebuff<GreenTrail>(perent.perent,(int)(perent.perent.damage()*dmgMultiplier/GreenTrail.dmg));
		}
		public PoisonTrail(){
			procCoefficent=0;
			chance=0.2f;
			dmgMultiplier=0.5f;
		}
		public override Proc newSelf(){
			return new PoisonTrail();
		}
	}

	public class OnExplodeDelay : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			//radius = 2 @ 10x, 4 @ 100x
			int mask = perent.perent.layerMask(false);
			Vector3 p = d.transform.position;
			CoroutineHandler.handler.StartCoroutine(DelayBoom(mask,p,d));
		}

		private IEnumerator DelayBoom(int mask,Vector3 p,Damageable d){
			yield return new WaitForSeconds(0.3f);
			if(d){
				p=d.transform.position;
			}
			float r = Mathf.Pow(2,Mathf.Log10(0.3f + dmg/10));
			r = Mathf.Min(Mathf.Max(r,0.5f),4);
			Effects.Explode(p,Mathf.Max(dmg/10,0.3f),this,mask,r);
		}

		public OnExplodeDelay(){
			procCoefficent=0;
			chance=0.7f;
			dmgMultiplier=0.5f;
		}

		public override Proc newSelf(){
			return new OnExplodeDelay();
		}
	}


	public class PullOut : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			float r = 1;
			Collider2D[] c = Physics2D.OverlapCircleAll(d.transform.position,r,perent.perent.layerMask(false));
			foreach(Collider2D col in c){
				if(col.gameObject!=d.gameObject){
					Damageable D = col.GetComponent<Damageable>();
					Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
					if(D && rb){
						float magnitude = -perent.perent.damage()/1.4f;
						Vector2 direction = col.transform.position-d.transform.position;
						direction = -direction.normalized*magnitude;
						rb.AddForce(direction,ForceMode2D.Impulse);
					}
				}
			}

		}

		public PullOut(){
			procCoefficent=0;
			chance=0.5f;
			dmgMultiplier=1f;
		}

		public override Proc newSelf(){
			return new PullOut();
		}
	}

	public class PushOut : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
			Rigidbody2D rb = d.GetComponent<Rigidbody2D>();
			if(rb){
				float magnitude = perent.perent.damage()/1.4f;
				Vector2 direction = d.transform.position-perent.perent.transform.position;
				direction = direction.normalized*magnitude;
				rb.AddForce(direction,ForceMode2D.Impulse);
			}

		}

		public PushOut(){
			procCoefficent=0;
			chance=0.5f;
			dmgMultiplier=1f;
		}

		public override Proc newSelf(){
			return new PushOut();
		}
	}


}

/*
	public class T : Proc{
		public override void OnProc(Damageable d){
			base.OnProc(d);
		}

		public T(){
			procCoefficent=0;
			chance=1f;
			dmgMultiplier=1f;
		}

		public override Proc newSelf(){
			return new T();
		}
	}
*/
