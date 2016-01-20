using UnityEngine;
using System.Collections;

public class BoosterScript : MonoBehaviour {

    // Use this for initialization
    private float defaultX;
    private float defaultY;
    void Start () {
        defaultX = this.gameObject.transform.localEulerAngles.x;
        defaultY = this.gameObject.transform.localEulerAngles.y;

        iTween.ValueTo(this.gameObject, iTween.Hash(
                    "from", 22,
                    "to", -22,
                    "time", 0.8f,
                    "looptype","pingpong",
                    "onupdate", "valueChange"
                    ));
    }

    void valueChange(float value){
        transform.rotation = Quaternion.Euler(defaultX, defaultY, value);
    }

    private bool firstFlg = false;
    void OnTriggerEnter2D(Collider2D collider){
        if(collider.gameObject.tag == "Chara" && !firstFlg){
            firstFlg = true;
            Destroy(this.gameObject, 0.05f);
        }
    }
}
