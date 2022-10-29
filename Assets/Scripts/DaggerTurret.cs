using UnityEngine;
using System.Collections.Generic;
namespace Modulo{
	public class DaggerAttack : MeleeAttack
	{
		public override void AddAttack(Combatant c)
		{
			c.AddAttack<DaggerAttack>();
		}

		protected override Weapon basicWeapon(){
			GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("dagger"));
			obj.transform.position=perent.transform.position+Vector3.up*1.525f;
			Collider2D c= obj.GetComponent<Collider2D>();
			obj.layer=7;
			Dagger d = new Dagger();
			d.perent=this;
			d.self=obj;
			WeaponHit h = d.self.AddComponent<WeaponHit>();
			h.perent=this;
			h.perentLayer=perent.layerMask(false);
			return d;
		}

		float directionTimer=0;
		float currentTurnDirection=0;
		float angle=0;

        public override void Update()
        {
            base.Update();

			bool allWait=true;
			foreach(Weapon c in weapons){
				allWait&= c.s==MeleeState.Ready;
			}

			directionTimer-=Time.deltaTime;
			if(allWait){
				if(directionTimer<0){
					currentTurnDirection=GetDirection();
					directionTimer=0.2f;
				}
				angle+=Mathf.PI*Time.deltaTime*currentTurnDirection;
			}

			if(increase || allWait){
				Vector3 d = weapons[0].self.transform.position-perent.transform.position;
				int nShots = perent.totalShots();
				float initialMulti=perent.dmgMultipler;
				int initialCross = perent.crossShots;
				int initialFunnel = perent.funnelShots;
				if(nShots>maxShots){
					perent.dmgMultipler*=nShots/maxShots;
					perent.crossShots=1;
					perent.funnelShots=maxShots;
				}
				for(int i=0;i<perent.crossShots;i++){
					float r = angle+(i*2*Mathf.PI/perent.crossShots) - (Mathf.PI*(perent.funnelShots-1)/maxShots);
					for(int j=0;j<perent.funnelShots;j++){
						Vector2 direction = new Vector2(Mathf.Sin(r),Mathf.Cos(r))*d.magnitude + (Vector2)perent.transform.position;
						weapons[i*perent.funnelShots+j].self.transform.position=direction;
						weapons[i*perent.funnelShots+j].start=direction;
						weapons[i*perent.funnelShots+j].self.transform.eulerAngles=new Vector3(0,0,-180*r/Mathf.PI);
						weapons[i*perent.funnelShots+j].baseAngle=new Vector3(0,0,-180*r/Mathf.PI);
						// AtFunc(direction);
						r+=2*Mathf.PI/maxShots;
					}
				}
				perent.dmgMultipler=initialMulti;
				perent.crossShots=initialCross;
				perent.funnelShots=initialFunnel;
			}
		}

		public DaggerAttack() : base()
		{
			range = 5;
			shotSpd=10;
			timerMax = 1f;
			procCoefficent = 1;
			dmg = 1;
		}
	}

	public class TentacleAttack : MeleeAttack
	{
		public List<Collider2D> hitTargets;
		public float t=0;

		public override void Update(){
			if(t>0.5f){
				hitTargets.Clear();
				foreach(Weapon w in weapons){
					((Tentacle)w).CollisionUpdate();
				}
				t=0;
			}else{
				t+=Time.deltaTime;
			}

			base.Update();
		}

		public override void AddAttack(Combatant c)
		{
			c.AddAttack<TentacleAttack>();
		}

		protected override Weapon basicWeapon(){
			Tentacle t = new Tentacle();
			t.perent=this;
			return t;
		}

		public TentacleAttack() : base()
		{
			range = 5;
			shotSpd=10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 1;
			attackPeirce=6;
			hitTargets=new List<Collider2D>();
			disabledProps = SpecialProperties.rapidFire | SpecialProperties.spdUp;
		}
	}

	public class ArmAttack: MeleeAttack
	{
		public List<Collider2D> hitTargets;
		public float t=0;

		public override void Update(){
			if(t>0.5f){
				hitTargets.Clear();
				foreach(Weapon w in weapons){
					((SwordHand)w).CollisionUpdate();
				}
				t=0;
			}else{
				t+=Time.deltaTime;
			}


			base.Update();
		}

		public override void AddAttack(Combatant c)
		{
			c.AddAttack<ArmAttack>();
		}

		protected override Weapon basicWeapon(){
			SwordHand t = new SwordHand();
			t.perent=this;
			t.end=perent.transform.position;
			t.previousEnd=perent.transform.position;
			t.center=perent.transform.position;
			WeaponHit h = t.self.AddComponent<WeaponHit>();
			h.perent=this;
			h.perentLayer=perent.layerMask(false);
			return t;
		}

		public ArmAttack() : base()
		{
			range = 5;
			shotSpd=10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 1;
			attackPeirce=6;
			hitTargets=new List<Collider2D>();
		}
	}

	public class SaberAttack : MeleeAttack
	{
		public List<Collider2D> hitTargets;
		public float t=0;

		public override void Update(){
			if(t>0.5f){
				hitTargets.Clear();
				foreach(Weapon w in weapons){
					((Beam)w).CollisionUpdate();
				}
				t=0;
			}else{
				t+=Time.deltaTime;
			}
			base.Update();
		}

		public override void AddAttack(Combatant c)
		{
			c.AddAttack<SaberAttack>();
		}

		protected override Weapon basicWeapon(){
			Beam t = new Beam();
			t.perent=this;
			t.center=perent.transform.position;
			WeaponHitter h = t.self.transform.GetChild(0).gameObject.AddComponent<WeaponParticle>();
			h.perent=this;
			h.perentLayer=perent.layerMask(false);
			t.collisionModule.collidesWith=perent.layerMask(false);
			return t;
		}

		public SaberAttack() : base()
		{
			range = 5;
			shotSpd=10;
			timerMax = 0.5f;
			procCoefficent = 1;
			dmg = 1;
			attackPeirce=6;
			hitTargets=new List<Collider2D>();
			disabledProps|= SpecialProperties.rapidFire;
		}
	}

}
