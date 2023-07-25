using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using MeshGen;
using static MeshGen.MeshGens;
using MUtils;
using System;

namespace Modulo {

public static class World {
    public static Dictionary<HexCoord, Node> nodes;
    public const int optimizationCubeSize = 4;
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

    public static Vector2 AngleToVec(float r){
        return new Vector3(Mathf.Sin(r),Mathf.Cos(r));
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

    static int inaccuracy=999;
    public static void SetOrb() {
        HexCoord orbCoord =HexCoord.NearestHex(orbTransform.position);
        // Debug.Log(orbTransform);
        // Debug.Log(orbCoord);
        if(orbPoint!=null){
            // Debug.Log(orbPoint.hc);
        }
        if (orbPoint == null || orbCoord != orbPoint.hc) {
            if (orbPoint != null) {
                // Debug.Log("a");
                orbPoint.ChangeState(NodeState.orb, ChangeStateMethod.Off,
                                     orbPoint.hc);
            }
            Node orb = HexCoordToNode(orbCoord);
            orb.ChangeState(NodeState.orb, ChangeStateMethod.On,
                                 orbCoord);
            orb = HexCoordToNode(orbCoord);
            if (orbPoint != null) {
                HexCoordToNode(orbPoint.hc).next= orb;
            }
            orbPoint = orb;
            orbPoint.realDistance = 0;
            inaccuracy++;

            if(inaccuracy>2){
                inaccuracy=0;
                ConstructMap();
            }
        }else{
            // Debug.Log("b");
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

    struct RepathNodeJob : IJob{
        public int q;
        public int r;
        public void Execute(){
            RepathNode(World.GetNode(new HexCoord(q,r)));
        }
    }

    public static void HotfixNode(Node n){
        EnsureIntegrety();
        if(debug){
            RepathNode(n);
        }else{
            RepathNodeJob rnj = new RepathNodeJob();
            rnj.q=n.hc.q;
            rnj.r=n.hc.r;
            handle = rnj.Schedule();
        }
    }


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
        foreach(EnemyFsm e in EnemyFsm.enemiesList){
            EnemyPos.Add(e.transform.position);
        }
        Vector3 orb = orbPoint.Position();
        Comparison<Vector3> comp = (Vector3 a, Vector3 b) => {
            return (int)((a - orb).magnitude -
                         (b - orb).magnitude);
        };
        EnemyPos.Sort(comp);
    }

    //pathfinding can improve by not nulling the nexts
    static List<Vector3> EnemyPos = new List<Vector3>();
    public static void RepathEnemies() {
        foreach (Vector3 pos in EnemyPos) {
            Node current = HexCoordToNode(HexCoord.NearestHex(pos));
            RepathNode(current);
        }
    }

    static void RepathNode(Node current){
        if (current.next == null) {
            List<Line> neighbors = current.Neighbors();
            bool nearbyNeighbor = false;
            foreach (Line neighbor in neighbors) {
                if (neighbor.to.next != null) {
                    bool validPath = neighbor.to.state <= current.state &&
                        neighbor.over.Count==0;
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

    public static bool debug {get;private set;} = false;
    static List<GameObject> fucked = new List<GameObject>();

    public static void EnsureIntegrety(){
        handle.Complete();
    }

    //why didnt i flip astar hmmm
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
                        o.transform.position = neighbor.to.Position();
                        fucked.Add(o);
                        NodeView nv = o.AddComponent<NodeView>();
                        nv.priority = priority;
                        nv.newCost = newCost;
                        nv.heuristic = neighbor.to.Heuristic();

                        GameObject a = MeshGens.MinObjGen(
                            Shapes.arrow, MatColour.rebeccaOrange);
                        a.transform.SetParent(o.transform);

                        Vector3 fromDist = (checking.Position() -
                                            neighbor.to.Position());
                        a.transform.position =
                            (fromDist) / 2 + neighbor.to.Position();
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

    public static bool CheckState(HexCoord hc,NodeState ns) =>
        CheckState(HexCoordToNode(hc),ns);

    public static bool CheckState(Node n,NodeState ns) =>
        n.HasState(ns);


    public static void UpdateState(HexCoord hc, NodeState ns,
                                   ChangeStateMethod mode) {
        HexCoordToNode(hc).ChangeState(ns, mode, hc);
        ConstructMap();
    }

    public static void UpdateState(HexCoord hc, NodeState ns,
                                   ChangeStateMethod mode, float range) {

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

        Component.CreateAlter(Component.Id.Pink,new HexCoord(5,0));
        Component.CreateAlter(Component.Id.Yellow,new HexCoord(-5,0));
        Cauldron.SpawnCauldron(new HexCoord(0,0));

        Item it = ItemRatio.table[0].item.FromTemplate(1, 1);
        GameObject g = it.ToGameObject(
            HexCoord.NearestHex(new Vector3(6, 6, 0)).position());
        // EnemyFsm o = MeshGens.ObjGen(Shapes.star,MatColour.white);
        // o.transform.position=new Vector3(-6,-6);
        yield return new WaitForSeconds(3.7f);

        // MeshGens.MinObjGen(Shapes.puddle,MatColour.white);
        // EnemyFsm o = null;
        // for (int i = 0; i < 7; i++) {
        //     o = EnemyGeneration.ObjGen((Shapes.circle), MatColour.white);
        //     o.transform.position = new Vector3(2 * i - 11, -5);
        // }
        Component.SpawnComponent(Component.Id.Grey, 11111,
                                      new Vector3(5,5));
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
                o.transform.position = n.Value.Position();
                CostView cv = o.AddComponent<CostView>();
                cv.cost=n.Value.StateToCost();
                cv.hc = n.Value.hc;
                cv.n=n.Value;
            }
        }
    }
}
}
