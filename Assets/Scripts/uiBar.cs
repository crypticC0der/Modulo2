using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public void Reset(){UpdateValue(1,1);}
    public void Zero(){UpdateValue(0,1);}
    SpriteRenderer renderer;
    SpriteRenderer childRenderer;
    float current;
    float aim;
    float transition;

    public void Start(){
        perentScale=transform.lossyScale.x/transform.localScale.x;
        renderer=GetComponent<SpriteRenderer>();
        childRenderer=angle.gameObject.GetComponent<SpriteRenderer>();
        Reset();
    }
    public void UpdateValue(float current,float max){
        this.aim=current/max;
        transition=3;
    }

    public void Enable(bool b){
        renderer.enabled=false;
        childRenderer.enabled=false;
    }

    public void Delete(){
        Destroy(renderer.gameObject);
        Destroy(childRenderer.gameObject);
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

    float perentScale;

    public void SetValue(float current,float max){
        Vector3 v = transform.localScale;
        v.x=(current/max)*(.5f+transform.localPosition.y);
        transform.localScale=v;
        v = transform.localPosition;
        Color c = Remap(current/max);
        renderer.color=c;
        childRenderer.color=c;
        v.x = 12f* 2/perentScale+ (11 * 2/perentScale-transform.localScale.x)/2;
        transform.localPosition=v;

        v.x-=(transform.localScale.x/2 + 0.375f* 2/perentScale);
        angle.transform.localPosition=v;
        if(current<=0.002f){angle.gameObject.GetComponent<SpriteRenderer>().enabled=false;}
    }
}
