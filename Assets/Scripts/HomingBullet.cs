using UnityEngine;

public class HomingBullet : MonoBehaviour{
	public Rigidbody2D rb;
	public float spd;
	public int layerMask = 0;

	public Vector2 integral;
	public Vector2 previousError;
	public void Start(){
		integral=new Vector2(0,0);
	}


	public void FixedUpdate(){
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,5,layerMask);
		int i=0;
		int bestI=-1;
		float bestDot=999;
		while(hits.Length>i){
			Vector2 d = hits[i].transform.position-transform.position;
			float dot = d.magnitude;// Vector2.Dot(d,rb.velocity)/(rb.velocity.magnitude*d.magnitude);
			if(dot<bestDot){
				bestDot=dot;
				bestI=i;
			}
			i++;
		}
		if(bestI!=-1){
			Vector2 direction= hits[bestI].transform.position-transform.position;
			direction= direction.normalized*spd;
			rb.velocity = Vector2.LerpUnclamped(direction,rb.velocity,0.8f);
		}


        if(rb.velocity.y!=0){
            float angle;
            angle = -180*Mathf.Atan(rb.velocity.x/rb.velocity.y)/Mathf.PI;
            if(rb.velocity.y<0){
               angle+=180;
            }
            transform.eulerAngles=new Vector3(0,0,angle);
        }
	}

}
