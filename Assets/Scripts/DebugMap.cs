using UnityEngine;
public class DebugMap : MonoBehaviour{
	float t=0;
	public float interval=3;
	public void FixedUpdate(){
		t-=Time.deltaTime;
		if(t<0){
			t=interval;
			World.Print();
		}
	}
}
