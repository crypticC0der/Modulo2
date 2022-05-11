using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FPS : MonoBehaviour{
    public TextMeshProUGUI tmp;
    public void Update(){
        if(Time.frameCount%100==0){
            tmp.text = (1/Time.deltaTime).ToString();
        }
    }
}
