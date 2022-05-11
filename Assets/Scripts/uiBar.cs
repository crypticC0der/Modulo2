using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiBar : MonoBehaviour
{
    public Transform angle;
    public void Start(){Reset();}
    public void Reset(){UpdateValue(1,1);}
    public void Zero(){UpdateValue(0,1);}
    public void UpdateValue(float current,float max){
        Vector3 v = transform.localScale;
        v.x=(current/max)*(.5f+transform.position.y);
        transform.localScale=v;

        v = transform.position;
        v.x = 12f + (11-transform.localScale.x)/2;
        transform.position=v;

        v.x-=transform.localScale.x/2 + 0.375f;
        angle.transform.position=v;
    }
}
