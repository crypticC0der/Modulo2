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
		public abstract void onApply(Combatant perent);
		public abstract void onRemove(Combatant perent);

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
			List<Damageable> nearbyTurrets = GetNearby(p);
			foreach (Damageable turret in nearbyTurrets){
				onApply((Turret)turret);
			}
			return base.ToGameObject(p);
		}

	}

	public class StatModule : Module{
		public Stats changes;

		public override void onApply(Combatant perent){
			perent.ApplyStats(changes);
		}

		public override void onRemove(Combatant perent){
			perent.RemoveStats(changes);
		}

		public StatModule(Stats s){
			changes = s;
		}

	}

	public class DebuffModule : Module{
		public Debuff debuff;
		public override void onApply(Combatant perent)
		{
			perent.toApply.Add(debuff);
		}
		public override void onRemove(Combatant perent)
		{
			perent.toApply.Remove(debuff);
		}
		public DebuffModule(Debuff d){
			debuff = d;
		}
	}
}
