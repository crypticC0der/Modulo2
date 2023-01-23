using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo{
	public class followAi : MonoBehaviour
	{
		void Start(){
			rb= GetComponent<Rigidbody2D>();
			fsm=GetComponent<EnemyFsm>();
		}


		EnemyFsm fsm;
		public float force;
		Rigidbody2D rb;
		void Update(){
			force=fsm.Speed()*10*fsm.speedBonus;
			int[] wap = World.WorldPos(transform.position);
			Node n = World.grid[wap[0],wap[1]].next;
			Vector3 v;
			if(n!=null){
				fsm.distance=n.realDistance;
				v = n.WorldPos()-transform.position;
			}else{
				v=PlayerBehavior.me.transform.position-transform.position;
				fsm.distance=v.magnitude;
			}
			rb.AddForce(v.normalized*Time.deltaTime*force/0.02f);
		}
	}
}
