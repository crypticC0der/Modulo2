using UnityEngine;
using System.Collections.Generic;

//this code is obselete
//dont use it

namespace Modulo {
public class Mouse{
	static Mouse m=null;
	MouseFollow mf;


	public Color GetColour() => mf.GetColor();
	public void SetColour(Component.Id colourID){
		mf.SetColor(Component.ComponentColour(colourID));
	}

	public static Mouse Get(){
		if(m==null){
			m = new Mouse();
			Cursor.visible=false;
		}
		return m;
	}

	private Mouse(){
		//this is sneaky and reuses alot of provate component methods
		//i dont like this
		GameObject o = new GameObject("mouse");
		o.transform.localScale = Vector3.one*0.5f;
		o.layer = 8;
		SpriteRenderer sr = o.AddComponent<SpriteRenderer>();
		sr.sprite = ItemTemplate.GetGraphic("orb2");
		mf = o.AddComponent<MouseFollow>();
	}

	class MouseFollow : MonoBehaviour{
		Renderer r;
		bool changed=false;

		public void Start() => r=GetComponent<Renderer>();

		public Color GetColor() => r.material.color;
		public void SetColor(Color c){
			r.material.color=c;
			changed=true;
		}

		public void Update(){
			if(!changed){
				r.material.color=Component.ComponentColour(Component.Id.Blue);
			}
			changed=false;

			Vector3 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			v.z=-9;
			transform.position=v;
		}
	}
}
}
