using UnityEngine;

namespace Modulo {

public class ReturningShot : MonoBehaviour {
    public float rotationalVelocity;
    public Rigidbody2D r;
    public Attack perent;
    public void Start() {
        rotationalVelocity = perent.shotSpeed() / perent.attackRange();
        rotationalVelocity = rotationalVelocity * rotationalVelocity;
    }

    public void FixedUpdate() {
        r.velocity -=
            rotationalVelocity *
            (Vector2)(transform.position - perent.perent.transform.position) *
            Time.deltaTime;
    }
}
}
