using UnityEngine;

namespace Modulo{

	public class CameraFollow : MonoBehaviour{
		public void LateUpdate(){
			Vector3 playerPos = PlayerBehavior.me.transform.position;
			playerPos.z=transform.position.z;
			transform.position = Vector3.LerpUnclamped(transform.position,playerPos,0.9f*Time.deltaTime);
		}
	}

}
