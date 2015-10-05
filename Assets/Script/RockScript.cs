using UnityEngine;
using System.Collections;

public class RockScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy(this.gameObject, 2.0f);
	}

	void OnCollisionEnter2D(Collision2D collision){
		if(collision.gameObject.tag == "Floor"){
			//指定時間後にトリガーに変更し削除
			StartCoroutine(rockDestroy());
		}
	}

	IEnumerator rockDestroy() {
		yield return new WaitForSeconds(0.3f);
		this.gameObject.GetComponent<EdgeCollider2D>().isTrigger = true; 
	}
}
