using UnityEngine;

namespace Modulo{
	public enum DebuffName{
		Burning=1,
		GreenTrail=2,
		Stunned=4,
		Slowed=8
	}

	///<summary>
	///debuffs are not procs, procs do a thing on hit
	///debuffs stay attached to an object for a bit
	///debuffs also dont care about the damage the attack did
	///</summary>
	[System.Serializable]
	public abstract class Debuff{
		public DebuffName name;
		public Damageable perent;
		public float timeLeft;
		public int stacks=1;
		public float chance=1;
		public string actualName;

		public abstract void Apply(Damageable d,Combatant applier);
		public virtual bool onUpdate(){
			timeLeft-=Time.deltaTime;
			if(timeLeft<0){
				onEnd();
				return false;
			}
			return true;
		}

		public virtual void onHit(Combatant hitter){}
		public virtual void onEnd(){
			perent.appliedDebuffs^=name;
		}
	}

	public class Burning:Debuff{
		const int dmg=2;

		public override void Apply(Damageable d,Combatant applier){
			d.ApplyDebuff<Burning>(applier);
		}

		public Burning(){
			actualName="burning";
			timeLeft=3;
			chance=1;
			name=DebuffName.Burning;
		}

		public override bool onUpdate(){
			perent.TakeDamage(new DamageData{dmg=dmg*stacks*Time.deltaTime});
			return base.onUpdate();
		}
	}


	public abstract class Trail:Debuff{

		private class TrailFade : MonoBehaviour{
			public LineRenderer lr;
			public float timedelta;
			float t=0;

			public void Update(){
				t+=Time.deltaTime;
				if (t>timedelta){
					t=0;
					if(lr.positionCount==0){
						GameObject.Destroy(gameObject);
					}else{
						lr.positionCount--;
					}
				}
			}
		}

		readonly float[] width = {0.4f,0.4f};
		protected Color[] colors = new Color[2];
		protected float dmg;
		protected float duration;
		protected float timeBetweenReads;
		Vector3[] points;
		LineRenderer lr;
		int length;
		int head=0;
		int mask;

		public Trail(bool all=true){
			Initialize();
			length = (int)Mathf.Round(duration/timeBetweenReads);
			points = new Vector3[length];
			if(all){
				GameObject o = new GameObject("lineRenderer");
				//define the renderer
				LineRenderer r = o.AddComponent<LineRenderer>();
				o.layer=7;
				r.material=Resources.Load<Material>("RayMaterial");
				r.positionCount=length;
				r.startWidth=width[0];
				r.endWidth=width[1];
				r.startColor=colors[0];
				r.endColor=colors[1];
				r.numCornerVertices=7;
				r.numCapVertices=3;
				lr=r;
			}
		}

		public override void onEnd(){
			if(lr.positionCount>2){
				TrailFade tf = lr.gameObject.AddComponent<TrailFade>();
				tf.lr=lr;
				tf.timedelta=timeBetweenReads;
			}else{
				GameObject.Destroy(lr);
			}
			base.onEnd();
		}

		float t=0;
		float colTimer=0;
		public override bool onUpdate(){
			if(t>timeBetweenReads){
				t=0;
				//adjust the circular queue
				head= (head+1) % length;
				lr.positionCount=length;
				points[head]=perent.transform.position;
				if(head>2){
					for(int i=0;i<length;i++){
						//get ith point from the head
						Vector3 p =points[(head-i+length)%length];
						if(p==Vector3.zero){
							//if its undefined then end the trail there
							lr.positionCount=i;
							lr.endWidth = Mathf.Lerp(width[0],width[1],i/(float)length);
							break;
						}else{
							//set the position
							lr.SetPosition(i,p);
						}
					}
				}
			}
			if(colTimer>1/30f){
				DamageData data = (new DamageData{dmg=dmg*stacks*colTimer});
				colTimer=0;
				Globals.DealDmgAlongLine(points,head,mask,(Damageable d) => {d.TakeDamage(data);},69,length);

			}
			t+=Time.deltaTime;
			colTimer+=Time.deltaTime;
			// perent.TakeDamage(new DamageData{dmg=dmg*stacks*Time.deltaTime});
			return base.onUpdate();
		}
		protected abstract void Initialize();

		public override void onHit(Combatant hitter){
			mask = hitter.layerMask(false);
			base.onHit(hitter);
		}

	}

	public class GreenTrail : Trail{
		public static new float dmg=4;

		public GreenTrail(bool all=true):base(all){}
		public GreenTrail():base(true){}

		public override void Apply(Damageable d,Combatant applier){
			d.ApplyDebuff<GreenTrail>(applier);
		}

		protected override void Initialize(){
			actualName="poison trail";
			timeLeft=3;
			name=DebuffName.GreenTrail;
			colors[0] = MeshGen.MeshGens.ColorFromHex(0x608820ff);
			colors[1] = MeshGen.MeshGens.ColorFromHex(0x44330000);
			base.dmg=GreenTrail.dmg;
			duration=0.5f;
			timeBetweenReads=1/30f;
		}
	}

	public class Stunned : Debuff{
		public override void Apply(Damageable d,Combatant applier){
			d.ApplyDebuff<Stunned>(applier);
		}
		public Stunned(){
			actualName="stunned";
			timeLeft=1;
			chance=0.5f;
			name=DebuffName.Stunned;
		}
	}

	public class Slowed: Debuff{
		public override void Apply(Damageable d,Combatant applier){
			d.ApplyDebuff<Slowed>(applier);
		}
		public Slowed(){
			actualName="slowed";
			timeLeft=3;
			chance=0.3f;
			name=DebuffName.Slowed;
		}
	}
}
