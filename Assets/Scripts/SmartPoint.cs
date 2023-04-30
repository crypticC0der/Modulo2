using UnityEngine;

namespace MUtils{
public class DynamicPoint{
	Vector3 point;
	Transform aim;

	public Vector3 GetV3(){
		if(aim){
			return aim.position;
		}else{
			return point;
		}
	}

	public DynamicPoint(Vector3 v){Set(v);}
	public DynamicPoint(Transform t){Set(t);}

	public static implicit operator Vector3(DynamicPoint dp) => dp.GetV3();
	public static implicit operator Vector2(DynamicPoint dp) => dp.GetV3();

	public void Set(Vector3 v){
		point=v;
		aim=null;
	}

	public void Set(Transform t){
		aim=t;
		point=Vector3.zero;
	}
}

public class MovingPoint{
	float speed;
	Vector3 v;
	DynamicPoint dp;

	public MovingPoint(float speed,DynamicPoint dp){
		this.speed=speed;
		this.dp=dp;
		v=dp;
	}

	public MovingPoint(float speed,DynamicPoint dp,Vector3 v){
		this.speed=speed;
		this.dp=dp;
		this.v=v;
	}

	public void Step(){
		Vector3 d = dp-v;
		if(d.magnitude<speed*Time.deltaTime){
			v=dp;
		}else{
			v+=d.normalized*speed*Time.deltaTime;
		}
	}

	public Vector3 GetPoint() => v;
	public void SetDynamicPoint(DynamicPoint dp) => this.dp=dp;
	public static implicit operator Vector3(MovingPoint mp) => mp.GetPoint();
	public static implicit operator Vector2(MovingPoint mp) => mp.GetPoint();
}
}
