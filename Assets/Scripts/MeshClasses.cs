using UnityEngine;
public static class MeshGens{

	struct PointData{
		public Vector3[] p;
		public Vector2[] u;
	}

	//triangle=3
	//square=4
	//circle=16
	public static Mesh Circle(Vector2 size,int v,float offset=0){
		return GenMesh(CirclePoints(size,v,offset),GenTri(v));
	}
	public static Mesh Circle(float size,int v,float offset=0){
		return GenMesh(CirclePoints(size,v,offset),GenTri(v));
	}

	public static Mesh Semicircle(float size,int v=8,float offset=0){
		PointData d = SemiPoints(size,v,offset+3*Mathf.PI/2);
		for(int i=0;i<v;i++){
			d.p[i]-=new Vector3(0,size/2);
			d.u[i]=d.p[i];
		}
		return GenMesh(d,GenTri(v));
	}

	static PointData CirclePoints(float size,int v,float offset=0){
		return CirclePoints(new Vector2(size,size),v,offset);
	}

	static PointData CirclePoints(Vector2 size,int v,float offset=0){
		Vector3[] points = new Vector3[v];
		Vector2[] uv = new Vector2[v];
		float r;
		for(int i=0;i<v;i++){
			r = offset+2*Mathf.PI*(float)(i)/v;
			points[i] = new Vector3((Mathf.Sin(r))*size.x,(Mathf.Cos(r))*size.y);
			uv[i] = points[i];
		}
		return new PointData{p=points,u=uv};
	}

	static Mesh GenMesh(PointData d,int[] tris){
		Mesh m = new Mesh();
		m.vertices=d.p;
		m.uv=d.u;
		m.triangles=tris;
		return m;
	}
	static Mesh GenMesh(Vector3[] point,Vector2[] uv,int[] tris){
		Mesh m = new Mesh();
		m.vertices=point;
		m.uv=uv;
		m.triangles=tris;
		return m;
	}

	static PointData SemiPoints(float size,int v,float offset=0){
		Vector3[] points = new Vector3[v];
		Vector2[] uv = new Vector2[v];
		float r;
		for(int i=0;i<v;i++){
			r = offset+Mathf.PI*(float)(i)/(v-1);
			points[i] = size* new Vector3(Mathf.Sin(r),Mathf.Cos(r));
			uv[i] = points[i];
		}
		return new PointData{p=points,u=uv};
	}

	public static int[] GenTri(int v){
		int[] tris=new int[(v-2)*3];
		for(int i=0;i<v-2;i++){
			int j = i*3;
			tris[j]=0;
			tris[j+1]=i+1;
			tris[j+2]=i+2;
		}
		return tris;
	}

	public static Mesh CircleOutline(float size,int v=6,float offset=0,float thickness=0.8f){
		Vector3[] points = new Vector3[v*2];
		Vector2[] uvs = new Vector2[v*2];
		int[] tris = new int[v*6];
		PointData outer = CirclePoints(size,v,offset);
		PointData inner = CirclePoints(size*thickness,v,offset);
		for(int i=0;i<v;i++){
			points[i]=inner.p[i];
			uvs[i]=inner.u[i];
		}
		for(int i=0;i<v;i++){
			points[i+v]=outer.p[i];
			uvs[i+v]=outer.u[i];
		}
		for(int i=0;i<v;i++){
			int j=6*i;
			tris[j+2]=i;
			tris[j+1]=(i+1)%v;
			tris[j]=(i+1)%v + v;
			tris[j+5]=i;
			tris[j+4]=tris[j];
			tris[j+3]=i+v;
		}
		return GenMesh(points,uvs,tris);
	}

	public static Mesh Star(float size, int v=5,float offset=0){
		PointData dinner = CirclePoints(size/3,v,offset-(Mathf.PI/v));
		PointData douter = CirclePoints(size,v,offset);
		Vector3[] points=new Vector3[v*2];
		Vector2[] uv=new Vector2[v*2];
		int[] tris=new int[6*(v-1)];
		for(int i=0;i<v;i++){
			int j=i*3;
			tris[j]=v+i;
			tris[j+1]=i;
			tris[j+2]=v+ (i+1)%v;
			points[i]=douter.p[i];
			points[v+i]=dinner.p[i];
			uv[i]=douter.u[i];
			uv[v+i]=dinner.u[i];
		}
		for(int i=0;i<v-2;i++){
			int j=(i+v)*3;
			tris[j]=v;
			tris[j+1]=v+1+i;
			tris[j+2]=v+2+i;
		}
		return GenMesh(points,uv,tris);
	}

	public static Mesh Quaterfoil(float size,int v){
		int vFix=v-1;
		Vector3[] points = new Vector3[vFix*4];
		Vector2[] uv = new Vector2[vFix*4];
		int[] tris = new int[12*vFix-6];
		int k=0;
		for(int i=0;i<4;i++){
			float r = i*Mathf.PI/2;
			Vector3 pointOffset = new Vector2(size*Mathf.Cos(r)/2,-size*Mathf.Sin(r)/2);
			PointData pd = SemiPoints(size/2,v,r);
			for(int j=0;j<v-1;j++){
				points[vFix*i+j] = pd.p[j]+pointOffset;
				uv[(vFix)*i+j] = pd.p[j]+pointOffset;
			}
			for(int j=0;j<v-2;j++){
				int s = i*3*(vFix-1);//this is the base index for j=0
				k = s+j*3;//this is the base index for j=j
				tris[k]=i*(vFix);
				tris[k+1]=i*vFix+j+1;
				tris[k+2]=(i*vFix+j+2)%(vFix*4);
			}
		}
		int tl = 12*vFix - 6;
		k+=3;
		for(int i=0;i<2;i++){
			tris[k+(i*3)]=0;
			tris[k+(i*3)+1]=(i+1)*(vFix);
			tris[k+(i*3)+2]=(i+2)*(vFix);
		}
		return GenMesh(points,uv,tris);
	}

	public static Mesh Curvilinear(float size,int v,float extension){
		int n = v*3;
		int k=0;
		if(extension<0){
			n++;
			k=1;
		}
		Vector3[] points = new Vector3[n];
		Vector2[] uv = new Vector2[n];
		if(extension<0){
			points[0]=new Vector3(0,0);
			uv[0]=new Vector3(0,0);
		}
		for (int i =0;i<3;i++){
			float angle = Mathf.PI*i*2/(float)3;
			float nextAngle = Mathf.PI*(i+1)*2/(float)3;
			float extensionAngle = angle+Mathf.PI/3;
			Vector3[] square = new Vector3[4];
			square[2] = extension*new Vector3(Mathf.Sin(extensionAngle),Mathf.Cos(extensionAngle));
			square[0] = size*new Vector3(Mathf.Sin(angle),Mathf.Cos(angle));
			square[1] = square[0]+square[2];
			square[3] = size*new Vector3(Mathf.Sin(nextAngle),Mathf.Cos(nextAngle));
			square[2] = square[2]+square[3];
			//bezier curve
			for(int j=0;j<v;j++){
				int idx =i*v+j+k;
				float t = j/(float)v;
				float tx = 1-t;
				Vector3 a,b,c;
				a = square[0]*(tx)+square[1]*(t);
				b = square[1]*(tx)+square[2]*(t);
				c = square[2]*(tx)+square[3]*(t);
				a = a*(tx)+b*(t);
				b = b*(tx)+c*(t);
				points[idx] = a*(tx)+b*(t);
				uv[idx]=points[idx];
			}
		}
		int[] tris = GenTri(n);
		return GenMesh(points,uv,tris);
	}

	static string[] shapeNames = new string[]{
		"star",
		"cross",
		"circle",
		"semicircle",
		"quaterfoil",
		"square",
		"rectangle",
		"octogon",
		"triangle",
		"diamond",
		"curvilinear",
		"hexagonOuter",
		"hexagon"
	};
	static Mesh[] meshes = new Mesh[13];
	static Material[] colors= new Material[10];

    [RuntimeInitializeOnLoadMethod]
	static void StructureGen(){
		MeshGen();
		ColourGen();
	}

	public static GameObject MinObjGen(Shapes shape,MatColour m){
		GameObject obj = new GameObject(shapeNames[(int)shape]);
		obj.AddComponent<MeshFilter>().mesh=meshes[(int)shape];
		obj.AddComponent<MeshRenderer>().material=colors[(int)m*2];
		return obj;
	}

	public static EnemyFsm ObjGen(Shapes shape,MatColour m){
		GameObject obj = new GameObject(shapeNames[(int)shape]);
		obj.AddComponent<MeshFilter>().mesh=meshes[(int)shape];
		obj.AddComponent<MeshRenderer>().material=colors[(int)m*2];

		//genchild
		GameObject child = new GameObject(shapeNames[(int)shape]+"_inner");
		child.AddComponent<MeshFilter>().mesh=meshes[(int)shape];
		child.AddComponent<MeshRenderer>().material=colors[(int)m*2+1];
		child.transform.localScale=new Vector3(0.8f,0.8f,0);
		child.transform.position+=new Vector3(0,0,-0.001f);
		child.transform.SetParent(obj.transform);

		obj.transform.position+=new Vector3(0,0,-0.1f);
		obj.layer=6;
		obj.AddComponent<CircleCollider2D>();
		Rigidbody2D r  = obj.AddComponent<Rigidbody2D>();
		r.constraints=RigidbodyConstraints2D.FreezeRotation;
		r.gravityScale=0;
		r.drag=2;
		obj.AddComponent<followAi>().force=10;

		EnemyFsm e;
		switch (shape) {
			case Shapes.star:
				e=obj.AddComponent<StarEnemy>();
				child.transform.localScale=new Vector3(0.6f,0.6f,0);
				break;
			case Shapes.cross:
				e=obj.AddComponent<CrossEnemy>();
				child.transform.localScale=new Vector3(0.6f,0.6f,0);
				break;
			case Shapes.circle:
				e=obj.AddComponent<CircleEnemy>();
				break;
			case Shapes.semicircle:
				e=obj.AddComponent<SemicircleEnemy>();
				break;
			case Shapes.quaterfoil:
				e=obj.AddComponent<QuaterfoilEnemy>();
				break;
			case Shapes.square:
				e=obj.AddComponent<SquareEnemy>();
				break;
			case Shapes.rectangle:
				e=obj.AddComponent<RectangleEnemy>();
				break;
			case Shapes.octogon:
				e=obj.AddComponent<OctogonEnemy>();
				break;
			case Shapes.triangle:
				e=obj.AddComponent<TriangleEnemy>();
				break;
			case Shapes.diamond:
				e=obj.AddComponent<DiamondEnemy>();
				break;
			case Shapes.curvilinear:
				e=obj.AddComponent<CurvilinearEnemy>();
				break;
			default:
				e=obj.AddComponent<EnemyFsm>();
				break;
		}
		return e;
	}

	static void MeshGen(){
		meshes[(int)Shapes.star] = Star(0.5f);
		meshes[(int)Shapes.star].name ="star";
		meshes[(int)Shapes.cross] = Star(0.5f,4);
		meshes[(int)Shapes.cross].name ="cross";
		meshes[(int)Shapes.circle]=Circle(0.5f,16);
		meshes[(int)Shapes.circle].name="circle";
		meshes[(int)Shapes.triangle]=Circle(0.5f,3);
		meshes[(int)Shapes.triangle].name="triangle";
		meshes[(int)Shapes.square]=Circle(0.5f,4,Mathf.PI/4);
		meshes[(int)Shapes.square].name="square";
		meshes[(int)Shapes.semicircle]=Semicircle(0.5f);
		meshes[(int)Shapes.semicircle].name="semicircle";
		meshes[(int)Shapes.quaterfoil]=Quaterfoil(0.5f,9);
		meshes[(int)Shapes.quaterfoil].name="quaterfoil";
		meshes[(int)Shapes.diamond]=Circle(new Vector2(0.35f,0.5f),4);
		meshes[(int)Shapes.diamond].name="diamond";
		meshes[(int)Shapes.rectangle]=Circle(new Vector2(0.35f,0.5f),4,Mathf.PI/4);
		meshes[(int)Shapes.rectangle].name="rectangle";
		meshes[(int)Shapes.curvilinear]=Curvilinear(0.5f,6,0.2f);
		meshes[(int)Shapes.curvilinear].name="curvilinear";
		meshes[(int)Shapes.octogon]=Circle(1,8,Mathf.PI/8);
		meshes[(int)Shapes.octogon].name="octogon";
		meshes[(int)Shapes.hexagonOuter]=CircleOutline(Mathf.Sqrt(3)/3,6,0,0.92f);
		meshes[(int)Shapes.hexagonOuter].name="hexagonOuter";
		meshes[(int)Shapes.hexagon]=Circle(Mathf.Sqrt(3)/3,6);
		meshes[(int)Shapes.hexagon].name="hexagon";
	}

	static void ColourGen(){
		ColourMats(MatColour.white,Color.white);
		ColourMats(MatColour.red,Color.red);
		ColourMats(MatColour.green,Color.green);
		ColourMats(MatColour.blue,Color.blue);
		ColourMats(MatColour.black,Color.black);
	}

	const float blackness=1/8f;
	static void ColourMats(MatColour color,Color c){
		Shader s = Shader.Find("Unlit/Color");
		colors[(int)color*2]=new Material(s);
		colors[(int)color*2].color=c;
		colors[(int)color*2+1]=new Material(s);
		colors[(int)color*2+1].color=new Color(blackness*c.r,blackness*c.g,blackness*c.b);
	}
}

public enum Shapes{
	star,
	cross,
	circle,
	semicircle,
	quaterfoil,
	square,
	rectangle,
	octogon,
	triangle,
	diamond,
	curvilinear,
	hexagonOuter,
	hexagon
};

public enum MatColour{
	white,
	red,
	blue,
	green,
	black
}
