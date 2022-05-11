using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class World
{
    public static Node[,] grid;
    public static int[] size;
    public const int playerCare=5;
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

    public static void clearGrid(bool main){
        if (!main){
            for(int i=0;i<size[0];i++){
                for(int j=0;j<size[1];j++){
                    grid[i,j].plrDistance=10000000;
                    grid[i,j].plrNext=null;
                }
            }
        }else{
            for(int i=0;i<size[0];i++){
                for(int j=0;j<size[1];j++){
                    grid[i,j].distance=10000000;
                    grid[i,j].next=null;
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
        List<Node> ns = Neighbors(node);
        foreach(Node n in ns){
            float change=1;
            if(n.x!=node.x && n.y !=node.y){change=1.4f;}
            float newd = node.plrDistance +change;
            if(newd<=n.distance*playerCare && newd<n.plrDistance){
                n.plrDistance=newd;
                n.plrNext=node;
                LookAt.Enqueue(n);
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

    public static void CheckNode(Node node){
        List<Node> ns = Neighbors(node);
        foreach(Node n in ns){
            float change=1;
            if(n.x!=node.x && n.y!=node.y){change = 1.4f;}
            float newd = node.distance+ change;
            if(newd<n.distance){
                n.distance=newd;
                n.next = node;
                LookAt.Enqueue(n);
            }
        }
    }

    static List<Node> Neighbors(Node node){
        List<Node> r = new List<Node>();
        if(node.x>0){
            r.Add(grid[node.x-1,node.y]);
        }
        if(node.x+1<size[0]){
            r.Add(grid[node.x+1,node.y]);
        }
        if(node.y+1<size[1]){
            r.Add(grid[node.x,node.y+1]);
        }
        if(node.y>0){
            r.Add(grid[node.x,node.y-1]);
        }
        return r;
    }

    public static int[] WorldPos(Vector3 v){
        return new int[2]{(int)Mathf.Round(v.x+(size[0]/2)),(int)Mathf.Round(v.y+(size[1]/2))};
    }
}

public class Node{
    public Node next;
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

}
