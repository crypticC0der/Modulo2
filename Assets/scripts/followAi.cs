using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followAi : MonoBehaviour
{
    void Start(){
        rb= GetComponent<Rigidbody2D>();
    }


    public float force;
    GameObject target;
    Rigidbody2D rb;
    void Update(){
        int[] wap = World.WorldPos(transform.position);
        Node n = World.grid[wap[0],wap[1]].GetNext();
        Vector3 v = new Vector3(n.x-wap[0],n.y-wap[1]);
        rb.AddForce(v.normalized*Time.deltaTime*force/0.02f);
    }

    public static GameObject closestTarget(Vector3 pos){
        GameObject target=null;
        float d =1000000000000000000;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject o in targets){
            float dx = (o.transform.position-pos).magnitude;
            if(dx<d){
                d=dx;
                target=o;
            }
        }
        return target;
    }
}
