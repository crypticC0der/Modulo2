using UnityEngine;

namespace Modulo {
public class NodeView : MonoBehaviour {
    public float priority;
    public float newCost;
    public float heuristic;

}

public class CostView : MonoBehaviour {
    public float cost;
    public HexCoord hc;
    public Node n;
}

}
