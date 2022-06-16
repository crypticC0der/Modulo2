using UnityEngine;

public static class Effects{
	public static void Explode(Vector3 center,float strength){
		//generate particles
		Collider2D[] c = Physics2D.OverlapCircleAll(center,10*strength);
		foreach(Collider2D col in c){
			Damageable D = col.GetComponent<Damageable>();
			if(D){
				D.ApplyDebuff<Burning>((int)(2*strength));
				D.TakeDamage(100*strength);
			}
		}
	}
}
