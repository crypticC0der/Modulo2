using UnityEngine;
using System.Collections.Generic;
using MUtils;

namespace Modulo{
	public abstract class Alter : MonoBehaviour,Clickable{
		[SerializeField] protected float effectRange {private set;get;} = 3;
		[SerializeField] protected float timerStart=0;
		[SerializeField] protected Component.Id toProduce;
		[SerializeField] private float timer=0;
		[SerializeField] private int contains=0;
		[SerializeField] private bool running=false;

		//will be run n times per frame
		protected abstract float Speed();
		protected abstract bool Condition();

		//run once per frame
		protected abstract void Effect();
		protected virtual void AnimStep(){
			if(Condition()){
				Rotate();
			}
		}
		protected virtual void AnimCleanStep(){}
		//run when state changed
		protected abstract void CleanUp();
		protected abstract void SetUp();

		public void LeftClick(ClickEventHandler e){
			if(running){return;}

			int inc = contains+1;
			if(PlayerBehavior.me.componentCount[0]>inc){
				PlayerBehavior.me.componentCount[0]-=inc;
				Component.SendComponents(Component.Id.Grey,inc,this);
				Component.UpdateComponentUI(Component.Id.Grey,
										PlayerBehavior.me.componentCount[0]);
				contains += inc;
				timerStart+=1;
			}
		}

		public virtual void RightClick(ClickEventHandler e){
			if(!running){
				running=true;
				timer=timerStart;
				SetUp();
			}
		}

		void Rotate(){
			Transform gear = transform.GetChild(0);
			Vector3 angle = gear.localEulerAngles;
			angle.z+=90*Time.deltaTime*Speed();
			gear.localEulerAngles=angle;
		}

		public void Update(){
			if(running){
				AnimStep();
				if(timer>0 && Condition()){
					Effect();
					timer-=Time.deltaTime*Speed();
				}else if(timer<=0){
					Component.SpawnComponent(toProduce,contains,
											 transform.position);
					contains=0;
					timer=0;
					timerStart=0;
					running=false;
					CleanUp();
				}
			}else{
				AnimCleanStep();
			}
		}
	}


	public class RefiningAlter : Alter{
		private const float timeToRun=5;
		private const float chainScale=.15f;
		// IKLimb limb;
		SpriteRenderer chainAlter;
		SpriteRenderer chainPlayer;

		public void Start(){
			toProduce=Component.Id.Yellow;
		}

		protected override void Effect(){}
		protected override bool Condition(){
			return (PlayerBehavior.me.transform.position - transform.position)
			   .magnitude<=effectRange;
		}
		protected override float Speed() => 1;

		protected override void AnimStep(){
			Vector3 plr = PlayerBehavior.me.transform.position;
			Vector3 d = plr - transform.position;

			if(d.magnitude > effectRange){
				d = d.normalized * effectRange/2f;
			}else{
				d/=2;
			}

			float sizeStep = 4.5f*Time.deltaTime;
			if(chainAlter.size.y - d.magnitude/chainScale < sizeStep){
				d = d.normalized*(chainAlter.size.y*chainScale + sizeStep);
			}

			UpdateChains(d);

			base.AnimStep();
		}

		void UpdateChains(Vector3 d){
			Vector3 rot = new Vector3(0,0,World.VecToAngle(d));
			Vector2 size = new Vector2(chainAlter.size.x,
									   d.magnitude/chainScale);

			chainAlter.size = size;
			chainAlter.transform.position = transform.position + d/2;
			chainAlter.transform.eulerAngles = rot;

			chainPlayer.size = size;
			chainPlayer.transform.position =
				PlayerBehavior.me.transform.position - d/2;
			rot.z+=180;
			chainPlayer.transform.eulerAngles = rot;
		}

		void SmoothMagnitude(Vector3 aim){

		}

		SpriteRenderer GenChain(){
			GameObject chain = new GameObject("chain");
			SpriteRenderer chainRenderer =
				chain.AddComponent<SpriteRenderer>();
			chainRenderer.sprite = ItemTemplate.GetGraphic("chain2");
			chainRenderer.drawMode = SpriteDrawMode.Tiled;
			chainRenderer.color = Component.ComponentColour(toProduce);
			chain.transform.localScale=Vector3.one*chainScale;
			chainRenderer.size = new Vector2(chainRenderer.size.x,0);
			return chainRenderer;
		}

		protected override void SetUp(){
			chainAlter = GenChain();
			chainPlayer = GenChain();
		}

		protected override void CleanUp(){}
		protected override void AnimCleanStep(){
			if(chainAlter == null){return;}

			float sizeStep = 4.5f*Time.deltaTime;
			if(chainScale*chainAlter.size.y > sizeStep){
				Vector3 plr = PlayerBehavior.me.transform.position;
				Vector3 d = plr - transform.position;
				Vector2 size = chainAlter.size;
				d = d.normalized*(size.y*chainScale - sizeStep);
				UpdateChains(d);
			}else{
				Destroy(chainAlter.gameObject);
				Destroy(chainPlayer.gameObject);
			}
		}

		public override void RightClick(ClickEventHandler e){
			timerStart=timeToRun;
			base.RightClick(e);
		}
	}

	public class SharpeningAlter : Alter, HasMask{
		struct GameLimb {
			public GameObject obj;
			public IKLimb limb;
		};


		const float w1=0.1f;
		const float w2=0.03f;
		const float s=5f;
		static Color c1 = MeshGen.MeshGens.ColorFromHex(0x802020ff);
		static Color c2 = MeshGen.MeshGens.ColorFromHex(0x501010ff);
		List<GameLimb> activeLimbs = new List<GameLimb>();
		[SerializeReference]
		List<IKLimb> deadLimbs = new List<IKLimb>();

		private const float dps=1;
		private bool runEffect=false;
		private const bool healthSafety=true;
		private float spd=0;

		protected override bool Condition() => true;
		protected override float Speed() => spd;
		public int simpleLayerMask()=>(this as HasMask).slm();
		protected override void SetUp(){}

		public void Start(){
			toProduce=Component.Id.Pink;
		}


		void KillLimb(int i){
			activeLimbs[i].limb.state= IKLimb.Status.Dying;
			deadLimbs.Add(activeLimbs[i].limb);
			activeLimbs.RemoveAt(i);
		}

		protected override void AnimStep(){
			for(int i=0;i<deadLimbs.Count;){
				if(deadLimbs[i].state==IKLimb.Status.Dead){
					deadLimbs.RemoveAt(i);
				} else {
					deadLimbs[i].Animate();
					i++;
				}
			}
			for(int i=0;i<activeLimbs.Count;){
				bool ded=false;
				activeLimbs[i].limb.Animate();
				if(activeLimbs[i].obj != null){
					Vector3 d = activeLimbs[i].limb.end.GetPoint()
						-transform.position;
					if(d.magnitude > 1.5* effectRange){
						KillLimb(i);
						ded=true;
					}
				}else{
					KillLimb(i);
					ded=true;
				}
				if(!ded){
					i++;
				}
			}
			base.AnimStep();
		}

		protected override void CleanUp(){
			while(activeLimbs.Count>0){
				KillLimb(activeLimbs.Count-1);
			}
		}

		protected override void AnimCleanStep(){
			for(int i=0;i<deadLimbs.Count;i++){
				deadLimbs[i].Animate();
				if(deadLimbs[i].state==IKLimb.Status.Dead){
					deadLimbs.RemoveAt(i);
				}
			}
		}

		void updateLimbs(){
			Collider2D[] cols = Physics2D.OverlapCircleAll(
				transform.position,
				effectRange, this.simpleLayerMask());
			foreach(Collider2D c in cols){
				bool match = false;
				foreach(GameLimb gl in activeLimbs){
					if(gl.obj == c.gameObject){
						match=true;
						break;
					}
				}
				if(!match){
					GameLimb newLimb;
					MovingPoint mp = new MovingPoint(s,
										new DynamicPoint(c.gameObject.transform),
										transform.position);
					newLimb.obj = c.gameObject;
					if(deadLimbs.Count>0){
						newLimb.limb = deadLimbs[deadLimbs.Count-1];
						deadLimbs.RemoveAt(deadLimbs.Count-1);
						newLimb.limb.end = mp;
					}else{
						Vector3 p = transform.position;
						p.z--;
						newLimb.limb = new IKLimb(12,effectRange,
												new DynamicPoint(p), mp);
						newLimb.limb.SetWidthGradient(w1,w2);
						newLimb.limb.SetColourGradient(c1,c2);
					}
					activeLimbs.Add(newLimb);
				}
			}
		}

		float t=0;
		protected override void Effect(){
			if(t>0.5f){
				updateLimbs();
				t=0;
			}
			t+=Time.deltaTime;
			spd=0;
			float dmg = dps*timerStart;
			DamageData d =
				new DamageData {properties = DamageProperties.bypassArmor,
								dmg=dmg*Time.deltaTime};

			foreach(GameLimb gl in activeLimbs){
				Vector3 dist =gl.obj.transform.position-
					gl.limb.end.GetPoint();
				if(dist.magnitude<0.1f){
					Damageable target = gl.obj.GetComponent<Damageable>();
					if(target){
						if(!healthSafety){
							d.dmg = Mathf.Min(dmg,target.maxHealth);
							d.dmg*=Time.deltaTime;
							spd+=d.dmg/dmg;
						}else{
							spd++;
						}
						d.direction = target.transform.position-transform.position;
						target.TakeDamage(d);
					}
				}
			}
		}
	}
}
