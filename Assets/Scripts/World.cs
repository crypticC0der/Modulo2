using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.Jobs;
using MeshGen;
using System;

namespace Modulo {

public class HexCoord {
    public int q, r;
    public static Vector2 Q = new Vector2(Mathf.Sin(90), Mathf.Cos(90));
    public static Vector2 R = new Vector2(Mathf.Sin(150), Mathf.Cos(150));
    const int mask = (~(8 - 1));
    public HexCoord(int q, int r) {
        this.q = q;
        this.r = r;
    }

    public Vector3 position() { return q * Q + r * R; }

    public bool IsRectCenter() {
        return (q & (~mask)) == 0 && (r & (~mask)) == 0;
    }

    public HexCoord RectCenter() { return new HexCoord(q & mask, q & mask); }

    public override bool Equals(object obj) { return Equals(obj as HexCoord); }

    public bool Equals(HexCoord hc) { return hc.q == q && hc.r == r; }

    public override int GetHashCode() {
        return (new Vector2(q, r)).GetHashCode();
    }

    public static HexCoord NearestHex(Vector3 v) {
        int x, y;
        y = (int)Mathf.Round(v.y / World.hexVec.y);
        if (v.y % 2 != 0) {
            x = (int)(Mathf.Round(v.x - World.hexVec.x) + World.hexVec.x);
        } else {
            x = (int)Mathf.Round(v.x);
        }
        y *= (int)World.hexVec.y;
        x = x - (y + (y & 1)) / 2;
        return new HexCoord(x, y);
    }

    public static HexCoord operator +(HexCoord a) => a;
    public static HexCoord
    operator +(HexCoord a, HexCoord b) => new HexCoord(a.q + b.q, a.r + b.r);
    public static HexCoord operator -(HexCoord a) => new HexCoord(-a.q, -a.r);
    public static HexCoord operator -(HexCoord a, HexCoord b) => a + (-b);
    public static HexCoord operator *(HexCoord a,
                                      int f) => new HexCoord(f * a.q, f *a.r);
}

public static class World {
    public static Vector2 hexVec = new Vector2(1f / 2, Mathf.Sqrt(3) / 2);
    public static Dictionary<HexCoord, Node> nodes;
    public const int optimizationCubeSize = 4;

    public static List<HexCoord> Neighbors;

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize() {
        Neighbors = new List<HexCoord>();
        Neighbors.Add(new HexCoord(0, -1));
        Neighbors.Add(new HexCoord(1, -1));
        Neighbors.Add(new HexCoord(1, 0));

        Neighbors.Add(new HexCoord(1, -2));
        Neighbors.Add(new HexCoord(2, -1));
        Neighbors.Add(new HexCoord(1, 1));

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                new NodePerent(new HexCoord(i * 5 - 5, j * 5 - 5));
            }
        }
    }

    public static void RenderAround(Vector3 v) {
        // visability Range
        const int vr = 5;
        HexCoord hc = HexCoord.NearestHex(v).RectCenter();
        for (int i = -vr; i < vr; i++) {
            for (int j = -vr; j < vr; j++) {
                NodePerent np =
                    (NodePerent)(HexCoordToNode(hc + new HexCoord(i, j)));
                np.Render();
            }
        }
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

    // // public static IEnumerator MapGen(){
    // public static void MapGen(){
    // 	// while(true){
    // 	// 	yield return new WaitForSeconds(1);
    // 	// 	Effects.Explode(new Vector3(0,5),1,null);
    // 	// 	Effects.Explode(new Vector3(0,-5),2,null);
    // 	// }

    // 	Item it = ItemRatio.table[0].item.FromTemplate(1,1);
    // 	GameObject g= it.ToGameObject(NearestHex(new Vector3(6,6,0)));
    // 	// EnemyFsm o = MeshGens.ObjGen(Shapes.star,MatColour.white);
    // 	// o.transform.position=new Vector3(-6,-6);
    // 	EnemyFsm o=null;
    // 	MeshGens.MinObjGen(Shapes.puddle,MatColour.white);
    // 	for(int i=0;i<7;i++){
    // 		// o = EnemyGeneration.ObjGen((Shapes.circle),MatColour.white);
    // 		// o.transform.position = new Vector3(2*i-11,-5);
    // 	}
    // 	// g.GetComponent<Damageable>().Draw(f);
    // }

    public struct StateData {
        public int x;
        public int y;
        public NodeState s;
    }

    public enum ChangeStateMethod { On, Off, Flip }
}

public enum NodeState {
    empty = 1,
    wall = 4,
    targeted = 8,
    big = 16,
    placeholder = 32,
    ground = 128,
    updating = 256
}

public class Line {
    public Node to;
    public List<Node> over;

    public Line(Node to) {
        this.to = to;
        over = new List<Node>();
    }
}

[System.Serializable]
public class Node {
    float distance;
    float realDistance;
    public NodeState state;
    public HexCoord hc { get; private set; }

    public bool HasState(NodeState ns) { return (state & ns) != 0; }

    delegate void UpdateOffset(int update);
    public Node(HexCoord hc, NodeState ns) {
        this.hc = hc;
        this.state = ns;
        World.nodes.Add(hc, this);
    }

    public HashSet<Line> Neighbors() {
        HashSet<Line> ret = new HashSet<Line>();
        if (HasState(NodeState.placeholder)) {
            return ret;
        }
        if (HasState(NodeState.big)) {
            // below
            HexCoord offset = new HexCoord(0, -1);
            Node Focus = World.HexCoordToNode(hc + offset);

            // this checks if a edge is a cube, and then goes through it until
            // its done, you are then left with the offset pointing to the
            // corner
            void RunEdge(UpdateOffset uo) {
                ret.Add(new Line(Focus));
                if (Focus.HasState(NodeState.big)) {
                    uo(World.optimizationCubeSize);
                } else {
                    for (int i = 0; i < World.optimizationCubeSize - 1; i++) {
                        uo(1);
                        Focus = World.HexCoordToNode(hc + offset);
                        ret.Add(new Line(Focus));
                    }
                }
                uo(1);
            }

            // starts at the bottom left (1 after the corner), ends up at the
            // bottom right corner (unfocused)
            RunEdge((int update) => { offset.q += update; });
            // adds the corner and the previous focus as a over
            Line rb = new Line(World.HexCoordToNode(hc + offset));
            rb.over.Add(Focus);

            // adds the next focus as an over
            offset.r += 1;
            Focus = World.HexCoordToNode(hc + offset);

            rb.over.Add(Focus);
            // goes from the bottom right to the top right corner unfocused
            RunEdge((int update) => { offset.r += update; });

            // focuses the corner and adds it
            Focus = World.HexCoordToNode(hc + offset);
            ret.Add(new Line(Focus));

            // moves over to the first non corner at the top right
            offset.q -= 1;
            // goes from the first non corner at the top right to the left top
            // corner
            RunEdge((int update) => { offset.q -= update; });

            // adds a corner and the last focused thing as an over
            Line lt = new Line(World.HexCoordToNode(hc + offset));
            rb.over.Add(Focus);

            offset.r -= 1;
            Focus = World.HexCoordToNode(hc + offset);
            rb.over.Add(Focus);
            // gets the next focus and adds it to the corners over, then moves
            // all the way to the last courner without adding it

            RunEdge((int update) => { offset.r -= update; });

            // focuses the last courner and adds it
            Focus = World.HexCoordToNode(hc + offset);
            ret.Add(new Line(Focus));
            return ret;
        } else {
            for (int i = 0; i < 3; i++) {
                Line a =
                    new Line(World.HexCoordToNode(hc + World.Neighbors[i]));
                Line b =
                    new Line(World.HexCoordToNode(hc - World.Neighbors[i]));
                ret.Add(a);
                ret.Add(b);
            }
            for (int i = 0; i < 3; i++) {
                HexCoord over2;
                HexCoord over1 = World.Neighbors[i];
                if (i != 3) {
                    over2 = World.Neighbors[i + 1];
                } else {
                    over2 = -World.Neighbors[0];
                }
                Line a =
                    new Line(World.HexCoordToNode(hc + World.Neighbors[i + 3]));
                a.over.Add(World.HexCoordToNode(hc - over1));
                a.over.Add(World.HexCoordToNode(hc - over2));

                Line b =
                    new Line(World.HexCoordToNode(hc - World.Neighbors[i + 3]));
                b.over.Add(World.HexCoordToNode(hc - over1));
                b.over.Add(World.HexCoordToNode(hc - over2));

                ret.Add(a);
                ret.Add(b);
            }
            return ret;
        }
    }

    public float StateToMultiplier() {
        float multiplier = 1;
        if (HasState(NodeState.wall)) {
            multiplier *= 6;
        }
        if (HasState(NodeState.targeted)) {
            multiplier *= 8;
        }
        if (HasState(NodeState.ground)) {
            multiplier *= 128;
        }
        if (HasState(NodeState.big)) {
            multiplier *= 4;
        }
        return multiplier;
    }
}

public class NodePerent : Node {
    public List<Renderer> attachedRenderers;
    public bool rectUpdated = false;

    public NodePerent(HexCoord hc)
        : base(hc, NodeState.placeholder | NodeState.big) {
        if (!hc.IsRectCenter()) {
            throw(new ArgumentException(
                "placeholder nodes must be at a multiple of " +
                World.optimizationCubeSize));
        }
    }

    public bool Generate() {
        state &= ~NodeState.placeholder;
        return true;
    }
    public void Render() {
        if (HasState(NodeState.placeholder)) {
            if (!Generate()) {
                return;
            }
        }
        RenderEnable(true);
    }

    public void StopRendering() => RenderEnable(false);

    void RenderEnable(bool val) {
        foreach (Renderer r in attachedRenderers) {
            if (val != r.enabled) {
                r.enabled = val;
            }
        }
    }

    public void Conglomerate() {
        if (rectUpdated && !HasState(NodeState.big)) {
            for (int i = 0; i < World.optimizationCubeSize; i++) {
                for (int j = 0; j < World.optimizationCubeSize; j++) {
                    if (World.HexCoordToNode(hc + new HexCoord(i, j)).state !=
                        state) {
                        return;
                    }
                }
            }

            state |= NodeState.big;
            for (int i = 0; i < World.optimizationCubeSize; i++) {
                for (int j = 0; j < World.optimizationCubeSize; j++) {
                    if (j != 0 || i != 0) {
                        World.nodes.Remove(hc + new HexCoord(i, j));
                    }
                }
            }
        }
    }
}
}
