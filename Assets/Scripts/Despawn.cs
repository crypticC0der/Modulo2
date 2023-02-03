using UnityEngine;
using System.Collections.Generic;

namespace Modulo {

/// <summary>
/// makes a object destroy itself after a couple seconds
/// </summary>
public class Despawn : MonoBehaviour {
    public float deathTimer = 0;
    public virtual void FixedUpdate() {
        deathTimer -= Time.deltaTime;
        if (deathTimer < 0) {
            Destroy(gameObject);
        }
    }
}

public class ArrowDespawn : MonoBehaviour {
    public float range;
    public void Start() { range *= Mathf.Sqrt(3); }
    public void FixedUpdate() {
        transform.localScale = Vector2.LerpUnclamped(
            transform.localScale, Vector2.one * range, Time.deltaTime * 7);
        if (range - transform.localScale.x < 0.1f) {
            Destroy(gameObject);
        }
    }
}

public class PolarMove : MonoBehaviour {
    public Rigidbody2D rb;
    public Transform perent;

    public void FixedUpdate() {
        Vector2 d = transform.position - perent.position;
        Vector2 dp = new Vector2(d.y, -d.x);
        Vector2 v = 0.1f * d + 0.9f * dp;
        rb.velocity = v.normalized * rb.velocity.magnitude;

        // if(initialAngle==420){
        // 	initialAngle= Mathf.Atan(d.x/d.y);
        // 	if(d.y<0){
        // 		initialAngle+=Mathf.PI;
        // 	}
        // }
        // float journey = d.magnitude/range;
        // float rads = journey*circles*Mathf.PI*2 + initialAngle;
        // transform.position=new
        // Vector3(Mathf.Sin(rads),Mathf.Cos(rads))*d.magnitude+perent.position;
        // rb.velocity=d.normalized*rb.velocity.magnitude;
    }
}

public class SpikeMove : MonoBehaviour {
    public Vector3 target;
    public float spd;

    public virtual void FixedUpdate() {
        Vector3 t = Vector2.LerpUnclamped(transform.position, target,
                                          Time.deltaTime * 2 * spd / 5);
        t.z = -1;
        transform.position = t;
    }
}

public class RayTracking : Despawn {
    public LineRenderer lineRenderer;
    public Attack perent;
    public override void FixedUpdate() {
        if (perent.perent != null) {
            if (perent.t == AttackType.Area) {
                lineRenderer.SetPosition(0, ((AreaAttack)(perent)).center);
            } else {
                lineRenderer.SetPosition(0, perent.perent.transform.position +
                                                Vector3.forward);
            }
        }
        base.FixedUpdate();
    }
}

public class RayDespawnTracking : MonoBehaviour {
    public DamageData data;
    public Attack perent;
    public LineRenderer lineRenderer;
    public LinkedList<Damageable> aims;
    public float length;
    public void FixedUpdate() {
        if (data.sender == null) {
            Destroy(gameObject);
            return;
        }

        DamageData dataScaled =
            new DamageData { dmg = data.dmg * Time.deltaTime,
                             sender = data.sender,
                             properties = data.properties };

        // combobulate List
        LinkedListNode<Damageable> aimNode = aims.First;
        Vector3 prevPos = data.sender.transform.position;
        int nodes = 0;
        while (aimNode != null) {
            if (aimNode.Value == null) {
                LinkedListNode<Damageable> die = aimNode;
                aimNode = aimNode.Next;
                aims.Remove(die);
            } else {
                Vector3 d = aimNode.Value.transform.position - prevPos;
                if (d.magnitude > length * 1.5f) {
                    LinkedListNode<Damageable> die = aimNode;
                    aimNode = aimNode.Next;
                    aims.Remove(die);
                } else {
                    nodes++;
                    RaycastHit2D[] raycastHits =
                        Physics2D.RaycastAll(prevPos, d, d.magnitude,
                                             perent.perent.layerMask(false));
                    LinkedListNode<Damageable> searchNode = aims.First;
                    LinkedListNode<Damageable> aimNodeHead = aimNode;
                    foreach (RaycastHit2D hit in raycastHits) {
                        searchNode = aims.First;
                        bool newHit = true;
                        while (searchNode != null) {
                            if (searchNode.Value.gameObject ==
                                hit.collider.gameObject) {
                                newHit = false;
                                break;
                            }
                            searchNode = searchNode.Next;
                        }
                        if (newHit) {
                            Damageable newDamage =
                                hit.collider.gameObject
                                    .GetComponent<Damageable>();
                            if ((perent.attackProperties() &
                                 SpecialProperties.homing) != 0) {
                                LinkedListNode<Damageable> dNode =
                                    new LinkedListNode<Damageable>(newDamage);
                                aims.AddBefore(aimNodeHead, dNode);
                                nodes++;
                                aimNodeHead = dNode;
                            } else {
                                newDamage.TakeDamage(dataScaled);
                            }
                        }
                    }
                    prevPos = aimNode.Value.transform.position;
                    aimNode = aimNode.Next;
                }
            }
        }

        Vector3[] posititons = new Vector3[nodes + 1];
        posititons[0] = data.sender.transform.position;
        int i = 0;
        aimNode = aims.First;
        bool remainingEnemies = false;
        float currentLength = 0;
        while (aimNode != null) {
            posititons[i + 1] = aimNode.Value.transform.position;
            currentLength += (posititons[i + 1] - posititons[i]).magnitude;
            i++;
            dataScaled.direction = aimNode.Value.transform.position -
                                   data.sender.transform.position;
            perent.DmgOverhead(dataScaled, aimNode.Value);
            remainingEnemies = true;
            aimNode = aimNode.Next;
        }
        posititons[posititons.Length - 1] += Vector3.forward;
        lineRenderer.positionCount = posititons.Length;
        lineRenderer.SetPositions(posititons);
        if (currentLength > length * 1.5f + ((nodes - 1) * 4f) ||
            !remainingEnemies) {
            Destroy(gameObject);
        }

        // lineRenderer.SetPosition(0,aims[0].position);
    }
}
}
