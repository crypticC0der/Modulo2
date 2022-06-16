using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner{
	float spacing=0;
	float quantity=1;
	int level=0;
	List<Attack> attacks=new List<Attack>();
	List<Proc> procs=new List<Proc>();
	List<Debuff> debuffs=new List<Debuff>();
	Shapes enemy=Shapes.circle;
	float timer=0;
	Vector3 spawn=new Vector3(0,0,0);
	MatColour matColour=MatColour.white;

	public EnemySpawner(){
	}

	public bool Run(){
		if(timer<0){
			timer=spacing;
			quantity--;
			Spawn(spawn,level);
		}
		timer-=Time.deltaTime;
		return quantity>0;
	}


	public EnemyFsm Spawn(Vector3 location, int level,Shapes s){
        EnemyFsm q = MeshGens.ObjGen(enemy,matColour);
        q.toApply=debuffs;
		q.procs=procs;
		foreach (Attack a in attacks){
			a.AddAttack(q);
		}
		q.perent=this;
		return q;
	}

	public EnemyFsm Spawn(Vector3 location, int level){
		return Spawn(location,level,enemy);
	}
}

public static class EnemyGeneration{
	public static List<EnemySpawner> spawners;
	public static void Run(){
		int i=0;
		while(i<spawners.Count){
			if(!spawners[i].Run()){
				spawners.RemoveAt(i);
			}else{
				i++;
			}
		}
	}
}
