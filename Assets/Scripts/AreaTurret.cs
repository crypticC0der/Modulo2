using UnityEngine;
using System.Collections.Generic;
using MeshGen;

namespace Modulo{
	public class ArrowAttack : AreaAttack
	{
		public override void AddAttack(Combatant c)
		{
			c.AddAttack<ArrowAttack>();
		}

		public override Vector3 AtFunc(GameObject o){
			Vector3 direction=o.transform.position-center;
			Damageable d = o.GetComponent<Damageable>();
			DmgOverhead(new DamageData{dmg=damage(),direction=direction},d);

			return direction;
		}

		public override void SingleAtFunc()
		{
			GameObject a =MeshGens.MinObjGen(Shapes.spikedHexagonOuter,MatColour.blue2);
			a.layer=7;
			Vector3 p =center;
			p.z+=10;
			a.transform.position=p;
			ArrowDespawn arr = a.AddComponent<ArrowDespawn>();
			arr.range=attackRange();
		}

		public ArrowAttack() : base()
		{
			range = 2;
			timerMax = 0.5f;
			procCoefficent = .6f;
			dmg = .2f;
			attackPeirce=9;
		}
	}
}
