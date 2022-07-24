using UnityEngine;
using System.Collections.Generic;
public class GasBomb: MonoBehaviour{
	public Proc perent;
	public int perentLayer;
	public SpriteRenderer sr;
	public Rigidbody2D rb;
	public float lossTime;
	public float initialDensity=0.5f;
	public float spd=-1;
	public void Start(){
		sr=GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
		if(spd==-1){
			spd=Random.Range(0.5f,1.5f);
			transform.eulerAngles=new Vector3(0,0,Random.Range(0,360));
		}
	}

	public void FixedUpdate(){
		float scale=transform.localScale.x+Time.deltaTime*lossTime;
		transform.localScale=Vector3.one*scale;
		float density=initialDensity/(scale*scale);
		Color c = sr.color;
		c.a=density/2;
		sr.color=c;

		Vector3 angle = transform.eulerAngles;
		angle.z+=spd*45*Time.deltaTime;
		transform.eulerAngles=angle;
		Vector3 velocity=rb.velocity;
		if(density<0.01){
			Destroy(gameObject);
		}
		if(scale>2){
			transform.localScale=Vector3.one;
			GasBomb[] gasBombs=new GasBomb[4];
			gasBombs[0]=this;
			for(int i=0;i<3;i++){
				gasBombs[i+1] = GameObject.Instantiate(gameObject).GetComponent<GasBomb>();
			}
			for(int i=0;i<4;i++){
				float r  = i*Mathf.PI/2 + Mathf.PI/4;
				Vector2 direction = new Vector2(Mathf.Sin(r),Mathf.Cos(r));
				gasBombs[i].initialDensity=density;
				gasBombs[i].perent=perent;
				gasBombs[i].transform.position+=(Vector3)direction* 1/2;
				gasBombs[i].spd=Random.Range(0.5f,1.5f);
				gasBombs[i].rb.velocity=Vector2.Lerp(velocity.normalized,direction,0.2f)*velocity.magnitude*gasBombs[i].spd;
				gasBombs[i].lossTime=lossTime;
				gasBombs[i].sr.color=c;
			}
		}
	}

	public void OnTriggerStay2D(Collider2D c){
		if ((1<<c.gameObject.layer & perentLayer)!=0)
		{
			Damageable d = c.GetComponent<Damageable>();
			if(d!=null){
				perent.collider=transform;
				float dmg =perent.dmg;
				float pc = perent.procCoefficent;
				float multiplier=Time.deltaTime * 2*Mathf.Min(initialDensity,0.5f)/(transform.localScale.x*transform.localScale.x);
				perent.dmg*=multiplier*2;
				perent.procCoefficent*=multiplier;
				perent.OnProc(d);
				perent.dmg=dmg;
				perent.procCoefficent=pc;
			}
		}
	}

}
