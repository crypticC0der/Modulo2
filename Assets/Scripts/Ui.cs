using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MeshGen;
using TMPro;
using TMPro.EditorUtilities;

// the ui namespace will no longer be used, i am going to use more diagetic information schema
namespace Ui{

	public class Frame{
		Vector2 size;
		Vector2 position;
		Canvas canvas;
		Stack<Frameable> frameHistory;

		public Frame(Vector2 size,Vector2 position){
			GameObject gameObject = MeshGens.MinObjGen(Shapes.square,MatColour.background);
			gameObject.transform.position=position;
			gameObject.transform.localScale=(Vector3)(size*2)+Vector3.forward;
			canvas = gameObject.AddComponent<Canvas>();
			canvas.worldCamera=GameObject.Find("Main Camera").GetComponent<Camera>();
			gameObject.AddComponent<GraphicRaycaster>();
			((RectTransform)canvas.transform).sizeDelta=new Vector2(0.7f,0.7f);
			this.position=position;
			this.size=size;
		}

		public GameObject Render(GameObject o,Vector2 pos,Vector2 size){
			GameObject go = GameObject.Instantiate<GameObject>(o);
			go.transform.SetParent(canvas.transform);
			foreach(MonoBehaviour c in go.GetComponents<MonoBehaviour>()){
				GameObject.Destroy(c);
			}
			Rigidbody2D rb = (go.GetComponent<Rigidbody2D>());
			if(rb){
				GameObject.Destroy(rb);
			}
			Collider2D col = go.GetComponent<Collider2D>();
			Vector3 s = go.transform.localScale;
			if(col){
				Vector2 ex = col.bounds.extents;
				float boundMax;
				if(Mathf.Abs(1-ex.x)>Mathf.Abs(1-ex.y)){
					boundMax=ex.x;
				}else{
					boundMax=ex.x;
				}
				s/=boundMax;
				GameObject.Destroy(col);
			}
			size = Size(size);
			pos.y+=size.y/2;
			float form = canvas.transform.lossyScale.y/canvas.transform.lossyScale.x;
			size.y*=form/0.7f;
			s.x*=size.y*2;
			s.y*=size.y*2;
			go.transform.localScale=s;
			go.transform.localPosition=Position(pos);
			return go;
		}

		public TextMeshPro Render(object o,Vector2 pos,Vector2 size,float ts=0.6f){
			GameObject go = new GameObject();
			go.transform.SetParent(canvas.transform);
			TextMeshPro tm = (go).AddComponent<TextMeshPro>();
			tm.color=MeshGens.ColorFromHex(0xa9b1d6ff);
			size = Size(size);
			Debug.Log(size);
			pos.y+=size.y/2;
			tm.transform.localPosition=Position(pos);
			tm.text = o.ToString();
			tm.fontSize=ts;
			float form = canvas.transform.lossyScale.y/canvas.transform.lossyScale.x;
			size.y*=form;
			tm.rectTransform.sizeDelta=size;
			tm.transform.localScale=new Vector3(1,1/form,1);
			return tm;
		}

		public Image Render(Sprite o,Vector2 pos,Vector2 size){
			GameObject go = new GameObject();
			go.transform.SetParent(canvas.transform);
			Image im = (go).AddComponent<Image>();
			size = Size(size);
			pos.y+=size.y/2;
			im.transform.localPosition=Position(pos);
			im.sprite= o;
			float form = canvas.transform.lossyScale.y/canvas.transform.lossyScale.x;
			size.y*=form;
			im.rectTransform.sizeDelta=size;
			im.transform.localScale=new Vector3(1,1/form,1);
			return im;
		}

		public void Render(List<object> objects, Vector2 pos,Vector2 size){
			Color bg = MeshGens.ColorFromHex(0x1a1f2aff);
			Color bg_alt = MeshGens.ColorFromHex(0x1a1b26ff);
			Color fg = MeshGens.ColorFromHex(0xa9b1d6ff);
			MenuCommand menuCommand = new MenuCommand(canvas.gameObject,0);
			GameObject go = TMPro_CreateObjectMenu.ReturnDropdown(menuCommand);
			Vector3 s = size;
			float scale = 7f/100;
			Vector3 localScale = Vector3.one;
			go.transform.localPosition=Position(pos);
			if(size.x*this.size.x/16<this.size.y*size.y/3){
				scale *= size.x/16;
				localScale.y *= this.size.x/this.size.y;
			}else{
				scale*=size.y/3;
				localScale.x *= this.size.y/this.size.x;
			}
			// float scale = Mathf.Min(s.x/16,s.y/3);
			localScale *= scale;
			go.transform.localScale=localScale;
			TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();
			dropdown.options.Clear();
			foreach(object o in objects){
				TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(o.ToString());
				dropdown.options.Add(data);
			}
			ScrollRect sr = go.transform.GetChild(2).GetComponent<ScrollRect>();
			sr.scrollSensitivity=32;

			ColorBlock cb = new ColorBlock{
				colorMultiplier=1,
				disabledColor=bg,
				normalColor=bg,
				highlightedColor=bg,
				pressedColor=bg_alt,
				selectedColor=bg_alt
			};
			dropdown.colors=cb;
			Transform label = dropdown.transform.GetChild(0);
			Debug.Log(label.name);
			TextMeshProUGUI tmp = label.GetComponent<TextMeshProUGUI>();
			tmp.color=fg;
			sr.GetComponent<Image>().color=bg;
			Toggle item = sr.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Toggle>();
			item.colors=cb;
			item.transform.GetChild(2).GetComponent<TextMeshProUGUI>().color=fg;
			cb.pressedColor=fg;
			cb.selectedColor=fg;
			sr.verticalScrollbar.colors=cb;
			sr.verticalScrollbar.GetComponent<Image>().color=bg;
		}

		public void Render(List<Frameable> frames, string title,Vector2 pos,Vector2 size){

		}

		public Vector2 Size(Vector2 v){
			return new Vector2(.7f*v.x,0.7f*v.y);
		}

		public Vector2 Position(Vector2 v){
			Vector2 p = position;
			p.x+=0.7f*v.x - 0.35f;
			p.y+=0.35f-0.7f*v.y;
			return p;
		}
	}

	public interface Frameable{
		public void Draw(Frame f);
	}
}
