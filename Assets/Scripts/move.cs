using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modulo {
public class move : MonoBehaviour {
    // Start is called before the first frame update
    Rigidbody2D rb;
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        wop = World.WorldPos(transform.position);
        World.PlaceOrb(wop[0], wop[1]);
        pwop = wop;
        World.orbTransform = transform;
    }

    // Update is called once per frame
    public float force;
    public int[] wop;
    public int[] pwop;
    float sprint = 2;
    float sprintTime = 0;
    public static float healTime = 1;
    void Update() {
        wop = World.WorldPos(World.orbTransform.position);
        if (wop[0] != pwop[0] || wop[1] != pwop[1]) {
            World.PlaceOrb(wop[0], wop[1]);
        }
        Vector3 v = new Vector3(Input.GetAxis("Horizontal"),
                                Input.GetAxis("Vertical")) *
                    Time.deltaTime;
        v *= PlayerBehavior.me.UniversalSpeedCalculator(1);
        if (v.x == 0 && v.y == 0) {
            healTime += Time.deltaTime;
        } else {
            healTime = 0;
            if (Input.GetButton("Sprint") & sprint > 0) {
                v *= 2;
                sprintTime = 0;
                sprint -= Time.deltaTime;
                PlayerBehavior.controller.bars[1].SetValue(sprint, 2);
            }
        }
        sprintTime += Time.deltaTime;
        if (sprintTime > 1f) {
            float multiplier = 0.1f;
            if (sprintTime > 2) {
                multiplier = 1;
            }
            if (sprint < 2) {
                sprint += Time.deltaTime * multiplier;
                PlayerBehavior.controller.bars[1].SetValue(sprint, 2);
            } else if (sprint > 2) {
                sprint = 2;
                PlayerBehavior.controller.bars[1].SetValue(sprint, 2);
            }
        }
        rb.AddForce(v * force / 0.02f);
        pwop = wop;
    }
}
}
