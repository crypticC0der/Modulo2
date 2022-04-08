using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class World
{
    public static Node[,] grid;
    public static int size;
    public static void Generate(int size,bool[,] filled){
        LookAt = new Queue<Node>();
        World.size=size;
        grid = new Node[size,size];
        for (int i=0;i<size;i++){
            for(int j=0;j<size;j++){
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
            for(int i=0;i<size;i++){
                for(int j=0;j<size;j++){
                    grid[i,j].plrDistance=10000000;
                    grid[i,j].plrNext=null;
                }
            }
        }else{
            for(int i=0;i<size;i++){
                for(int j=0;j<size;j++){
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
            if(newd<=n.distance*2 && newd<n.plrDistance){
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
            UpdatePlrNodes(LookAt.Dequeue());
        }
    }

    public static void CheckNode(Node node){
        List<Node> ns = Neighbors(node);
        foreach(Node n in ns){
            float change=1;
            if(n.x!=node.x && n.y!=node.y){change = 1.4f;}
            float newd = node.distance+ change;
            if(newd<=n.distance){
                n.distance=newd;
                n.next = node;
                LookAt.Enqueue(n);
            }
        }
    }

    static List<Node> Neighbors(Node node){
        List<Node> r = new List<Node>();
        for(int i=-1;i<2;i++){
            if(node.x+i<size && node.x+i>=0){
                for(int j=-1;j<2;j++){
                    if(node.y+j<size && node.y+j>=0){
                        //this is bad yes i am aware but also, fuck you and die
                        r.Add(grid[node.x+i,node.y+j]);
                    }
                }
            }
        }
        return r;
    }

    public static int[] WorldPos(Vector3 v){
        return new int[2]{(int)(v.x+10),(int)(v.y+10)};
    }
}

public class Node{
    public Node next;
    public float distance;
    public Node plrNext;
    public float plrDistance;
    public int x;public int y;

    public Node GetNext(){
        if(plrDistance<distance*2){
           return plrNext;
        }else{
            return next;
        }
    }

    public float GetDist(){
        if(plrDistance<distance*2){
           return plrDistance;
        }else{
            return distance;
        }
    }

}
