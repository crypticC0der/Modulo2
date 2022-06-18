using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followAi : MonoBehaviour
{
    void Start(){
        rb= GetComponent<Rigidbody2D>();
        fsm=GetComponent<EnemyFsm>();
    }


    EnemyFsm fsm;
    public float force;
    Rigidbody2D rb;
    void Update(){
        force=fsm.speed*10*fsm.speedBonus;
        int[] wap = World.WorldPos(transform.position);
        Node n = World.grid[wap[0],wap[1]].GetNext();
        fsm.distance=n.GetDistReal();
        Vector3 v = n.WorldPos()-transform.position;
        v=v.normalized;
        rb.AddForce(v.normalized*Time.deltaTime*force/0.02f);
    }
}
