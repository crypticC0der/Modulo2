using UnityEngine;
using System.Collections.Generic;

namespace Modulo {
public class ClickEventHandler : MonoBehaviour {
    public delegate void ClickTodo(Vector3 point);
    public Queue<ClickTodo> todoList = new Queue<ClickTodo>();
    public void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Collider2D hit;
        if (Input.GetMouseButtonDown(0)) {
            Vector3 worldPoint =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (todoList.Count != 0) {
                todoList.Dequeue()(worldPoint);
            } else {
                hit = Physics2D.OverlapCircle(worldPoint, 0.5f);
                if (hit) {
                    Debug.Log(hit.name);
                    Damageable d = hit.GetComponent<Damageable>();
                    if (d) {
                        d.Click(this);
                    }
                }
            }
        }
    }
}
}
