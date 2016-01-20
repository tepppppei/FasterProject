using UnityEngine;
using System.Collections;

public class FloorDestroyScript : MonoBehaviour {

    // Use this for initialization
    void Start () {
    }

    void OnTriggerEnter2D(Collider2D collider){
        if(collider.gameObject.tag == "DestroyFloor"){
            Destroy(this.gameObject);
        }
    }
}
