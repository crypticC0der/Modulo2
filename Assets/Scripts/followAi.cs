using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo {
public class followAi : MonoBehaviour {
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        fsm = GetComponent<EnemyFsm>();
    }

    EnemyFsm fsm;
    public float force;
    Rigidbody2D rb;
    bool valid=true;
    // float PathTime;
    // float AllTime;
    void Update() {
        if (World.stable) {
            Node current =
                World.HexCoordToNode(HexCoord.NearestHex(transform.position));
            force = fsm.Speed() * 10 * fsm.speedBonus;
            Node n=null;
            if(current.valid){
                n = current.next;
            }else{
                if(valid){
                    World.HotfixNode(current);
                }
            }
            valid= current.valid;
            Vector3 v;
            if (n != null) {
                fsm.distance = n.realDistance;
                v = n.Position() - transform.position;
                // PathTime+=Time.deltaTime;
            } else {
                v = World.orbTransform.position - transform.position;
                fsm.distance = v.magnitude;
            }
            // AllTime+=Time.deltaTime;
            // Debug.Log(100*PathTime/AllTime);
            rb.AddForce(v.normalized * Time.deltaTime * force / 0.02f);
        }
    }
}
}
