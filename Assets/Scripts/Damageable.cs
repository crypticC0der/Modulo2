using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour{
    public float health=200;
    public float maxHealth=200;
    public float regen=1;

    public virtual void FixedUpdate(){
        health+=regen*Time.deltaTime;
    }

    public void TakeDamage(float d){
        health-=d;

        if(health<0){
            health=0;
            int[] p = World.WorldPos(gameObject.transform.position);
            World.RemoveConstruct(p[0],p[1]);
            GameObject.Destroy(gameObject);
        }

        if(PlayerBehavior.dmg==this){
            PlayerBehavior.controller.bars[0].UpdateValue(health,maxHealth);
        }
    }
}
