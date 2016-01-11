using UnityEngine;
using System.Collections;

public class StarScript : MonoBehaviour {

    GameObject starEffect;
    // Use this for initialization
    void Start () {
        starEffect = (GameObject)Resources.Load("Effect/" + this.name);
        //Destroy(this.gameObject, 2.0f);
    }

    private bool firstFlg = false;
    void OnTriggerEnter2D(Collider2D collider){
        if(collider.gameObject.tag == "Chara" && !firstFlg){
            firstFlg = true;
            //StartCoroutine(rockDestroy());
            GameObject star = Instantiate (starEffect, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), Quaternion.identity) as GameObject;
            star.transform.localScale = new Vector3(1, 1, 1);
            Destroy(this.gameObject, 0.05f);
        }
    }
}
