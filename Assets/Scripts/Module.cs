using UnityEngine;
using System.Collections.Generic;

namespace Modulo{
	public enum ModuleType{
		uncommon,
		exotic,
		rare,
		epic
	};

	public abstract class Module : Item{
		public ModuleType t;
		public float range;
		public string effect;
		public IsItem me;

		public abstract void onApply(Combatant perent,float multiplier);

		public void onApply(EnemyFsm perent){onApply(perent,power);}
		public void onApply(Turret perent,IsItem module){
			onApply(perent,DistanceMultiplier(perent,module)*power);
		}

		public abstract void onRemove(Combatant perent,float multiplier);

		public void onRemove(EnemyFsm perent){onRemove(perent,power);}
		public void onRemove(Turret perent,IsItem module){
			onRemove(perent,DistanceMultiplier(perent,module)*power);
		}


		public static float DistanceMultiplier(Turret applied,IsItem module){
			float distance = (applied.transform.position-module.transform.position).magnitude;
			if (distance<=1){distance=1;}
			if (distance<=2){distance=2;}
			if (distance<=3){distance=3;}
			float stability = module.item.stability*applied.stability;
			return 1/(distance*stability);
		}

		public List<Damageable> GetNearby(Vector3 p){
			List<Damageable> ret = new List<Damageable>();
			Collider2D[] inRange = Physics2D.OverlapCircleAll(p,range);
			foreach(Collider2D obj in inRange){
				Damageable damageable;
				if(damageable= obj.GetComponent<Damageable>()){
					if(damageable.type==EntityTypes.Turret){
						ret.Add(damageable);
					}
				}
			}
			return ret;
		}

		public override GameObject ToGameObject(Vector3 p)
		{
			GameObject go = base.ToGameObject(p);
			me = go.GetComponent<IsItem>();
			List<Damageable> nearbyTurrets = GetNearby(p);
			foreach (Damageable turret in nearbyTurrets){
				onApply((Turret)turret,me);
			}
			return go;
		}

	}

	public class StatModule : Module{
		public Stats changes;

		public override void onApply(Combatant perent,float multiplier){
			perent.ApplyStats(changes,multiplier);
		}

		public override void onRemove(Combatant perent,float multiplier){
			perent.RemoveStats(changes,multiplier);
		}

		public StatModule(Stats s){
			changes = s;
		}

	}

	public class DebuffModule : Module{
		public Debuff debuff;
		public override void onApply(Combatant perent,float multiplier)
		{
			perent.toApply.Add(debuff);
		}
		public override void onRemove(Combatant perent,float multiplier)
		{
			perent.toApply.Remove(debuff);
		}
		public DebuffModule(Debuff d){
			debuff = d;
			debuff.modulePerent=this;
		}
	}
}
