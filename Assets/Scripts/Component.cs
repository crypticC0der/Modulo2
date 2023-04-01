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

    public static void SpawnComponent(Component.Id id, int amount,
                                      Vector3 position) {
        for (int e = 3; e >= 0; e--) {
            int p = (int)Mathf.Pow(10, e);
            while (p <= amount) {
                amount -= p;
                components[(int)id].CreateComponent(p,
                                position + new Vector3(Random.Range(-1f, 1f),
                                                       Random.Range(-1f, 1f)),
                                (e + 1) / 4f);
            }
        }
    }

	public GameObject CreateComponent(int amount,
											Vector3 position, float size) {
		GameObject o = new GameObject(name+" orb");
		ComponentData d = o.AddComponent<ComponentData>();
		d.amount= amount;
		d.id=(int)id;
		d.transform.localScale = new Vector3(size, size);
		d.transform.position = position;
		o.layer = 8;
		SpriteRenderer sr = o.AddComponent<SpriteRenderer>();
		sr.sprite = baseSprite;
		sr.color=colour;
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
		sr.sprite = ItemTemplate.GetGraphic(name+"Converter");

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
}


public class ComponentData : MonoBehaviour {
    public float time = 0.5f;
    public int amount;
	public int id;

	public IEnumerable<PicksComponent> PickerNumerator(){
		if(id==(int)Component.Id.Blue){
			foreach(EnemyFsm enemy in EnemyFsm.enemiesList){
				yield return (PicksComponent)enemy;
			}
		}else{
			yield return (PicksComponent)PlayerBehavior.me;
		}
	}

    private void FixedUpdate() {
        time -= Time.deltaTime;
        if (time < 0 && PlayerBehavior.me != null) {
			Vector3 force = Vector3.zero;
			foreach(PicksComponent pick in PickerNumerator()){
				Vector3 d = pick.Distance(transform.position);
				if(d.magnitude>0.5f){
					force+=Force(d);
				}else{
					pick.CollectComponent(this);
					Destroy(gameObject);
					return;
				}
			}
			transform.position += force * Time.deltaTime;
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
