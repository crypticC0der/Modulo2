using UnityEngine;
using System.Collections.Generic;
using MUtils;
using MeshGen;

namespace Modulo{
	public class Cauldron : MonoBehaviour{
		delegate bool canRotate();

		int InserterSum(){
			return inserters[0].contains +
				inserters[1].contains +
				inserters[2].contains;
		}

		public void LeftClick(ClickEventHandler e)  {
			inserters[focus].Insert();
		}

		public void RightClick(ClickEventHandler e){
			focus=(focus+1)%inserters.Length;
			RotateArrow();
		}

		public void LeftClickHold(ClickEventHandler e){
			inserters[focus].InsertCont();
		}

		public void OnMouseOver(){
			hovered=true;
			MouseChange.ChangeName("hand");
			MouseChange.ChangeOutline(
				Component.ComponentColour(inserters[focus].component));
			// Mouse.SetColour(inserters[focus].component);
		}

		int focus=0;
		ComponentInserter[] inserters;
		HexBorder[] borders;
		public float timer=5;
		AboveText at;
		public void Start(){
			canRotate r = () => timer < 3 && InserterSum()>0;
			AddGear(1*Mathf.PI/3,r,
					Component.ComponentColour(Component.Id.Yellow));
			AddGear(3*Mathf.PI/3,r,
					Component.ComponentColour(Component.Id.Grey));
			AddGear(5*Mathf.PI/3,r,
					Component.ComponentColour(Component.Id.Pink));

			ComponentInserter.OnInsert hook = ()=>{
				timer=5;
				at.SetText(InserterSum());
			};
			inserters = new ComponentInserter[3]{
				new ComponentInserter(Component.Id.Pink,transform,() => {},hook),
				new ComponentInserter(Component.Id.Yellow,transform,() => {},hook),
				new ComponentInserter(Component.Id.Grey,transform,() => {},hook)
			};
			borders = new HexBorder[2]{
				new HexBorder(Component.ComponentColour(Component.Id.Pink)),
				new HexBorder(Component.ComponentColour(Component.Id.Yellow))
			};
			GameObject perent = new GameObject("cauldron");
			perent.transform.position=transform.position;
			transform.SetParent(perent.transform);
			transform.localPosition=Vector3.zero;
			borders[0].setParent(perent.transform);
			borders[1].setParent(perent.transform);
			borders[0].clockwise=false;
			arrow =
				MeshGens.MinObjGen(Shapes.arrow,MatColour.white)
				.GetComponent<Renderer>();
			arrow.transform.SetParent(perent.transform);
			RotateArrow();
			at=new AboveText(transform);
			at.SetColour(Component.ComponentColour(Component.Id.Grey));
		}

		Renderer arrow;
		void RotateArrow(){
			float r = -120f*(focus+1);
			arrow.transform.localEulerAngles=r*Vector3.forward;
			arrow.transform.localPosition=-World.AngleToVec(-r*Mathf.PI/180f)*1.2f;
		}

		class Rotate : MonoBehaviour{
			public canRotate condition{set; private get;}
			const float spd=180;
			public void FixedUpdate(){
				if(condition()){
					transform.eulerAngles += 180*Vector3.forward*Time.deltaTime;
				}
			}
		}

		void Craft(){
			Item obtained = Crafting.Obtain(inserters[1].contains,
							inserters[0].contains,
							inserters[2].contains);
			inserters[0].Reset();
			inserters[1].Reset();
			inserters[2].Reset();
			at.SetText(0);
			PlayerBehavior.me.AddToDeck(obtained);
		}

		public void Update(){
			Animate();
			if(InserterSum()<=0){return;}
			if(timer>0){
				timer-=Time.deltaTime;
			}else{
				Craft();
			}
		}

		bool hovered=false;
		void Animate(){
			if(timer<3 || InserterSum()==0){at.Hide();}
			else{at.Show();}

			if(hovered!=arrow.enabled){
				arrow.enabled=hovered;
			}
			hovered=false;
			if(InserterSum()<=0){return;}
			float v = Mathf.Min(timer/3,1) / (InserterSum());

			borders[0].updateLR(v*inserters[0].contains);
			borders[1].updateLR(v*inserters[1].contains);
		}

		const float Radius=0.35f;
		void AddGear(float r,canRotate condition,Color c){
			GameObject gear = new GameObject("gear");
			gear.transform.localScale*=1.1f;
			gear.transform.SetParent(gameObject.transform);
			gear.transform.localPosition = Radius *
				new Vector3(Mathf.Sin(r),Mathf.Cos(r),-1);
			SpriteRenderer sr = gear.AddComponent<SpriteRenderer>();
			sr.sprite = ItemTemplate.GetGraphic("gear");
			sr.color=c;
			sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			gear.AddComponent<Rotate>().condition=condition;
		}

		public static GameObject SpawnCauldron(HexCoord hc){
			GameObject cauldron = new GameObject("cauldron");
			cauldron.AddComponent<Cauldron>();
			cauldron.transform.position = hc.position();
			SpriteRenderer sr = cauldron.AddComponent<SpriteRenderer>();
			SpriteSize spriteSize = ItemTemplate.GetSpriteSize("base");
			sr.sprite = spriteSize.sprite;
			cauldron.transform.localScale = spriteSize.size;
			SpriteMask sm = cauldron.AddComponent<SpriteMask>();
			sm.sprite=spriteSize.sprite;
			cauldron.AddComponent<PolygonCollider2D>();
			cauldron.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			return cauldron;
		}


	}

}
