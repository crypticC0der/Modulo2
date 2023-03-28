using UnityEngine;

namespace Modulo {

public class CameraFollow : MonoBehaviour {
    public float speed=0.8f;
    Vector3 vel=Vector3.zero;
    public void FixedUpdate() {


        Vector3 playerPos = PlayerBehavior.me.transform.position;
        playerPos.z = transform.position.z;
        // transform.position=playerPos;
        // transform.position = Vector3.SmoothDamp(transform.position,
        //                                         playerPos,ref vel,speed);
        transform.position = Vector3.Lerp(
            transform.position, playerPos,
            1-Mathf.Pow(1-speed*3f,Time.fixedDeltaTime*60));
    }
}

}

