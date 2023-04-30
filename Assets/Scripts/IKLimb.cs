using UnityEngine;
using System.Collections;
using System;

namespace MUtils{
[System.Serializable]
public class IKLimb{
	public enum Status{
		Normal,Dying,Spawning,Dead
	}

    const int segMultiplier = 6;
	public MovingPoint end;
	public DynamicPoint start;
	public Status state=Status.Spawning;
	public float reach;
	public float reachModifier=0;
    int segments;
    LineRenderer r;
    Vector3[] points;

	const float animTime=0.7f;


	public void Animate(){
		if(state==Status.Spawning){
			reachModifier +=Time.deltaTime/animTime;
			if(reachModifier>1){
				reachModifier=1;
				state=Status.Normal;
			}
		}

		if(state==Status.Dying){
			reachModifier -=Time.deltaTime/animTime;
			if(reachModifier<0){
				reachModifier=0;
				state=Status.Dead;
				GameObject.Destroy(r.gameObject);
			}
		}

		end.Step();
		ManageSegments();
		r.SetPositions(points);
	}

    void ManageSegments() {
		float segLen = reachModifier * reach/segments;

		//back
		points[segments]=end;
		for(int j=1;j<segments;j++){
			Vector2 d = points[segments - j] - points[segments - j + 1];
			d = d.normalized * segLen;
			points[segments-j]=(Vector2)points[segments-j+1]+d;
		}

		//forward
		points[0]=this.start;
		for(int j=0;j<segments;j++){
			Vector2 d = points[j+1] - points[j];
			d = d.normalized * segLen;
			points[j+1]=(Vector2)points[j]+d;
		}
    }

	public IKLimb(int segments,float reach,
					   DynamicPoint start, MovingPoint end){
		GameObject obj = new GameObject("ik");
		obj.layer=7;
        r = obj.AddComponent<LineRenderer>();
        r.positionCount = segments+1;
        r.useWorldSpace = true;
        r.numCapVertices = 3;
        r.numCornerVertices = 3;
        r.material = Resources.Load<Material>("RayMaterial");

		points = new Vector3[segments+1];

		this.segments=segments;
		this.reach=reach;
		this.start=start;
		this.end=end;
	}

	public void SetWidth(float w) => SetWidthGradient(w,w);
	public void SetWidthGradient(float start,float end){
        r.startWidth = start;
        r.endWidth = end;
	}

	public void SetColour(Color c) => SetColourGradient(c,c);
	public void SetColourGradient(Color start,Color end){
        r.startColor = start;
        r.endColor = end;
	}

	public void Hide() => r.enabled=false;
	public void Show() => r.enabled=true;
}

}
