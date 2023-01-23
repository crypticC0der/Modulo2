using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.Jobs;
using MeshGen;

namespace Modulo{
	public static class World
	{
		static List<JobHandle> jobs = new List<JobHandle>();
		public static Transform orbTransform;
		public const int maxDist=100;
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
						grid[i,j].realDistance=-1;
					}else{
						grid[i,j].distance=999999;
						grid[i,j].realDistance=-1;
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
		}

		public static float VecToAngle(Vector3 v){
			float angle=90;
			if(v.y!=0){
				angle = -180*Mathf.Atan(v.x/v.y)/Mathf.PI;
				if(v.y<0){
				   angle+=180;
				}
			}else if(v.x>0){
				angle=-90;
			}else if(v.x==0){
				angle=0;
			}

			if(angle<0){
				angle+=360;
			}
			return angle;
		}

		// public static IEnumerator MapGen(){
		public static void MapGen(){
			// while(true){
			// 	yield return new WaitForSeconds(1);
			// 	Effects.Explode(new Vector3(0,5),1,null);
			// 	Effects.Explode(new Vector3(0,-5),2,null);
			// }

			Item it = ItemRatio.table[0].item.FromTemplate(1,1);
			GameObject g= it.ToGameObject(NearestHex(new Vector3(6,6,0)));
			// EnemyFsm o = MeshGens.ObjGen(Shapes.star,MatColour.white);
			// o.transform.position=new Vector3(-6,-6);
			EnemyFsm o=null;
			MeshGens.MinObjGen(Shapes.puddle,MatColour.white);
			for(int i=0;i<7;i++){
				o = EnemyGeneration.ObjGen((Shapes.circle),MatColour.white);
				o.transform.position = new Vector3(2*i-11,-5);
			}
			// g.GetComponent<Damageable>().Draw(f);
		}

		public static void Print(){
			int[] wop = World.WorldPos(orbTransform.position);
			Debug.Log("saving");
			string r = "";
			for(int i =0;i<size[1];i++){
				for(int j=0;j<size[0];j++){
					// if(grid[j,i].distance*playerCare>grid[j,i].plrDistance){
					//     r+="p";
					// }
					if(i==wop[0]&&j==wop[1]){
						r+=string.Format("orbb, ");
					} else{
						r+=string.Format("{0:00.0}, ",grid[j,i].distance);
					}
				}
				r+="\n";
			}
			File.WriteAllText(@"/home/mj/grid.txt",r);
		}


		public static void clearGrid(bool main){
			for(int i=0;i<size[0];i++){
				for(int j=0;j<size[1];j++){
					grid[i,j].distance=largeDist;
					grid[i,j].realDistance=largeDist;
					grid[i,j].next=grid[i,j];
				}
			}
		}

		static List<Node> BorderNode=new List<Node>();
		static Queue<Node> LookAt;

		static void EnsureIntegrity(){
			foreach (JobHandle jh in jobs){
				jh.Complete();
			}
			jobs.Clear();
		}

		struct PlaceOrbJob : IJob{
			public int x,y;

			public void Execute(){
				clearGrid(true);
				grid[x,y].distance=0;
				grid[x,y].realDistance=0;
				grid[x,y].next=grid[x,y];
				LookAt.Enqueue(grid[x,y]);
				while(LookAt.Count>0){
					CheckNode(LookAt.Dequeue());
				}
			}

		}

		static JobHandle SafeJobHandler<J>(J job) where J : struct,IJob {
			EnsureIntegrity();
			JobHandle jobHandle = job.Schedule();
			jobs.Add(jobHandle);
			return jobHandle;

		}

		//handler for PlaceOrb Thread
		public static JobHandle PlaceOrb(int x,int y){
			return SafeJobHandler(new PlaceOrbJob(){x=x,y=y});

		}

		public struct StateData{
			public int x;
			public int y;
			public NodeState s;
		}

		struct ChangeStatesJob : IJob{
			public List<StateData> data;
			public void Execute(){
				foreach(StateData sd in data){
					grid[sd.x,sd.y].state =sd.s;
				}
				Reset();
			}
		}

		//handler for changestates method
		public static JobHandle ChangeStates(List<StateData> data){
			return SafeJobHandler(new ChangeStatesJob(){data=data});
		}

		public static void Reset(){
			int[] wop = World.WorldPos(orbTransform.position);
			PlaceOrb(wop[0],wop[1]);
		}

		struct ChangeStateJob : IJob{
			public int x;
			public int y;
			public NodeState s;
			public ChangeStateMethod m;
			public void Execute(){
				Node n=grid[x,y];
				LookAt.Enqueue(n);
				n.state|=NodeState.updating;
				switch(m){
					case ChangeStateMethod.On: n.state|=s;break;
					case ChangeStateMethod.Off: n.state&=~s;break;
					case ChangeStateMethod.Flip: n.state^=s;break;
				}
				ManageOutNodes();
			}
		}

		//handler for changestates method
		public static JobHandle ChangeState(int x,int y,NodeState s,ChangeStateMethod m=ChangeStateMethod.Flip){
			ChangeStateJob csj = new ChangeStateJob{
				x=x,y=y,s=s,m=m
			};
			return SafeJobHandler(csj);
		}

		public enum ChangeStateMethod{
			On,Off,Flip
		}

		struct ChangeStatesInRangeJob : IJob{
			public int x;
			public int y;
			public float range;
			public NodeState s;
			public ChangeStateMethod m;

			public void Execute(){
				float r2 = range*range;
				float realX=x;
				LookAt.Enqueue(grid[x,y]);
				if(y%2!=0){realX+=0.5f;}
				for(int i=Mathf.Max(0,x-(int)range);i<=x+(int)range && i < size[0];i++){
					float dx=realX-i;
					for(int j=Mathf.Max(0,y-(int)range);j<=y+(int)range && j < size[1];j++){
						if(j%2!=0){dx-=0.5f;}
						float dy = y-j;
						float d2 = (dy*dy * 3/4) + dx*dx;
						if(d2<=r2){
							Node n = grid[i,j];
							n.state|=NodeState.updating;
							switch (m) {
								case ChangeStateMethod.On: n.state|=s;break;
								case ChangeStateMethod.Off: n.state&=~s;break;
								case ChangeStateMethod.Flip: n.state^=s;break;
							}
						}
					}
				}
				ManageOutNodes();
			}
		}

		//handler for changestates method
		public static JobHandle ChangeStatesInRange(int x,int y,float range,NodeState s,ChangeStateMethod m=ChangeStateMethod.Flip){
			return SafeJobHandler(new ChangeStatesInRangeJob(){
							x=x,
							y=y,
							range=range,
							s=s,
							m=m,
							});
		}

		public static void ManageOutNodes(){
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
			if(node.distance==largeDist){return;}
			List<NodeData> ns = Neighbors(node);
			node.distance=largeDist;
			foreach(NodeData data in ns){
				bool invalidPath=false;
				if(data.n.over!=null){
					invalidPath |= (data.n.over[0].distance==largeDist);
					invalidPath |= (data.n.over[1].distance==largeDist);
				}
				invalidPath |= (data.n.next.distance==largeDist);
				invalidPath |= ((data.n.state&NodeState.updating)!=0);
				data.n.state&=~NodeState.updating;
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
				float multiplier = (float)node.state;
				if (data.over!=null){
					addedDist=1.5f;
					foreach(Node n in data.over){
						multiplier+=((float)n.state-1);
					}
				}
				float ad = addedDist;
				addedDist*=multiplier;
				if(node.distance+addedDist<data.n.distance && node.distance+addedDist<maxDist){
					data.n.distance=node.distance+addedDist;
					data.n.realDistance=node.realDistance+ad;
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

			int offset = ((node.y)%2);
			//check initial direction
			Direction nd = directions[0];
			if(nd.y!=0){nd.x+=offset;}
			bool initial = node.Valid(nd);
			Node initialNode=null;
			if(initial){
				initialNode = (grid[node.x+nd.x,node.y+nd.y]);
				if(initialNode.state==NodeState.ground){
					initialNode=null;
					initial=false;
				}else{
					r.Add(new NodeData{n=initialNode});
				}
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
					if(currentNode.state!=NodeState.ground){
						r.Add(new NodeData{n=currentNode});
						//check long direction
						Direction md = longDirections[i];
						if(md.y%2!=0){
							md.x+=offset;
						}
						//if all is well add to the list
						if(correct&&node.Valid(md)){
							Node n = grid[node.x+md.x,node.y+md.y];
							if(n.state!=NodeState.ground){
								r.Add(new NodeData{n=n,over=new Node[]{previousNode,currentNode}});
							}

						}
					}else{
						currentNode=null;
						valid=false;
					}
				}
				correct=valid;
				previousNode=currentNode;
			}
			//check final long direction
			if(correct && initial && node.Valid(longDirections[0])){
				Node longNode = (grid[node.x+longDirections[0].x,node.y+longDirections[0].y]);
				if(longNode.state!=NodeState.ground){
					r.Add(new NodeData{n=longNode,over=new Node[]{previousNode,initialNode}});
				}
			}
			return r;
		}

		public static int[] WorldPos(Vector3 v){
			// (int)Mathf.Round(v.x+(size[0]/2))
			int[] p = new int[2]{0,(int)Mathf.Round(v.y/hexVec.y+(size[1]/2))};
			if(p[1]%2!=0){v.x-=hexVec.x;}
			p[0]=(int)(Mathf.Round(v.x+size[0]/2));
			return p;
		}

		public static Vector3 NearestHex(Vector3 v){
			v.y=Mathf.Round(v.y/hexVec.y);
			if(v.y%2!=0){
				v.x=Mathf.Round(v.x-hexVec.x)+hexVec.x;
			}else{
				v.x=Mathf.Round(v.x);
			}
			v.y*=hexVec.y;
			return v;
		}


		public static List<Vector2Int> HexesInRange(int x,int y,float range){
			List<Vector2Int> r=new List<Vector2Int>();
			float r2 = range*range;
			float realX=x;
			if(y%2!=0){realX+=0.5f;}
			for(int i=Mathf.Max(0,x-(int)range);i<x+(int)range && i < size[0];i++){
				float dx=realX-i;
				for(int j=Mathf.Max(0,y-(int)range);j<y+(int)range && j < size[1];j++){
					if(j%2!=0){dx-=0.5f;}
					float dy = y-j;
					float d2 = (dy*dy * 3/4) + dx*dx;
					if(d2<=r2){
						r.Add(new Vector2Int(i,j));
					}
				}
			}
			return r;
		}
	}

	public enum NodeState{
		empty=1,
		wall=4,
		targeted=8,
		ground=128,
		updating=256
	}

	[System.Serializable]
	public class Node{
		public NodeState state=NodeState.empty;
		public int sint;
		[SerializeReference]
		public Node next=null;
		public Node[] over=null;
		public float distance=99999;
		public float realDistance=9999;
		public int x;public int y;

		public GameObject RenderNode(){
			GameObject o =MeshGens.MinObjGen(Shapes.hexagonOuter,MatColour.rebeccaOrangeAnti);
			GameObject a = MeshGens.MinObjGen(Shapes.arrow,MatColour.rebeccaOrange);
			sint=(int)state;
			Vector3 p =WorldPos();
			o.transform.SetParent(World.gridObj.transform);
			o.transform.localPosition=p;
			a.transform.SetParent(o.transform);
			Vector3 nextDist =next.WorldPos() - p;
			a.transform.localPosition=(nextDist)/2;
			a.transform.eulerAngles = new Vector3(0,0,World.VecToAngle(nextDist));
			a.transform.localScale=new Vector3(1,nextDist.magnitude,1);
			return o;
		}

		public string ToString(){
			return x.ToString() + " , " + y.ToString();
		}


		public Vector3 WorldPos(){
			Vector3 v = new Vector3(x,y);
			if(y%2!=0){
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
				if(dx==0 ^ y%2!=0){
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
}
