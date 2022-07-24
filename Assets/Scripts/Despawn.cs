using UnityEngine;
/// <summary>
/// makes a object destroy itself after a couple seconds
/// </summary>
public class Despawn: MonoBehaviour
{
	public float deathTimer=0;
	public virtual void FixedUpdate()
	{
		deathTimer-=Time.deltaTime;
		if(deathTimer<0){
			Destroy(gameObject);
		}
	}
}

public class SpikeDespawn: MonoBehaviour
{
	public Vector2 target;
	public float spd;
	public float deathTimer=0;
	public virtual void FixedUpdate()
	{
		Vector3 t = Vector2.LerpUnclamped(transform.position,target,Time.deltaTime*2*spd/5);
		t.z=-1;
		transform.position=t;
		deathTimer-=Time.deltaTime;
		if(deathTimer<0){
			Destroy(gameObject);
		}
	}
}

public class RayTracking : Despawn{
	public LineRenderer lineRenderer;
	public Transform perent;
    public override void FixedUpdate()
    {
		if(perent!=null){
			lineRenderer.SetPosition(0,perent.position+Vector3.forward);
		}
        base.FixedUpdate();
	}
}

public class RayDespawnTracking :MonoBehaviour {
	public DamageData data;
	public Attack perent;
	public LineRenderer lineRenderer;
	public Damageable[] aims;
	public float length;
	public void FixedUpdate(){
		if(data.sender==null){
			Destroy(gameObject);
			return;
		}
		// TODO i am redoing how the lines work, they shall dynamically reposition themselves to aim at multiple points, this is so homing on the lasers can work properly
		Vector3[] posititons = new Vector3[aims.Length+1];
		posititons[0]=data.sender.transform.position;
		float proc=perent.procCoefficent*Time.deltaTime*2;
		DamageData dataScaled = new DamageData{
			dmg=data.dmg*Time.deltaTime,
			sender=data.sender,
			properties=data.properties
		};
		bool remainingEnemies=false;
		for(int i=0;i<aims.Length;i++){
			if(aims[i]!=null){
				posititons[i+1]=aims[i].transform.position;
				dataScaled.direction=aims[i].transform.position-data.sender.transform.position;
				perent.DmgOverhead(dataScaled,aims[i]);
				remainingEnemies=true;
			}
			else{
				posititons[i+1]=posititons[i];
			}
		}
		posititons[posititons.Length-1]+=Vector3.forward;
		lineRenderer.SetPositions(posititons);
		float currentLength =(posititons[0]-posititons[posititons.Length-1]).magnitude;
		Debug.Log(length);
		if(currentLength>length*1.5f || !remainingEnemies){
			Destroy(gameObject);
		}



		// lineRenderer.SetPosition(0,aims[0].position);
	}
}
