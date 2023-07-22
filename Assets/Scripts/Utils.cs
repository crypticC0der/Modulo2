using System.Collections.Generic;
using UnityEngine;
namespace MUtils{
	public class HexBorder{
		public static float hexSize = 1 / Mathf.Sqrt(3);
		public static Vector2 hexVec = new Vector2(1f / 2, Mathf.Sqrt(3) / 2);

		const float w=0.08f;
		LineRenderer lr;
		public bool clockwise=true;

		public void updateLR(float progress){
			if(progress>1){progress=1;}
			else if(progress<0){progress=0;}
			progress*=6;
			int points = (int)Mathf.Ceil(progress);
			int vertexes= (int)Mathf.Floor(progress);
			Vector3[] pointsList = new Vector3[points+1];
			for(int i =0;i<points+1;i++){
				float r = 2*Mathf.PI*i/6f;
				if(!clockwise){
					r=-r;
				}
				pointsList[i] = (hexSize- w/2) * new Vector3(
					Mathf.Sin(r),
					Mathf.Cos(r));
			}
			if(points!=vertexes){
				pointsList[points] = Vector3.Lerp(pointsList[vertexes],
													pointsList[points],
													progress-vertexes);
			}
			lr.positionCount=points+1;
			lr.SetPositions(pointsList);
		}

		public void Destroy() => GameObject.Destroy(lr.gameObject);


		public void setParent(Transform t){
			lr.transform.SetParent(t);
			lr.transform.localPosition=Vector3.zero;
		}

		public HexBorder(Color c){
			GameObject go = new GameObject("hb");
			lr = go.AddComponent<LineRenderer>();
			go.layer=7;
			lr.useWorldSpace = false;
			lr.numCapVertices = 0;
			lr.numCornerVertices = 0;
			lr.material = Resources.Load<Material>("RayMaterial");
			lr.startWidth=w;
			lr.endWidth=w;
			lr.startColor=c;
			lr.endColor=c;
		}
	}
}
