using UnityEngine;
using System.Collections.Generic;
using MUtils;
using MeshGen;

namespace Modulo{
	public class ComponentInserter{
		public delegate void OnInsert();

		OnInsert hookStart;
		OnInsert hookStep;
		public int contains{get; private set;}
		public Component.Id component;
		Transform t;
		float heldFor=0;
		float missed=0;

		const float timeStep=0.16f;
		float stepTimer=0;

		public void Insert(){
			heldFor=3;
			missed=0;
			hookStart();
		}

		public void InsertCont(){
			if(stepTimer<timeStep){
				stepTimer+=Time.deltaTime;
				return;
			}
			float incacc = (Mathf.Pow(2,heldFor+stepTimer) - Mathf.Pow(2,heldFor))/Mathf.Log(2) + missed;
			int inc=(int)(incacc);
			missed=incacc-inc;
			if(PlayerBehavior.me.SpendComponent(component,inc,
												t.position)){
				contains += inc;
				Debug.Log(contains);
				heldFor+=stepTimer;
				hookStep();
			}
			stepTimer=0;
		}

		public void Reset() =>contains=0;
		public ComponentInserter(Component.Id component,Transform perent,
								 OnInsert hook,OnInsert hookStep){
			this.hookStep=hookStep;
			this.hookStart=hook;
			this.component=component;
			this.t=perent;
		}

	}

	public abstract class Alter : MonoBehaviour,Clickable{
		[SerializeField] protected float effectRange {private set;get;} = 3;
		[SerializeField] protected float timerStart=3;
		[SerializeField] protected Component.Id toProduce;
		[SerializeField] private float timer=0;
		[SerializeField] private bool running=false;
		private ComponentInserter ci;
		private HexBorder hb;

		//will be run n times per frame
		protected abstract float Speed();
		protected abstract bool Condition();

		//run once per frame
		protected abstract void Effect();
		protected virtual void AnimStep(){
			if(Condition()){
				Rotate();
				hb.updateLR(timer/timerStart);
			}
		}
		protected virtual void AnimCleanStep(){}
		//run when state changed
		protected virtual void CleanUp(){
			hb.Destroy();
		}

		protected virtual void SetUp(){
			Component.ExplodeComponents(Component.Id.Grey,
										ci.contains,
										transform.position,
										effectRange);
			hb = new HexBorder(Component.ComponentColour(Component.Id.Grey));
			hb.setParent(transform);
			hb.updateLR(1);
		}

		public virtual void Start(){
			ci = new ComponentInserter(Component.Id.Grey,transform,() =>{}, () => timerStart+=Time.deltaTime);
		}


		public void LeftClickHold(ClickEventHandler e){
			if(running){return;}
			ci.InsertCont();
			Debug.Log("a");
		}
		public void RightClickHold(ClickEventHandler e){}
		public void Hover(ClickEventHandler e){
			// Mouse.SetColour(Component.Id.Grey);
		}

		public void LeftClick(ClickEventHandler e){
			if(running){return;}
			ci.Insert();
		}

		public virtual void RightClick(ClickEventHandler e){
			if(!running && ci.contains>0){
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
					Component.SpawnComponent(toProduce,ci.contains,
											 transform.position);
					ci.Reset();
					timer=0;
					timerStart=3;
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

		public override void Start(){
			toProduce=Component.Id.Yellow;
			base.Start();
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
			base.SetUp();
		}

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
		public int simpleLayerMask() =>(this as HasMask).slm();

		public void Start(){
			toProduce=Component.Id.Pink;
			base.Start();
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
			base.CleanUp();
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

	public class Cauldron : MonoBehaviour,Clickable{
		delegate bool canRotate();

		public void LeftClick(ClickEventHandler e)  => inserters[focus].Insert();
		public void RightClick(ClickEventHandler e){
			focus=(focus+1)%inserters.Length;
			// transform.eulerAngles+=new Vector3(0,0,-120);
			RotateArrow();
		}

		public void LeftClickHold(ClickEventHandler e) => inserters[focus].InsertCont();
		public void RightClickHold(ClickEventHandler e){}
		public void Hover(ClickEventHandler e){
			hovered=true;
			// Mouse.SetColour(inserters[focus].component);
		}

		int focus=0;
		ComponentInserter[] inserters;
		HexBorder[] borders;
		float timer=5;
		public void Start(){
			AddGear(1*Mathf.PI/3,() => timer<3,
					Component.ComponentColour(Component.Id.Yellow));
			AddGear(3*Mathf.PI/3,() => timer<3,
					Component.ComponentColour(Component.Id.Grey));
			AddGear(5*Mathf.PI/3,() => timer<3,
					Component.ComponentColour(Component.Id.Pink));

			ComponentInserter.OnInsert hook = ()=>timer=5;
			inserters = new ComponentInserter[3]{
				new ComponentInserter(Component.Id.Pink,transform,hook,hook),
				new ComponentInserter(Component.Id.Yellow,transform,hook,hook),
				new ComponentInserter(Component.Id.Grey,transform,hook,hook)
			};
			borders = new HexBorder[2]{
				new HexBorder(Component.ComponentColour(Component.Id.Pink)),
				new HexBorder(Component.ComponentColour(Component.Id.Yellow))
			};
			GameObject perent = new GameObject("cauldron");
			perent.transform.position=transform.position;
			transform.localPosition=Vector3.zero;
			transform.SetParent(perent.transform);
			borders[0].setParent(perent.transform);
			borders[1].setParent(perent.transform);
			borders[0].clockwise=false;
			arrow =
				MeshGens.MinObjGen(Shapes.arrow,MatColour.white)
				.GetComponent<Renderer>();
			arrow.transform.SetParent(perent.transform);
			RotateArrow();
		}

		Renderer arrow;
		void RotateArrow(){
			float r = -120f*(focus+1);
			arrow.transform.localEulerAngles=r*Vector3.forward;
			arrow.transform.localPosition=-World.AngleToVec(-r*Mathf.PI/180f)*1.2f;
		}

		class Rotate : MonoBehaviour{
			public canRotate condition{set; private get;}
			const float spd=180;
			public void FixedUpdate(){
				if(condition()){
					transform.eulerAngles += 180*Vector3.forward*Time.deltaTime;
				}
			}
		}

		void Craft(){
			Debug.Log("craft, joe bide");
			inserters[0].Reset();
			inserters[1].Reset();
			inserters[2].Reset();
			timer=5;
		}

		public void Update(){
			Animate();
			int sum = inserters[0].contains+inserters[1].contains+
				inserters[2].contains;
			if(sum<=0){return;}
			if(timer>0){
				timer-=Time.deltaTime;
			}else{
				Craft();
			}
		}

		bool hovered=false;
		void Animate(){
			if(hovered!=arrow.enabled){
				arrow.enabled=hovered;
			}
			hovered=false;
			int sum = inserters[0].contains+
				inserters[1].contains+
				inserters[2].contains;
			if(sum<=0){return;}
			float v = Mathf.Min(timer/3,1) / (sum);

			borders[0].updateLR(v*inserters[0].contains);
			borders[1].updateLR(v*inserters[1].contains);
		}

		const float Radius=0.35f;
		void AddGear(float r,canRotate condition,Color c){
			GameObject gear = new GameObject("gear");
			gear.transform.localScale*=1.1f;
			gear.transform.SetParent(gameObject.transform);
			gear.transform.localPosition = Radius *
				new Vector3(Mathf.Sin(r),Mathf.Cos(r),-1);
			SpriteRenderer sr = gear.AddComponent<SpriteRenderer>();
			sr.sprite = ItemTemplate.GetGraphic("gear");
			sr.color=c;
			sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			gear.AddComponent<Rotate>().condition=condition;
		}

		public static GameObject SpawnCauldron(HexCoord hc){
			GameObject cauldron = new GameObject("cauldron");
			cauldron.AddComponent<Cauldron>();
			cauldron.transform.position = hc.position();
			SpriteRenderer sr = cauldron.AddComponent<SpriteRenderer>();
			SpriteSize spriteSize = ItemTemplate.GetSpriteSize("base");
			sr.sprite = spriteSize.sprite;
			cauldron.transform.localScale = spriteSize.size;
			SpriteMask sm = cauldron.AddComponent<SpriteMask>();
			sm.sprite=spriteSize.sprite;
			cauldron.AddComponent<PolygonCollider2D>();
			cauldron.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			return cauldron;
		}


	}

}
