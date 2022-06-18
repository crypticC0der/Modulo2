using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct SpriteSize{
    public Sprite sprite;
    public Vector3 size;
}

public class UIControl : MonoBehaviour{
    public uiBar[] bars;
    public GameObject panel;
    public GameObject craftPan;
    Transform mainCanv;
    SpriteRenderer holdimg;
    public SpriteRenderer inWorldClone;
    bool invDown=false;
    bool cftDown=false;
    bool plcDown=false;

    public void Render(){
        foreach (Transform c in panel.transform){GameObject.Destroy(c.gameObject);}
        PlayerBehavior.deck.Render(panel);
    }

    public void Start(){
        inventory=new Slider((RectTransform)panel.transform,new Vector3(0,-172.5f,0),new Vector3(0,-66.5f,0));
        crafting =new Slider((RectTransform)craftPan.transform,new Vector3(-356,0,0),craftPan.transform.position,2048,1024);
        mainCanv = panel.transform.parent;
        GameObject holding = new GameObject("itemHeld");
        // holding.transform.SetParent(mainCanv);
        holdimg = holding.AddComponent<SpriteRenderer>();
        GameObject holdclone = GameObject.Instantiate(holding);
        inWorldClone = holdclone.GetComponent<SpriteRenderer>();
        inWorldClone.color = new Color(0,0,0,0.5f);
    }

    Slider crafting;
    Slider inventory;
    public void Update(){

        Vector3 p= Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z =0;

        if(!inventory.slidin && !crafting.slidin && !invDown && Input.GetButton("inventory")){
            crafting.ToStart();
            inventory.Toggle();
        }

        if(!inventory.slidin && !crafting.slidin && !cftDown && Input.GetButton("crafting")){
            inventory.ToStart();
            crafting.Toggle();
        }
        if(Input.GetButton("place") && !plcDown && PlayerBehavior.holding!=null){
            if(!crafting.OnSlider(p) && !inventory.OnSlider(p)){
                PlayerBehavior.Place();
            }
        }

        if(Input.GetButtonDown("uiToggle")){
            if(World.gridObj!=null){
                GameObject.Destroy(World.gridObj);
                World.gridObj=null;
            }else{
                World.GenGrid();
            }
        }

        inventory.Update();
        crafting.Update();
        plcDown = Input.GetButton("place");
        invDown = Input.GetButton("inventory");
        cftDown = Input.GetButton("crafting");

        if (PlayerBehavior.holding!=null){
            SpriteSize ss = PlayerBehavior.holding.GetSpriteSize();
            holdimg.sprite = ss.sprite;
            inWorldClone.sprite = ss.sprite;
            holdimg.transform.localScale=ss.size;
            inWorldClone.transform.localScale=ss.size;
            holdimg.gameObject.transform.position=p;
            inWorldClone.gameObject.transform.position=World.NearestHex(p);
        }else{
            holdimg.sprite=null;
            inWorldClone.sprite=null;
        }

    }

    class Slider{
        RectTransform t;
        public bool slidin=false;
        Vector3[] aims;
        float intSpd;
        float acc;
        float spd;
        Vector3 direction;
        int i=0;

        public Slider(RectTransform trans,Vector3 start,Vector3 end,float initialSpd=938,float acceleration=640){
            direction=new Vector3(0,0,0);
            intSpd=initialSpd;
            acceleration=640;
            spd=0;
            t=trans;
            aims = new Vector3[]{start,end};
            t.localPosition=start;
        }

        public void Toggle(){
            i=1-i;
            Start();
        }

        public void ToStart(){
            i=0;
            Start();
        }

        public void ToEnd(){
            i=1;
            Start();
        }

        void Start(){
            direction = (aims[i]-t.localPosition).normalized;
            spd=intSpd;
            slidin=true;
        }

        public void Update(){
            if(slidin){
                spd+=acc*Time.deltaTime;
                t.localPosition+=direction*spd*Time.deltaTime;
                if(Vector3.Dot(aims[i]-t.localPosition,direction)<=0){
                    t.localPosition=aims[i];
                    slidin=false;
                }
            }
        }

        public bool OnSlider(Vector3 position){
            return t.rect.Contains(t.InverseTransformPoint(position));
        }
        
    }

}
