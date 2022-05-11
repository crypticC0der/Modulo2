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
        World.Generate(new int[]{36,22},new bool[36,22]);
        wop = World.WorldPos(transform.position);
        World.MovePlayer(wop[0],wop[1]);
        pwop = wop;
    }

    // Update is called once per frame
    public float force;
    public int[] wop;
    public int[] pwop;
    void Update()
    {
        wop = World.WorldPos(transform.position);
        if(wop[0]!=pwop[0] || wop[1] != pwop[1]){
            World.MovePlayer(wop[0],wop[1]);
        }
        Vector3 v=new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"))*Time.deltaTime;
        rb.AddForce(v*force/0.02f);
        pwop = wop;
    }
}
