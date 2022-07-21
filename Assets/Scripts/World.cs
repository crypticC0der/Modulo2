using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class World
{
    public static GameObject gridObj=null;
    public static Node[,] grid;
    public static int[] size;
    public const int playerCare=2;
    public const int largeDist = 2000000000;
    public static void Generate(int[] size,bool[,] filled){
        LookAt = new Queue<Node>();
        World.size=size;
        grid = new Node[size[0],size[1]];
        for (int i=0;i<size[0];i++){
            for(int j=0;j<size[1];j++){
                grid[i,j] = new Node();
                grid[i,j].next= null;
                grid[i,j].x=i;
                grid[i,j].y=j;
                if(filled[i,j]){
                    grid[i,j].distance=-1;
                }else{
                    grid[i,j].distance=999999;
                }
            }
        }
    }
    public static Vector2 hexVec = new Vector2(1f/2,Mathf.Sqrt(3)/2);

    public static void GenGrid(){
        gridObj=new GameObject("grid");
        gridObj.transform.position=new Vector3(0,0,100);
        for (int i=0;i<size[0];i++){
            for(int j=0;j<size[1];j++){
                grid[i,j].RenderNode();
            }
        }
    }


    [RuntimeInitializeOnLoadMethod]
    public static void Initialize(){
        World.Generate(new int[]{72,44},new bool[72,44]);
        ItemRatio.table[5].item.FromTemplate(1,1).ToGameObject(NearestHex(new Vector3(6,6,0)));
        // EnemyFsm o = MeshGens.ObjGen(Shapes.star,MatColour.white);
        // o.transform.position=new Vector3(-6,-6);
        for(int i=0;i<3;i++){
            EnemyFsm o = MeshGens.ObjGen((Shapes.circle),MatColour.white);
            o.transform.position = new Vector3(2*i-11,-5);
        }
    }

    public static void Print(){
        Debug.Log("saving");
        string r = "";
        for(int i =0;i<size[1];i++){
            for(int j=0;j<size[0];j++){
                // if(grid[j,i].distance*playerCare>grid[j,i].plrDistance){
                //     r+="p";
                // }
                r+=string.Format("{0:00.0}, ",grid[j,i].GetDist());
            }
            r+="\n";
        }
        File.WriteAllText(@"/home/mj/grid.txt",r);
    }


    public static void clearGrid(bool main){
        if (!main){
            for(int i=0;i<size[0];i++){
                for(int j=0;j<size[1];j++){
                    grid[i,j].plrDistance=largeDist;
                    grid[i,j].plrNext=grid[i,j]; //avoids errors
                }
            }
        }else{
            for(int i=0;i<size[0];i++){
                for(int j=0;j<size[1];j++){
                    grid[i,j].distance=largeDist;
                    grid[i,j].next=grid[i,j];
                }
            }
        }
    }

    public static void MovePlayer(int x,int y){
        clearGrid(false);
        grid[x,y].plrDistance=0;
        grid[x,y].plrNext=grid[x,y];
        LookAt.Enqueue(grid[x,y]);
        while(LookAt.Count>0){
            UpdatePlrNodes(LookAt.Dequeue());
        }
    }

    static Queue<Node> LookAt;
    public static void UpdatePlrNodes(Node node){
        List<NodeData> ns = Neighbors(node);
        foreach(NodeData data in ns){
            float addedDist = 1;
            if (data.over!=null){
                addedDist=1.5f;
            }
            float newd= node.plrDistance +addedDist;
            if(newd<=data.n.distance*playerCare && newd<data.n.plrDistance && newd<100){
                data.n.plrDistance=newd;
                data.n.plrNext=node;
                LookAt.Enqueue(data.n);
            }
        }
    }

    public static void AddConstruct(int x,int y){
        grid[x,y].distance=0;
        grid[x,y].next=grid[x,y];
        LookAt.Enqueue(grid[x,y]);
        while(LookAt.Count>0){
            CheckNode(LookAt.Dequeue());
        }
    }

    static List<Node> BorderNode=new List<Node>();
    public static void RemoveConstruct(int x,int y){
        if(grid[x,y].next!=grid[x,y]){return;}
        LookAt.Enqueue(grid[x,y]);
        //forward check
        while(LookAt.Count>0){
            BackCheck(LookAt.Dequeue());
        }
        //border validation
        foreach(Node b in BorderNode){
            if(b.distance!=largeDist && b.next.distance!=largeDist){
                LookAt.Enqueue(b);
            }
        }
        //backwards write
        BorderNode.Clear();
        while(LookAt.Count>0){
            CheckNode(LookAt.Dequeue());
        }
    }

    public static void BackCheck(Node node){
        List<NodeData> ns = Neighbors(node);
        if(node.distance==largeDist){return;}
        node.distance=largeDist;
        foreach(NodeData data in ns){
            bool invalidPath=false;
            if(data.n.over!=null){
                invalidPath |= (data.n.over[0].distance==largeDist);
                invalidPath |= (data.n.over[1].distance==largeDist);
            }
            invalidPath |= (data.n.next.distance==largeDist);
            if(invalidPath){
                LookAt.Enqueue(data.n);
            }else{
                BorderNode.Add(data.n);
            }
        }
    }

    public static void CheckNode(Node node){
        List<NodeData> ns = Neighbors(node);
        foreach(NodeData data in ns){
            float addedDist = 1;
            if (data.over!=null){
                addedDist=1.5f;
            }
            if(node.distance+addedDist<data.n.distance){
                data.n.distance=node.distance+addedDist;
                data.n.next = node;
                data.n.over=data.over;
                LookAt.Enqueue(data.n);
            }
        }
    }

    //data for neighbors
    struct NodeData{
        public Node n;
        public Node[] over;
    }
    public struct Direction{
        public int x;
        public int y;
    }
    static Direction[] directions = new Direction[]{
        new Direction{x=0,y=1},
        new Direction{x=1,y=0},
        new Direction{x=0,y=-1},
        new Direction{x=-1,y=-1},
        new Direction{x=-1,y=0},
        new Direction{x=-1,y=1}
    };
    static Direction[] longDirections = new Direction[]{
        new Direction{x=0,y=2},
        new Direction{x=1,y=1},
        new Direction{x=1,y=-1},
        new Direction{x=0,y=-2},
        new Direction{x=-2,y=1},
        new Direction{x=-2,y=1}
    };

    static List<NodeData> Neighbors(Node node){
        List<NodeData> r = new List<NodeData>();

        int offset = ((node.y+1)%2);
        //check initial direction
        Direction nd = directions[0];
        if(nd.y!=0){nd.x+=offset;}
        bool initial = node.Valid(nd);
        Node initialNode=null;
        if(initial){
            initialNode = (grid[node.x+nd.x,node.y+nd.y]);
            r.Add(new NodeData{n=initialNode});
        }
        bool correct = initial;
        Node previousNode = initialNode;
        //check all non initial directions
        for(int i=1;i<6;i++){
            nd = directions[i];
            //manage offsets
            if(nd.y!=0){nd.x+=offset;}
            bool valid = node.Valid(nd);
            Node currentNode=null;
            if(valid){
                //add to list
                currentNode = (grid[node.x+nd.x,node.y+nd.y]);
                r.Add(new NodeData{n=currentNode});
                //check long direction
                Direction md = longDirections[i];
                if(md.y%2!=0){
                    md.x+=offset;
                }
                //if all is well add to the list
                if(correct&&node.Valid(md)){
                    r.Add(new NodeData{n=grid[node.x+md.x,node.y+md.y],over=new Node[]{previousNode,currentNode}});
                }
            }
            correct=valid;
            previousNode=currentNode;
        }
        //check final long direction
        if(correct && initial && node.Valid(longDirections[0])){
            Node longNode = (grid[node.x+longDirections[0].x,node.y+longDirections[0].y]);
            r.Add(new NodeData{n=longNode,over=new Node[]{previousNode,initialNode}});
        }
        return r;


        // hex version 1
        // //horiz
        // if(node.x>0){r.Add(grid[node.x-1,node.y]);}
        // if(node.x+1<size[0]){r.Add(grid[node.x+1,node.y]);}

        // //down
        // if(node.y>0){
        //     r.Add(grid[node.x,node.y-1]);
        //     if(node.y%2==0 && node.x+1<size[0]){
        //         //if not shifted
        //         r.Add(grid[node.x+1,node.y-1]);
        //     }else if(node.y%2==1&& node.x>0){
        //         //if shifted
        //         r.Add(grid[node.x-1,node.y-1]);
        //     }
        // }
        // //up
        // if(node.y+1<size[1]){
        //     r.Add(grid[node.x,node.y+1]);
        //     if(node.y%2==0 && node.x+1<size[0]){
        //         //if not shifted
        //         r.Add(grid[node.x+1,node.y+1]);
        //     }else if(node.y%2==1&& node.x>0){
        //         //if shifted
        //         r.Add(grid[node.x-1,node.y+1]);
        //     }
        // }
    }

    public static int[] WorldPos(Vector3 v){
        // (int)Mathf.Round(v.x+(size[0]/2))
        int[] p = new int[2]{0,(int)Mathf.Round(v.y/hexVec.y+(size[1]/2))};
        if(p[1]%2==1){v.x-=hexVec.x;}
        p[0]=(int)(Mathf.Round(v.x+size[0]/2));
        return p;
    }

    public static Vector3 NearestHex(Vector3 v){
        v.y=Mathf.Round(v.y/hexVec.y);
        if(v.y%2==1){
            v.x=Mathf.Round(v.x-hexVec.x)+hexVec.x;
        }else{
            v.x=Mathf.Round(v.x);
        }
        v.y*=hexVec.y;
        return v;
    }
}

public class Node{
    public Node next;
    public Node[] over=null;
    public float distance;
    public Node plrNext;
    public float plrDistance;
    public int x;public int y;

    public Node GetNext(){
        if(plrDistance<distance*World.playerCare){
           return plrNext;
        }else{
            return next;
        }
    }

    public GameObject RenderNode(){
        GameObject o =MeshGens.MinObjGen(Shapes.hexagonOuter,MatColour.black);
        Vector3 p =WorldPos();
        o.transform.SetParent(World.gridObj.transform);
        o.transform.localPosition=p;
        return o;
    }

    public string ToString(){
        return x.ToString() + " , " + y.ToString();
    }

    public float GetDist(){
        if(plrDistance<distance*World.playerCare){
           return plrDistance;
        }else{
            return distance;
        }
    }

    public float GetDistReal(){
        if(plrDistance<distance){
           return plrDistance;
        }else{
            return distance;
        }
    }

    public Vector3 WorldPos(){
        Vector3 v = new Vector3(x,y);
        if(y%2==1){
            v.x+=World.hexVec.x;
        }
        v -= new Vector3(World.size[0]/2,World.size[1]/2);
        v.y*=World.hexVec.y;
        return v;
    }

    public Vector3 Direction(){
        if(next.y-y==0){
            return new Vector3(next.x-x,0);
        }else{
            int dx=next.x-x;
            Vector3 r = new Vector3(0,next.y-y*1/2);
            r.x=Mathf.Sqrt(3);
            if(dx==0 ^ y%2==1){
                r.x=-Mathf.Sqrt(3);
            }
            return r;
        }
    }

    public bool Valid(World.Direction d){
        d.y+=y;
        d.x+=x;
        return (d.x>=0 && d.y>=0 && d.y<World.size[1] && d.x<World.size[0]);
    }
}
