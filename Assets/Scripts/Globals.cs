using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Modulo{
	public static class Globals{

		static Dictionary<string,Attack> attacks=new Dictionary<string,Attack>();
		static Dictionary<string,Module> modules=new Dictionary<string,Module>();
		static Dictionary<string,Debuff> debuffs=new Dictionary<string,Debuff>();
		static Dictionary<string,Proc>   procs  =new Dictionary<string,Proc  >();
		public delegate void Run(Damageable d);

		public static void DealDmgAlongLine(Vector3[] positions,int head,int layerMask,Run onHit,int peirce,int length){
			int hits=0;
			List<Collider2D> pcol = new List<Collider2D>();

			int di = (int)Mathf.Log(length,2);
			for(int i=0;i<(length-di);i+=di){
				RaycastHit2D[] cols = Physics2D.LinecastAll(positions[(i+head)%length],positions[(i+head+di)%length],layerMask);
				for(int j=0;j<cols.Length;j++){
					if(!pcol.Contains(cols[j].collider)){
						pcol.Add(cols[j].collider);
						Damageable d = cols[j].collider.GetComponent<Damageable>();
						if(d){
							onHit(d);
							hits++;
							if(hits>peirce){
								return;
							}
						}
					}
				}
			}

		}
	}
}
