using UnityEngine;
namespace Modulo{
	public abstract class Alter : MonoBehaviour,Clickable{
		[SerializeField] protected int effectRange {private set;get;} = 3;
		[SerializeField] protected float timerStart=0;
		[SerializeField] protected Component.Id toProduce;
		[SerializeField] private float timer=0;
		[SerializeField] private int contains=0;
		[SerializeField] private bool running=false;

		protected abstract bool Condition();
		protected abstract void Effect();
		protected abstract float Speed();

		public void LeftClick(ClickEventHandler e){
			if(!running){
				int inc = contains+1;
				if(PlayerBehavior.me.componentCount[0]>inc){
					PlayerBehavior.me.componentCount[0]-=inc;
					Component.UpdateComponentUI(Component.Id.Grey,
											PlayerBehavior.me.componentCount[0]);
					contains += inc;
					timerStart+=1;
				}
			}
		}

		public virtual void RightClick(ClickEventHandler e){
			if(!running){
				running=true;
				timer=timerStart;
			}
		}

		public void Update(){
			if(running){
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
				}
			}
		}
	}


	public class RefiningAlter : Alter{
		private const float timeToRun=5;

		public void Start(){
			toProduce=Component.Id.Yellow;
		}

		protected override void Effect(){}
		protected override bool Condition(){
			return (PlayerBehavior.me.transform.position - transform.position)
			   .magnitude<=effectRange;
		}
		protected override float Speed() => 1;

		public override void RightClick(ClickEventHandler e){
			timerStart=timeToRun;
			base.RightClick(e);
		}
	}

	public class SharpeningAlter : Alter, HasMask{
		private const float dps=1;
		private bool runEffect=false;
		private const bool healthSafety=true;

		public void Start(){
			toProduce=Component.Id.Pink;
		}

		protected override void Effect(){}
		protected override bool Condition() => true;
		public int simpleLayerMask()=>(this as HasMask).slm();

		protected override float Speed(){
			Collider2D[] cols = Physics2D.OverlapCircleAll(
				transform.position,
				effectRange, this.simpleLayerMask());

			float spd=0;
			float dmg = dps*timerStart;
			DamageData d =
				new DamageData {properties = DamageProperties.bypassArmor,
								dmg=dmg*Time.deltaTime};
			foreach(Collider2D c in cols){
				Damageable target = c.GetComponent<Damageable>();
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
			return spd;
		}

	}
}
