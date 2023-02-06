using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MeshGen;

namespace Modulo {
public enum NodeState {
    empty = 1,
    orb=2,
    targeted = 4,
    wall = 8,
    big = 16,
    placeholder = 32,
    ground = 128,
    updating = 256
}

public class Line {
    public Node to;
    //overs may be null (this is rare so you dont need to account for
    //it propeerly
    public List<Node> over;

    public Line(Node to) {
        this.to = to;
        over = new List<Node>();
    }

    public float Cost(Node start){
        float d = HexCoord.Distance(start.hc,to.hc);
        float m = to.StateToCost();
        for(int i=0;i<over.Count;i++){
            if(over[i]!=null){
                m+=over[i].StateToCost();
            }
        }
        return d+m;
    }
}

public enum ChangeStateMethod { On, Off, Flip }

[System.Serializable]
public class Node {
    public float realDistance;
    public NodeState state {get;protected set;}
    public HexCoord hc { get; private set; }
    public Node next=null;
    bool valid=false;
    List<Line> neighbors = new List<Line>();


    public void ChangeState(NodeState ns,ChangeStateMethod csm,HexCoord at){
        bool changing = (csm == ChangeStateMethod.On && !HasState(ns))
            || (csm == ChangeStateMethod.Off && HasState(ns))
            || csm==ChangeStateMethod.Flip;

        if(changing && HasState(NodeState.big)){
            ((NodePerent)(this)).Severance();
            World.HexCoordToNode(hc).ChangeState(ns,csm,hc);
            return;
        }else if(changing){
            switch (csm) {
                case ChangeStateMethod.On:
                    state |= ns;
                    break;
                case ChangeStateMethod.Off:
                    state &= ~ns;
                    break;
                case ChangeStateMethod.Flip:
                    state ^= ns;
                    break;
                default:
                    break;
            }
            AttemptConglomeration();
        }
    }

    private void AttemptConglomeration(){
        if(World.HexCoordToNode(hc.RectCenter()).state==state){
            ((NodePerent)World.HexCoordToNode(hc.RectCenter())).Conglomerate();
        }
    }

    public bool HasState(NodeState ns) { return (state & ns) != 0; }


    delegate void UpdateOffset(int update);
    public Node(HexCoord hc, NodeState ns) {
        this.hc = hc;
        this.state = ns;
        World.nodes.Add(hc, this);
        FundementalChange();
    }

    public float Heuristic(){
        return Mathf.Pow(HexCoord.Distance(World.orbPoint.hc,hc),1);
    }

    public void SetNext(Node n){
        next=n;
        realDistance = n.realDistance + HexCoord.Distance(this.hc,n.hc);
    }

    protected void FundementalChange(){
        this.Invalidate();
        foreach(Line l in Neighbors()){
            l.to.Invalidate();
        }
    }

    public void Invalidate(){
        valid=false;
        neighbors.Clear();
    }

    public List<Line> Neighbors() {
        if(valid){
            return neighbors;
        }
        List<Line> ret = new List<Line>();
        if (HasState(NodeState.placeholder)) {
            return ret;
        }
        if (HasState(NodeState.big)) {
            // below
            HexCoord offset = new HexCoord(0, -1);
            Node Focus = World.GetNode(hc + offset);

            // this checks if a edge is a cube, and then goes through it until
            // its done, you are then left with the offset pointing to the
            // corner
            void RunEdge(UpdateOffset uo) {
                if(Focus!=null){
                    ret.Add(new Line(Focus));
                }
                if (Focus != null && Focus.HasState(NodeState.big)) {
                    uo(World.optimizationCubeSize);
                } else {
                    for (int i = 0; i < World.optimizationCubeSize - 1; i++) {
                        uo(1);
                        Focus = World.GetNode(hc + offset);
                        if(Focus!=null){
                            ret.Add(new Line(Focus));
                        }
                    }
                }
                uo(1);
            }

            // starts at the bottom left (1 after the corner), ends up at the
            // bottom right corner (unfocused)
            RunEdge((int update) => { offset.q += update; });
            // adds the corner and the previous focus as a over
            Node corn=World.GetNode(hc + offset);
            if(corn!=null){
                Line rb = new Line(corn);
                rb.over.Add(Focus);

                // adds the next focus as an over
                offset.r += 1;
                Focus = World.GetNode(hc + offset);

                rb.over.Add(Focus);
                ret.Add(rb);
            }else{
                offset.r += 1;
                Focus = World.GetNode(hc + offset);
            }
            // goes from the bottom right to the top right corner unfocused
            RunEdge((int update) => { offset.r += update; });

            // focuses the corner and adds it
            Focus = World.GetNode(hc + offset);
            if(Focus!=null){
                ret.Add(new Line(Focus));
            }

            // moves over to the first non corner at the top right
            offset.q -= 1;
            // goes from the first non corner at the top right to the left top
            // corner
            RunEdge((int update) => { offset.q -= update; });

            // adds a corner and the last focused thing as an over
            corn = World.GetNode(hc + offset);
            if(corn!=null){
                Line lt = new Line(corn);
                lt.over.Add(Focus);

                offset.r -= 1;
                Focus = World.GetNode(hc + offset);
                lt.over.Add(Focus);
            }else{
                offset.r-=1;
                Focus = World.GetNode(hc + offset);
            }
            // gets the next focus and adds it to the corners over, then moves
            // all the way to the last courner without adding it

            RunEdge((int update) => { offset.r -= update; });

            // focuses the last courner and adds it
            Focus = World.GetNode(hc + offset);
            if(Focus!=null){
                ret.Add(new Line(Focus));
            }

        } else {
            for (int i = 0; i < 3; i++) {
                Node n= World.GetNode(hc + World.Neighbors[i]);
                if(n!=null){
                    ret.Add(new Line(n));
                }
                n= World.GetNode(hc - World.Neighbors[i]);
                if(n!=null){
                    ret.Add(new Line(n));
                }
            }
            for (int i = 0; i < 3; i++) {
                HexCoord over2;
                HexCoord over1 = World.Neighbors[i];
                if (i != 3) {
                    over2 = World.Neighbors[i + 1];
                } else {
                    over2 = -World.Neighbors[0];
                }
                Node n = World.GetNode(hc + World.Neighbors[i + 3]);
                if(n!=null){
                    Line a = new Line(n);
                    a.over.Add(World.GetNode(hc - over1));
                    a.over.Add(World.GetNode(hc - over2));
                    ret.Add(a);
                }

                n = World.GetNode(hc - World.Neighbors[i + 3]);
                if(n!=null){
                    Line a = new Line(n);
                    a.over.Add(World.GetNode(hc - over1));
                    a.over.Add(World.GetNode(hc - over2));
                    ret.Add(a);
                }
            }
        }
        neighbors = ret;
        valid=true;
        return ret;
    }

    public float StateToCost() {
        float cost = 0;
        if (HasState(NodeState.wall)) {
            cost += 3;
        }
        if (HasState(NodeState.targeted)) {
            cost += 1;
        }
        if (HasState(NodeState.ground)) {
            cost += 128;
        }
        if (HasState(NodeState.orb)) {
            cost = 0;
        }
        return cost;
    }

    public override bool Equals(object obj) { return Equals(obj as Node); }

    public bool Equals(Node n) { return hc.Equals(n.hc); }

    public override int GetHashCode() {
        return (hc).GetHashCode();
    }
}

public class NodePerent : Node {
    public List<Renderer> attachedRenderers = new List<Renderer>();

    public NodePerent(HexCoord hc)
        : base(hc, NodeState.placeholder | NodeState.big) {
        if (!hc.IsRectCenter()) {
            throw(new ArgumentException(
                "placeholder nodes must be at a multiple of " +
                World.optimizationCubeSize + " attempted to be placed at " + hc.ToString()));
        }
    }

    public bool Generate() {
        if(HasState(NodeState.big)){
            state &= ~NodeState.placeholder;
        }else{
            for (int i = 0; i < World.optimizationCubeSize; i++) {
                for (int j = 0; j < World.optimizationCubeSize; j++) {
                    HexCoord pos = hc + new HexCoord(i, j);
                    Node n = World.GetNode(pos);
                    n.ChangeState(NodeState.placeholder,
                                  ChangeStateMethod.Off, pos);
                }
            }
        }

        // GameObject o = MeshGens.MinObjGen(
        //     Shapes.hexagon,MatColour.rebeccaPurple);
        // o.transform.position = hc.position();
        // ChangeState(NodeState.wall,ChangeStateMethod.On,hc);
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

    public void StopRendering () => RenderEnable(false);

    void RenderEnable(bool val) {
        foreach (Renderer r in attachedRenderers) {
            if (val != r.enabled) {
                r.enabled = val;
            }
        }
    }

    IEnumerable<HexCoord> ChildCoords(){
        for (int i = 0; i < World.optimizationCubeSize; i++) {
            for (int j = 0; j < World.optimizationCubeSize; j++) {
                yield return (hc + new HexCoord(i, j));
            }
        }
    }

    IEnumerable<Node> Children(){
        for (int i = 0; i < World.optimizationCubeSize; i++) {
            for (int j = 0; j < World.optimizationCubeSize; j++) {
                yield return World.GetNode(hc + new HexCoord(i, j));
            }
        }
    }

    public void Conglomerate() {
        if (!HasState(NodeState.big)) {
            foreach(Node child in this.Children()){
                if (child.state != state) {
                    return;
                }
            }

            state |= NodeState.big;
            foreach(HexCoord coord in this.ChildCoords()){
                if (coord!=hc) {
                    World.nodes.Remove(coord);
                }
            }
            FundementalChange();
        }
    }

    public void Severance(){
        state &= ~NodeState.big;
        foreach(HexCoord coord in this.ChildCoords()){
            if (coord!=hc) {
                new Node(coord, state);
            }
        }
        FundementalChange();
    }
}
}
