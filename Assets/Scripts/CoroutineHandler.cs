using UnityEngine;
using System.Collections;
public class CoroutineHandler : MonoBehaviour {
    public static CoroutineHandler handler;
    public void Start() { handler = this; }
}
