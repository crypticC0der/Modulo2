using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using MeshGen;
using static MeshGen.MeshGens;
using Utils;
using System;

namespace Modulo {

public static class World {
    public static float hexSize = 1 / Mathf.Sqrt(3);
    public static Vector2 hexVec = new Vector2(1f / 2, Mathf.Sqrt(3) / 2);
    public static Dictionary<HexCoord, Node> nodes;
    public const int optimizationCubeSize = 8;
    public static Node orbPoint;
    public static Transform orbTransform;
    public static bool stable = true;
    public static List<HexCoord> Neighbors;
    static JobHandle handle;

    public static bool PowerOfTwo(int f) {
        return (f != 0) && ((f & (f - 1)) == 0);
    }

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize() {
        nodes = new Dictionary<HexCoord, Node>();
        Neighbors = new List<HexCoord>();
        Neighbors.Add(new HexCoord(0, +1));
        Neighbors.Add(new HexCoord(1, +1));
        Neighbors.Add(new HexCoord(1, 0));

        Neighbors.Add(new HexCoord(1, +2));
        Neighbors.Add(new HexCoord(2, +1));
        Neighbors.Add(new HexCoord(1, 1));

        if (!PowerOfTwo(optimizationCubeSize)) {
            throw(new ArgumentException(
                "optimization cube size must be a multiple of 2"));
        }

        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                new NodePerent(new HexCoord(i * optimizationCubeSize,
                                            j * optimizationCubeSize));
            }
        }

        for (int i = -100; i <= 100; i++) {
            for (int j = -100; j <= 100; j++) {
                HexCoord hc = new HexCoord(i, j);
                if (hc != HexCoord.NearestHex(hc.position())) {
                    throw(new ArgumentException(
                        hc.ToString() + " -> " + hc.position().ToString() +
                        " -> " +
                        HexCoord.NearestHex(hc.position()).ToString()));
                }
            }
        }
    }

    public static void RenderAround(HexCoord hc) {
        // visability Range
        HexCoord center = hc.RectCenter();
        const int vr = 5;
        for (int i = -vr; i <= vr; i++) {
            for (int j = -vr; j <= vr; j++) {
                HexCoord c =
                    center + (optimizationCubeSize * new HexCoord(i, j));
                NodePerent np = (NodePerent)(HexCoordToNode(c));
                np.Render();
            }
        }
    }

    public static Node GetNode(HexCoord hc) {
        if (nodes.ContainsKey(hc)) {
            return nodes[hc];
        } else if (nodes.ContainsKey(hc.RectCenter())) {
            return nodes[hc.RectCenter()];
        }
        return null;
    }

    public static Node HexCoordToNode(HexCoord hc) {
        if (nodes.ContainsKey(hc)) {
            return nodes[hc];
        } else if (nodes.ContainsKey(hc.RectCenter())) {
            return nodes[hc.RectCenter()];
        } else {
            return new NodePerent(hc.RectCenter());
        }
    }

    public static float VecToAngle(Vector3 v) {
        float angle = 90;
        if (v.y != 0) {
            angle = -180 * Mathf.Atan(v.x / v.y) / Mathf.PI;
            if (v.y < 0) {
                angle += 180;
            }
        } else if (v.x > 0) {
            angle = -90;
        } else if (v.x == 0) {
            angle = 0;
        }
        if (angle < 0) {
            angle += 360;
        }
        return angle;
    }

    public static void SetOrb(HexCoord orbCoord) {
        if (orbPoint == null || orbCoord != orbPoint.hc) {
            if (orbPoint != null) {
                orbPoint.ChangeState(NodeState.orb, ChangeStateMethod.Off,
                                     orbPoint.hc);
            }
            Node orb = HexCoordToNode(orbCoord);
            orbPoint = orb;
            orbPoint.realDistance = 0;
            orbPoint.ChangeState(NodeState.orb, ChangeStateMethod.On,
                                 orbPoint.hc);
            ConstructMap();
        }
    }

    static int constructCalls = 0;
    static void ConstructMap() { constructCalls++; }

    static void MapFix() {
        RepathPrerecs();

        if(debug){
            RepathEnemies();
        }else{
            RepathJob rj = new RepathJob();
            handle = rj.Schedule();
        }
    }

    struct RepathJob : IJob{
        public void Execute(){
            RepathEnemies();
        }
    };

    static int astrCalls = 0;
    static int earlyExits = 0;
    public static void Run() {
        if (constructCalls > 0) {
            astrCalls = 0;
            earlyExits = 0;
            MapFix();
            if(debug){
                Debug.Log(astrCalls);
                Debug.Log(earlyExits);
            }
        }
        constructCalls = 0;
    }

    public static void DebugToggle(){
        WorldDebugger.ToggleGrid();
        EnsureIntegrety();
        debug=!debug;
    }

    public static void RepathPrerecs(){

        foreach (KeyValuePair<HexCoord, Node> entry in nodes) {
            entry.Value.next = null;
        }

        foreach (GameObject x in fucked) {
            GameObject.Destroy(x);
        }
        fucked.Clear();

        EnsureIntegrety();

        EnemyPos.Clear();
        Vector3 orb = orbPoint.hc.position();
        Comparison<EnemyFsm> comp = (EnemyFsm a, EnemyFsm b) => {
            return (int)((a.transform.position - orb).magnitude -
                         (b.transform.position - orb).magnitude);
        };
        EnemyFsm.enemiesList.Sort(comp);
        foreach(EnemyFsm e in EnemyFsm.enemiesList){
            EnemyPos.Add(e.transform.position);
        }

    }

    static List<Vector3> EnemyPos = new List<Vector3>();
    public static void RepathEnemies() {
        foreach (Vector3 pos in EnemyPos) {
            Node current =
                HexCoordToNode(HexCoord.NearestHex(pos));
            if (current.next == null) {
                List<Line> neighbors = current.Neighbors();
                bool nearbyNeighbor = false;
                foreach (Line neighbor in neighbors) {
                    if (neighbor.to.next != null) {
                        bool validPath = neighbor.to.state <= current.state;
                        foreach (Node n in neighbor.over) {
                            validPath &= neighbor.to.state <= current.state;
                        }
                        if (validPath) {
                            nearbyNeighbor = true;
                            current.SetNext(neighbor.to);
                        }
                    }
                }
                if (!nearbyNeighbor) {
                    AStar(current);
                }
            }
        }
    }

    static bool debug = false;
    static List<GameObject> fucked = new List<GameObject>();

    static void EnsureIntegrety(){
        handle.Complete();
    }

    public static void AStar(Node start) {
        astrCalls++;
        PriorityQueue<Node, float> toCheck = new PriorityQueue<Node, float>();
        toCheck.Enqueue(start, 0);

        Dictionary<Node, float> costs = new Dictionary<Node, float>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        costs[start] = 0;

        Node goal = null;
        while (toCheck.Count > 0) {
            Node checking = toCheck.Dequeue();
            // if neccicary add the ability to accidently run into anothers pass
            if (checking.HasState(NodeState.orb) || checking.next != null) {
                // win
                if (!checking.HasState(NodeState.orb)) {
                    earlyExits++;
                }
                goal = checking;
                break;
            }
            // evaluate neghbors
            List<Line> neighbors = checking.Neighbors();
            foreach (Line neighbor in neighbors) {
                float newCost = costs[checking] + neighbor.Cost(checking);
                if (!costs.ContainsKey(neighbor.to) ||
                    newCost < costs[neighbor.to]) {
                    costs[neighbor.to] = newCost;
                    float priority = newCost + neighbor.to.Heuristic();

                    if (debug) {
                        GameObject o = MeshGens.MinObjGen(
                            Shapes.hexagonOuter, MatColour.rebeccaOrangeAnti);
                        o.transform.position = neighbor.to.hc.position();
                        fucked.Add(o);
                        NodeView nv = o.AddComponent<NodeView>();
                        nv.priority = priority;
                        nv.newCost = newCost;
                        nv.heuristic = neighbor.to.Heuristic();

                        GameObject a = MeshGens.MinObjGen(
                            Shapes.arrow, MatColour.rebeccaOrange);
                        a.transform.SetParent(o.transform);

                        Vector3 fromDist = (checking.hc.position() -
                                            neighbor.to.hc.position());
                        a.transform.position =
                            (fromDist) / 2 + neighbor.to.hc.position();
                        a.transform.eulerAngles =
                            new Vector3(0, 0, World.VecToAngle(-fromDist));
                        a.transform.localScale =
                            new Vector3(1, 1.2f * fromDist.magnitude, 1);
                    }

                    toCheck.Enqueue(neighbor.to, priority);
                    cameFrom[neighbor.to] = checking;
                }
            }
        }
        // reconstruction
        if (goal != null) {
            Node backChecking = goal;
            while (cameFrom.ContainsKey(backChecking)) {
                cameFrom[backChecking].SetNext(backChecking);
                backChecking = cameFrom[backChecking];
            }
            return;
        }
    }

    public static void UpdateState(HexCoord hc, NodeState ns,
                                   ChangeStateMethod mode) {
        EnsureIntegrety();
        HexCoordToNode(hc).ChangeState(ns, mode, hc);
        ConstructMap();
    }

    public static void UpdateState(HexCoord hc, NodeState ns,
                                   ChangeStateMethod mode, float range) {
        EnsureIntegrety();
        Debug.Log(ns);
        foreach (HexCoord cord in hc.InRange((int)Mathf.Ceil(range))) {
            HexCoordToNode(cord).ChangeState(ns, mode, cord);
        }
        ConstructMap();
    }

    public static IEnumerator MapGen() {
        // while(true){
        // 	yield return new WaitForSeconds(1);
        // 	Effects.Explode(new Vector3(0,5),1,null);
        // 	Effects.Explode(new Vector3(0,-5),2,null);
        // }
        Item it = ItemRatio.table[0].item.FromTemplate(1, 1);
        GameObject g = it.ToGameObject(
            HexCoord.NearestHex(new Vector3(6, 6, 0)).position());
        // EnemyFsm o = MeshGens.ObjGen(Shapes.star,MatColour.white);
        // o.transform.position=new Vector3(-6,-6);
        yield return new WaitForSeconds(3.7f);
        // MeshGens.MinObjGen(Shapes.puddle,MatColour.white);
        EnemyFsm o = null;
        for (int i = 0; i < 7; i++) {
            o = EnemyGeneration.ObjGen((Shapes.circle), MatColour.white);
            o.transform.position = new Vector3(2 * i - 11, -5);
        }
        // g.GetComponent<Damageable>().Draw(f);
    }
}

public static class WorldDebugger {
    static GameObject gridObj;
    public static void ToggleGrid() {
        if(gridObj){
            GameObject.Destroy(gridObj);
        }else{
            gridObj = new GameObject();
            foreach(KeyValuePair<HexCoord,Node> n in World.nodes){
                Color c = Color.Lerp(Color.green,Color.red,
                                    (n.Value.StateToCost()));
                Material m = ColorToMat(c);
                GameObject o = MinObjGen(Shapes.hexagonOuter, m);
                o.transform.SetParent(gridObj.transform);
                o.transform.position = n.Value.hc.position();
                CostView cv = o.AddComponent<CostView>();
                cv.cost=n.Value.StateToCost();
                cv.hc = n.Value.hc;
                cv.c=c;
            }
        }
    }
}
}
