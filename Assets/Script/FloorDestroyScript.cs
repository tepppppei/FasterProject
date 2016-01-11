using UnityEngine;
using System.Collections;

public class FloorDestroyScript : MonoBehaviour {

    // Use this for initialization
    void Start () {
    }

    void OnTriggerEnter2D(Collider2D collider){
        Debug.Log(collider.gameObject.tag + "とぶつかった");
        if(collider.gameObject.tag == "DestroyFloor"){
            Debug.Log("ぶつかった");
            Destroy(this.gameObject);
        }
    }
}
