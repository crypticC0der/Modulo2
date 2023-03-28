using UnityEngine;
namespace Modulo{
	public abstract class Alter : MonoBehaviour,Clickable{
		[SerializeField]
		protected int effectRange {private set;get;} = 3;

		protected float timer=0;
		protected Component.Id toProduce;
		private int contains=0;
		private bool running=false;

		protected abstract bool Condition();
		protected abstract void Effect();
		protected abstract int Speed();

		public void LeftClick(ClickEventHandler e){
			int inc = contains+1;
			if(PlayerBehavior.me.componentCount[0]>inc){
				PlayerBehavior.me.componentCount[0]-=inc;
				contains += inc;
				timer+=1;
			}
		}

		public virtual void RightClick(ClickEventHandler e){running=true;}

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
					running=false;
				}
			}
		}
	}


	public class RefiningAlter : Alter{
		private const float timeToRun=5;
		protected override void Effect(){}
		protected override bool Condition(){
			return (PlayerBehavior.me.transform.position - transform.position)
			   .magnitude>effectRange;
		}
		protected override int Speed() => 1;

		public override void RightClick(ClickEventHandler e){
			timer=timeToRun;
			base.RightClick(e);
		}

	}

	public class SharpeningAlter : Alter, HasMask{
		private const float dps=1;
		private bool runEffect=false;

		public void Start(){
			toProduce=Component.Id.Pink;
		}

		protected override void Effect(){}
		protected override bool Condition() => true;
		public int simpleLayerMask()=>(this as HasMask).simpleLayerMask();

		protected override int Speed(){
			Collider2D[] cols = Physics2D.OverlapCircleAll(
				transform.position,
				effectRange, this.simpleLayerMask());

			DamageData d =
				new DamageData { dmg = dps*Time.deltaTime,
									properties = DamageProperties.bypassArmor };
			foreach(Collider2D c in cols){
				Damageable D = c.GetComponent<Damageable>();
				if(D){
					d.direction = D.transform.position-transform.position;
					D.TakeDamage(d);
				}
			}
			return cols.Length;
		}

	}
}
