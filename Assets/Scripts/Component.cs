using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Modulo{
public class Component{
	public Color colour;
	public string name;
	public Id id;
	public TextMeshPro text;
	static Sprite _baseSprite=null;

	static Component[] components =
        new Component[] {
            new Component("grey",0x808080ff,Component.Id.Grey ),
            new Component("blue",0x150e79ff,Component.Id.Blue ),
            new Component("pink",0xff80ccff,Component.Id.Pink ),
            new Component("yellow",0xe7ff00ff,Component.Id.Yellow )
        };

	static private Sprite baseSprite{
		get{
			if(_baseSprite!=null){
				return _baseSprite;
			}else{
				_baseSprite = ItemTemplate.GetGraphic("orb2");
				return _baseSprite;
			}
		}
		set{}
	}

	public enum Id{
		Grey,
		Blue,
		Pink,
		Yellow,
	}

	const float spawnRange=0.5f;
    public static void SpawnComponent(Component.Id id, int amount,
                                      Vector3 position) {
        for (int e = 5; e >= 0; e--) {
            int p = (int)Mathf.Pow(10, e);
            while (p <= amount) {
				Vector3 to = position;
				to.x+=Random.Range(-spawnRange, spawnRange)*(e+1)/2;
				to.y+=Random.Range(-spawnRange, spawnRange)*(e+1)/2;
                amount -= p;
                components[(int)id].CreateComponent(p,(e + 1) / 4f,position,to);
            }
        }
    }

    public static void SendComponents(Component.Id id, int amount,
                                      Alter reciver) {
        for (int e = 3; e >= 0; e--) {
            int p = (int)Mathf.Pow(10, e);
            while (p <= amount) {
				Vector3 pos = PlayerBehavior.me.transform.position;
				pos.x+=Random.Range(-spawnRange, spawnRange)*(e+1)/2;
				pos.y+=Random.Range(-spawnRange, spawnRange)*(e+1)/2;

                amount -= p;
                components[(int)id].DummyComponent(p,pos, (e + 1) / 4f,reciver);
            }
        }
    }

	public GameObject SimpleComponent(int amount, Vector3 position, float size){
		GameObject o = new GameObject(name+" orb");
		o.transform.localScale = new Vector3(size, size);
		o.transform.position = position;
		o.layer = 8;
		SpriteRenderer sr = o.AddComponent<SpriteRenderer>();
		sr.sprite = baseSprite;
		sr.color=colour;
		return o;
	}

	public GameObject DummyComponent(int amount, Vector3 position,
									 float size, Alter reciver) {
		GameObject o = SimpleComponent(amount,position,size);
		o.AddComponent<ComponentSend>().to=reciver.transform.position;
		return o;
	}

	public GameObject CreateComponent(int amount, float size,
									  Vector3 start, Vector3 to) {
		GameObject o = SimpleComponent(amount,start,size);
		ComponentData d = new ComponentData();
		d.amount= amount;
		d.id=(int)id;
		ComponentSend cs = o.AddComponent<ComponentSend>();
		cs.data=d;
		cs.to=to;

		return o;
	}

	public static void UpdateComponentUI(Component.Id type,int val){
		if(type==Id.Blue){return;}
		components[(int)type].UpdateComponentUI(val);
	}

	public void UpdateComponentUI(int val){
		text.text = val.ToString();
	}

	public static Alter CreateAlter(Component.Id type, HexCoord location){
		return components[(int)type].CreateAlter(location);
	}

	public Alter CreateAlter(HexCoord location){
		Vector3 p = location.position();
		GameObject alter = new GameObject(name+ " Alter");
		alter.transform.position=p;
		SpriteRenderer sr = alter.AddComponent<SpriteRenderer>();
		SpriteSize ss = ItemTemplate.GetSpriteSize(name+"Converter");
		sr.sprite = ss.sprite;
		alter.transform.localScale=ss.size;

		GameObject gear = new GameObject("gear");
		gear.transform.SetParent(alter.transform);
		gear.transform.localPosition=new Vector3(0,0,-1);
		Sprite gearSprite = ItemTemplate.GetGraphic("gear");
		gear.AddComponent<SpriteRenderer>().sprite = gearSprite;

		World.UpdateState(location ,NodeState.ground,ChangeStateMethod.On);
		Collider2D col = alter.AddComponent<PolygonCollider2D>();
		alter.layer = 9;

		Alter altComp=null;
		switch (id){
			case Id.Pink:
				altComp=alter.AddComponent<SharpeningAlter>();
				break;
			case Id.Yellow:
				altComp=alter.AddComponent<RefiningAlter>();
				break;
		}
		return altComp;
	}

	Component(string name, uint colour, Id id){
		this.colour=MeshGen.MeshGens.ColorFromHex(colour);
		this.id=id;
		this.name=name;
		GameObject tbox = GameObject.Find(name+"Text");
		if(tbox){
			text = tbox.GetComponent<TextMeshPro>();
			text.color=this.colour;
		}
		GameObject imgobject = GameObject.Find(name+"Img");
		if(imgobject){
			Image image = imgobject.GetComponent<Image>();
			image.color=this.colour;
			image.sprite=baseSprite;
		}
	}

	public static Color ComponentColour(Component.Id id){
		return components[(int)id].colour;
	}
}

public class ComponentData{
    public float time = 0.5f;
    public int amount;
	public int id;
}

public class ComponentSend : MonoBehaviour{
	const float speed=5;
	public Vector3 to;
	public ComponentData data;
	void FixedUpdate(){
		Vector3 d = to-transform.position;
		if(d.magnitude<0.3f){
			if(data!=null){
				//send to recive
				gameObject.AddComponent<ComponentRecive>().data = data;
				Destroy(this);
			}else{
				Destroy(gameObject);
			}
		}
		transform.position += d.normalized*speed*Time.deltaTime;
	}
}

public class ComponentRecive : MonoBehaviour {
	public ComponentData data;

	public IEnumerable<PicksComponent> PickerNumerator(){
		if(data.id==(int)Component.Id.Blue){
			foreach(EnemyFsm enemy in EnemyFsm.enemiesList){
				yield return (PicksComponent)enemy;
			}
		}else{
			yield return (PicksComponent)PlayerBehavior.me;
		}
	}

    private void FixedUpdate() {
        data.time -= Time.deltaTime;
        if (data.time < 0 && PlayerBehavior.me != null) {
			Vector3 force = Vector3.zero;
			foreach(PicksComponent pick in PickerNumerator()){
				Vector3 d = pick.Distance(transform.position);
				if(d.magnitude>0.5f){
					force+=Force(d);
				}else{
					pick.CollectComponent(data);
					Destroy(gameObject);
					return;
				}
			}
			force*=Time.deltaTime/(2*transform.localScale.x);
			transform.position += force;
        }
    }

    public Vector3 Force(Vector3 d) {
        float f = 30 / (d.magnitude * d.magnitude + 5);
        return d.normalized * f;
    }
}

public interface PicksComponent{
	public Vector3 Distance(Vector3 point);
    public void CollectComponent(ComponentData data);
}
}
