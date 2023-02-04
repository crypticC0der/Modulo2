using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshGen;
using Utils;
using System;

namespace Modulo {

public static class World {
    public static Vector2 hexVec = new Vector2(1f / 2, Mathf.Sqrt(3) / 2);
    public static Dictionary<HexCoord, Node> nodes;
    public const int optimizationCubeSize = 4;
    public static Node orbPoint;
    public static Transform orbTransform;
    public static bool stable=true;

    public static List<HexCoord> Neighbors;

    public static bool PowerOfTwo(int f){
        return (f != 0) && ((f & (f - 1)) == 0);
    }

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize() {
        nodes = new Dictionary<HexCoord,Node>();
        Neighbors = new List<HexCoord>();
        Neighbors.Add(new HexCoord(0, -1));
        Neighbors.Add(new HexCoord(1, -1));
        Neighbors.Add(new HexCoord(1, 0));

        Neighbors.Add(new HexCoord(1, -2));
        Neighbors.Add(new HexCoord(2, -1));
        Neighbors.Add(new HexCoord(1, 1));

        if(!PowerOfTwo(optimizationCubeSize)){
            throw(new ArgumentException("optimization cube size must be a multiple of 2"));
        }

        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                new NodePerent(new HexCoord(
                        i * optimizationCubeSize,
                        j * optimizationCubeSize));
            }
        }
    }

    public static void RenderAround(HexCoord hc) {
        // visability Range
        HexCoord center = hc.RectCenter();
        const int vr = 5;
        for (int i = -vr; i < vr; i++) {
            for (int j = -vr; j < vr; j++) {
                NodePerent np =
                    (NodePerent)(HexCoordToNode(center +
                                optimizationCubeSize * new HexCoord(i, j)));
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

    public static void SetOrb(HexCoord orbCoord){
        if(orbPoint ==null || orbCoord!=orbPoint.hc){
            if(orbPoint!=null){
                orbPoint.ChangeState(NodeState.orb,ChangeStateMethod.Off,orbPoint.hc);
            }
            Node orb = HexCoordToNode(orbCoord);
            orbPoint=orb;
            ConstructMap();
        }
    }

    static void ConstructMap(){
        foreach(KeyValuePair<HexCoord,Node> entry in nodes){
            entry.Value.next=null;
        }
        RepathEnemies();

    }

    public static void RepathEnemies(){
        Vector3 orb = orbPoint.hc.position();
        Comparison<EnemyFsm> comp = (EnemyFsm a,EnemyFsm b) =>{
            return (int)((a.transform.position - orb).magnitude -
                    (b.transform.position - orb).magnitude);
        };
        EnemyFsm.enemiesList.Sort(comp);
        foreach(EnemyFsm enemy in EnemyFsm.enemiesList){
            Node current = HexCoordToNode(
                HexCoord.NearestHex(enemy.transform.position));
            if(current.next==null){
                List<Line> neighbors = current.Neighbors();
                bool nearbyNeighbor=false;
                foreach(Line neighbor in neighbors){
                    if(neighbor.to.next!=null){
                        bool validPath = neighbor.to.state <= current.state;
                        foreach(Node n in neighbor.over){
                            validPath &= neighbor.to.state <= current.state;
                        }
                        if(validPath){
                            nearbyNeighbor=true;
                            current.SetNext(neighbor.to);
                        }
                    }
                }
                if(!nearbyNeighbor){
                    AStar(current);
                }
            }
        }
    }

    public static void AStar(Node start){
        PriorityQueue<Node,float> toCheck = new PriorityQueue<Node,float>();
        toCheck.Enqueue(start,0);

        Dictionary<Node,float> costs = new Dictionary<Node,float>();
        Dictionary<Node,Node> cameFrom = new Dictionary<Node,Node>();

        Node goal=null;
        while(toCheck.Count>0){
            Node checking = toCheck.Dequeue();
            //if neccicary add the ability to accidently run into anothers pass
            if(checking.HasState(NodeState.orb)){
                //win
                checking.realDistance=0;
                checking.next=checking;
                goal = checking;
                break;
            }
            //evaluate neghbors
            List<Line> neighbors = checking.Neighbors();
            foreach(Line neighbor in neighbors){
                float newCost = costs[checking] + neighbor.Cost(checking);
                if (neighbor.to.next == null || newCost < costs[neighbor.to]){
                    costs[neighbor.to] = newCost;
                    float priority = newCost + neighbor.to.Heuristic();
                    toCheck.Enqueue(neighbor.to,priority);
                    cameFrom.Add(neighbor.to,checking);
                }
            }
        }
        //reconstruction
        if(goal!=null){
            Node backChecking=goal;
            while(cameFrom.ContainsKey(backChecking)){
                cameFrom[backChecking].SetNext(backChecking);
                backChecking = cameFrom[backChecking];
            }
        }
    }

    public static void UpdateState(HexCoord hc,NodeState ns
                                   ,ChangeStateMethod mode){
        HexCoordToNode(hc).ChangeState(ns,mode,hc);
    }

    public static void UpdateState(HexCoord hc,NodeState ns,
                                   ChangeStateMethod mode,float range){
        hc.ForEachInRange((int)Mathf.Ceil(range),
                       (HexCoord cord) => UpdateState(cord,ns,mode));
    }

    public static IEnumerator MapGen(){
    	// while(true){
    	// 	yield return new WaitForSeconds(1);
    	// 	Effects.Explode(new Vector3(0,5),1,null);
    	// 	Effects.Explode(new Vector3(0,-5),2,null);
    	// }
    	Item it = ItemRatio.table[0].item.FromTemplate(1,1);
    	GameObject g= it.ToGameObject(HexCoord.NearestHex(new Vector3(6,6,0)).position());
    	// EnemyFsm o = MeshGens.ObjGen(Shapes.star,MatColour.white);
    	// o.transform.position=new Vector3(-6,-6);
        yield return new WaitForSeconds(0.7f);
    	// MeshGens.MinObjGen(Shapes.puddle,MatColour.white);
    	EnemyFsm o=null;
    	for(int i=0;i<7;i++){
    		o = EnemyGeneration.ObjGen((Shapes.circle),MatColour.white);
    		o.transform.position = new Vector3(2*i-11,-5);
    	}
    	// g.GetComponent<Damageable>().Draw(f);
    }

}

public static class WorldDebugger{
    public static GameObject gridObj;
    public static void GenGrid(){
        gridObj = new GameObject();
    }
}
}
