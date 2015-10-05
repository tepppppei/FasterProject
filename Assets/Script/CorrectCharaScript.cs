using UnityEngine;
using System.Collections;

public class CorrectCharaScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnCollisionEnter2D(Collision2D collision){
		if(collision.gameObject.tag == "Chara"){
			collision.gameObject.transform.localPosition = new Vector3(
				this.gameObject.transform.localPosition.x,
				collision.gameObject.transform.localPosition.y,
				0
			);
		}
	}
}
