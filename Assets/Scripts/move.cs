using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        wop = World.WorldPos(transform.position);
        World.MovePlayer(wop[0],wop[1]);
        pwop = wop;
    }


    // Update is called once per frame
    public float force;
    public int[] wop;
    public int[] pwop;
    float sprint=2;
    float sprintTime=0;
    void Update()
    {
        wop = World.WorldPos(transform.position);
        if(wop[0]!=pwop[0] || wop[1] != pwop[1]){
            World.MovePlayer(wop[0],wop[1]);
        }
        Vector3 v=new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"))*Time.deltaTime;
        if(Input.GetButton("Sprint")&sprint>0){
            v*=2;
            sprintTime=0;
            sprint-=Time.deltaTime;
            PlayerBehavior.controller.bars[1].SetValue(sprint,2);
        }
        Debug.Log(sprint);
        sprintTime+=Time.deltaTime;
        if(sprintTime>2){
            if(sprint<2){
                sprint+=Time.deltaTime;
                PlayerBehavior.controller.bars[1].SetValue(sprint,2);
            }else if(sprint>2){
                sprint=2;
                PlayerBehavior.controller.bars[1].SetValue(sprint,2);
            }
        }
        rb.AddForce(v*force/0.02f);
        pwop = wop;
    }
}
