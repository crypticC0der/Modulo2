using UnityEngine;
using System.Collections.Generic;

namespace Modulo{
	public enum SpecialProperties{
		random=1,
		homing=2,
		predictive=4,
		rapidFire=8,
		returning=16,
		spdUp=32,
		polar=64
	}

	/// <summary>
	/// this basically is a combatable entity that is still this is the basis for both a turret and an enemy
	/// you use this to create classes that can fight and have attacks
	/// </summary>
	public class Combatant : Damageable{
		public List<Proc> procs = new List<Proc>();
		public List<Debuff> toApply = new List<Debuff>();
		public List<Attack> attacks = new List<Attack>();
		public float dmgPlus=0;
		public float dmgMultipler=1;
		public float attackRate=1;
		public float attackSpeed=0;
		public float shotSpeed=1; //only applicable with 2 things
		public float maxRange=0;
		public float range=1;
		public int peirce=0;
		public int funnelShots=1;
		public int crossShots=1;
		public int totalShots(){return funnelShots*crossShots;}
		public SpecialProperties specialProperties;

		//targeting works like so
		//player should only get hit in crossfire, or by enemies if theres nothing more interesting
		//if the player is holding the orb the enemies will very much target the player though
		//the only time this gets tricky is with area and "melee" attacks
		//with area attacks, they will only trigger if things that arent players are in it as well
		//so the player will get hit, but only in crossfire
		public int layerMask(bool targeting){
			int mask = ~((1<<gameObject.layer) | (1<<7)); //dont hit self or bullet, everythign else is fair game
			if(targeting && type!=EntityTypes.Enemy){
				mask&=~(1<<0); //dont aim at player if ur a thingy
			}
			return mask;
		}

		public virtual void RemoveStats(Stats changes){
			maxHealth-=changes.maxHealth;
			regen-=changes.HpRegen;
			dmgPlus-=changes.damage;
			dmgMultipler-=changes.dmgMultipler;
			attackSpeed-=changes.attackSpeed;
			attackRate-=changes.attackRate;
			shotSpeed-=changes.shotSpeed;
			range-=changes.range;
			peirce-=changes.peirce;
			funnelShots+=changes.funnelShots;
			crossShots+=changes.crossShots;
			if(changes.range!=0 && type==EntityTypes.Turret){
				ValidateRange(true);
			}
		}

		public virtual void ApplyStats(Stats changes){
			maxHealth+=changes.maxHealth;
			regen+=changes.HpRegen;
			dmgPlus+=changes.damage;
			dmgMultipler+=changes.dmgMultipler;
			attackSpeed+=changes.attackSpeed;
			attackRate+=changes.attackRate;
			shotSpeed-=changes.shotSpeed;
			range+=changes.range;
			peirce+=changes.peirce;
			funnelShots-=changes.funnelShots;
			crossShots-=changes.crossShots;
			if(changes.range!=0 && type==EntityTypes.Turret){
				ValidateRange(false);
			}
		}

		public void ValidateRange(bool reset=false){
			int[] wop = World.WorldPos(transform.position);
			if(reset){
				World.ChangeStatesInRange(wop[0],wop[1],maxRange,NodeState.targeted,World.ChangeStateMethod.Off);
			}
			maxRange=0;
			foreach(Attack a in attacks){
				if (a.attackRange()>maxRange){
					maxRange=a.attackRange();
				}
			}
			World.ChangeStatesInRange(wop[0],wop[1],maxRange,NodeState.targeted,World.ChangeStateMethod.On);
		}

		public void AddAttack<A>() where A:Attack,new(){
			Attack a = new A();
			attacks.Add(a);
			a.perent=this;

			if (a.attackRange()>maxRange && type==EntityTypes.Turret){
				maxRange=a.attackRange();
				int[] wop = World.WorldPos(transform.position);
				World.ChangeStatesInRange(wop[0],wop[1],maxRange,NodeState.targeted,World.ChangeStateMethod.On);
			}
		}

		public void AddProc<P>() where P:Proc,new(){
			procs.Add(new P());
		}

		public void ApplyDebuffs(float procCoefficent,Damageable d){
			foreach(Debuff de in toApply){
				if(Random.value<procCoefficent*de.chance){
					de.Apply(d);
				}
			}
		}

		public void RunProc(float procCoefficent,Attack att,float dmg,Damageable hit){
			foreach(Proc p in procs){
				if(Random.value<procCoefficent*p.chance){
					(p.Go(dmg,att)).OnProc(hit);
				}
			}
		}

		public virtual void RunAttacks(){
			foreach (Attack a in attacks){
				a.Update();
			}

		}

		public override void FixedUpdate(){
			base.FixedUpdate();
			RunAttacks();
		}
	}
}
