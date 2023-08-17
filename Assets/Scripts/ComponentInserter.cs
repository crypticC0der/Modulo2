using UnityEngine;
using System.Collections.Generic;

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
			int inc=contains+1;
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
			if(inc<=0){
				return;
			}
			missed=incacc-inc;
			if(PlayerBehavior.me.SpendComponent(component,inc,
												t.position)){
				contains += inc;
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
}
