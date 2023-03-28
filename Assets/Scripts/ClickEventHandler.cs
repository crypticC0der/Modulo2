using UnityEngine;
using System.Collections.Generic;

namespace Modulo {
public class ClickEventHandler : MonoBehaviour {
    public delegate void ClickTodo(Vector3 point);
    public Queue<ClickTodo>[] todoList = new Queue<ClickTodo>[]{
        new Queue<ClickTodo>(),
        new Queue<ClickTodo>()
    };

    public void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Collider2D hit;
        Vector3 worldPoint =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
        for(int i=0;i<2;i++){
            if (Input.GetMouseButtonDown(i)) {
                if (todoList[i].Count != 0) {
                    todoList[i].Dequeue()(worldPoint);
                } else {
                    hit = Physics2D.OverlapCircle(worldPoint, 0.5f);
                    if (hit) {
                        Debug.Log(hit.name);
                        Clickable d = hit.GetComponent<Clickable>();
                        if (d!=null) {
                            if(i==0){
                                d.LeftClick(this);
                            }else{
                                d.RightClick(this);
                            }
                        }
                    }
                }
            }
        }
    }

    LineRenderer r;
    public void Start(){
        r = Attack.MinimalRay(Color.red,Color.blue,
                    new Vector3[]{transform.position,new Vector3(0,0,0)});
        r.startWidth=0.1f;

    }

    public float heuristic;
    public Vector3[] pos;
    public void LateUpdate(){
        pos = new Vector3[]{World.orbTransform.position,
            Camera.main.ScreenToWorldPoint(Input.mousePosition)};
        r.enabled=World.debug;
        Node mn = World.HexCoordToNode(HexCoord.NearestHex(pos[1]));
        pos[1] = (mn.Position());
        heuristic=mn.Heuristic();
        r.SetPositions(pos);
    }
}

public interface Clickable{
    public void LeftClick(ClickEventHandler e);
    public void RightClick(ClickEventHandler e);
}

}
