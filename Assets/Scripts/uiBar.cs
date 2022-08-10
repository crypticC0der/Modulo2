using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo{
	public interface Bar{
		public void Reset();
		public void Zero();
		public void SetValue(float current,float max);
		public void UpdateValue(float current,float max);
		public void Enable(bool b);
		public void Delete();
	}

	public class uiBar : MonoBehaviour,Bar
	{
		public Color[] colors;
		public float[] intervals;
		public Transform angle;
		public void Reset(){
			transition=0;
			current=1;
			aim=1;
			SetValue(1,1);
		}
		public void Zero(){UpdateValue(0,1);}
		SpriteRenderer renderer;
		float current;
		float aim;
		float transition;

		public void Start(){
			renderer=GetComponent<SpriteRenderer>();
			Reset();
		}
		public void UpdateValue(float current,float max){
			this.aim=current/max;
			transition=3;
		}

		public void Enable(bool b){
			renderer.enabled=false;
		}

		public void Delete(){
			Destroy(renderer.gameObject);
			Destroy(this);
		}

		public Color Remap(float v){
			float t = Mathf.InverseLerp(intervals[0],intervals[1],v);
			return Color.LerpUnclamped(colors[0],colors[1],t);
		}

		public void Update(){
			if(transition>Time.deltaTime){
				current += (aim-current)/transition;
				transition-=Time.deltaTime;
				SetValue(current,1);
			}else if(transition!=0){
				transition=0;
				current=aim;
				SetValue(aim,1);
			}
		}


		public void SetValue(float current,float max){
			Color c = Remap(current/max);
			renderer.color=c;
			renderer.gameObject.GetComponent<SpriteRenderer>().enabled=current>=0.002f;
			Vector3 p = transform.localPosition;
			Vector3 s = transform.localScale;
			float scale = 4.8f + 1.125f*p.y;
			s.x=(current/max)*(scale);
			p.x=-(scale-s.x)/2-0.53f+0.525f*p.y;
			transform.localPosition=p;
			transform.localScale=s;
			p.x+=s.x/2-0.185f;
			p.z+=1;
			angle.localPosition=p;
		}
	}
}
