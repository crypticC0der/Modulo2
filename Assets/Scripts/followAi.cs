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
    void Update() {
        if(World.stable){
            Node current = World.HexCoordToNode(
                HexCoord.NearestHex(transform.position));
            force = fsm.Speed() * 10 * fsm.speedBonus;
            Node n = current.next;
            Vector3 v;
            if (n != null) {
                fsm.distance = n.realDistance;
                v = n.hc.position() - transform.position;
            } else {
                v = World.orbTransform.position - transform.position;
                fsm.distance = v.magnitude;
            }
            rb.AddForce(v.normalized * Time.deltaTime * force / 0.02f);
        }
    }
}
}
