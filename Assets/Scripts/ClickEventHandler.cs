using UnityEngine;
using System.Collections.Generic;

namespace Modulo {
public class ClickEventHandler : MonoBehaviour {
    public delegate void ClickTodo(Vector3 point);
    public Queue<ClickTodo>[] todoList = new Queue<ClickTodo>[]{
        new Queue<ClickTodo>(),
        new Queue<ClickTodo>(),
        new Queue<ClickTodo>(),
        new Queue<ClickTodo>()
    };

    public void SendClick(int mode, Vector3 at){
        Collider2D[] hits = Physics2D.OverlapCircleAll(at, 0.5f);
        foreach(Collider2D hit in hits) {
            Clickable d = hit.GetComponent<Clickable>();
            if (d!=null) {
                switch (mode) {
                    case 0: d.LeftClick(this); break;
                    case 1: d.RightClick(this); break;
                    case 2: d.LeftClickHold(this); break;
                    case 3: d.LeftClickHold(this); break;
                    case 4: d.Hover(this); break;
                }
            }
        }
    }

    public void HandleClick(int mode,Vector3 at){
        bool clicked=true;
        switch (mode) {
            case 0: clicked=Input.GetMouseButtonDown(0); break;
            case 1: clicked=Input.GetMouseButtonDown(1); break;
            case 2: clicked=Input.GetMouseButton(0); break;
            case 3: clicked=Input.GetMouseButton(1); break;
        }


        if (clicked) {
            if (todoList[mode].Count != 0) {
                todoList[mode].Dequeue()(at);
            } else {
                SendClick(mode,at);
            }
        }

    }

    public void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
        for(int i=0;i<4;i++){
            HandleClick(i,worldPoint);
        }
        SendClick(4,worldPoint);
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
    public void LeftClickHold(ClickEventHandler e);
    public void RightClickHold(ClickEventHandler e);
    public void Hover(ClickEventHandler e);
}

}
