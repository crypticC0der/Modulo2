using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiBar : MonoBehaviour
{
    public Transform angle;
    public void Start(){Reset();}
    public void Reset(){UpdateValue(1,1);}
    public void Zero(){UpdateValue(0,1);}

    float current;public float aim;float transition;

    public void UpdateValue(float current,float max){
        this.aim=current/max;
        transition+=4;
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
        Vector3 v = transform.localScale;
        v.x=(current/max)*(.5f+transform.position.y);
        transform.localScale=v;

        v = transform.position;
        v.x = 12f + (11-transform.localScale.x)/2;
        transform.position=v;

        v.x-=transform.localScale.x/2 + 0.375f;
        angle.transform.position=v;
        Debug.Log(current);
        if(current<=0.002f){angle.gameObject.GetComponent<SpriteRenderer>().enabled=false;}
    }
}
