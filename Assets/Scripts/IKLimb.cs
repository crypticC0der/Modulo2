using UnityEngine;
using System.Collections;

namespace MUtils{
public class IKLimb{
    const int segMultiplier = 6;

	public MovingPoint end;
	public DynamicPoint start;
	float reach;
    int segments;
    LineRenderer r;
    Vector3[] points;

	public void Animate(){
		end.Step();
		ManageSegments();
		r.SetPositions(points);
	}

    void ManageSegments() {
		float segLen = reach/segments;

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
