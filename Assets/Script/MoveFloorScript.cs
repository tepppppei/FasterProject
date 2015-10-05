using UnityEngine;
using System.Collections;

public class MoveFloorScript : MonoBehaviour {

	private float moveSpeed = 1.4f;
	private float addCubePositionX = 1.05f;
	//フラグ
	private bool isRight = true;

	// Use this for initialization
	void Start () {
		StartCoroutine(moveStart());
	}

	IEnumerator moveStart() {
		yield return new WaitForSeconds(0.6f);
		move();
	}

	IEnumerator resetTrigger() {
		yield return new WaitForSeconds(0.6f);
		this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
	}

	private void move() {
		float posX = 0;
		if (isRight) {
			posX = this.gameObject.transform.localPosition.x + (addCubePositionX * 2);
		} else {
			posX = this.gameObject.transform.localPosition.x - (addCubePositionX * 2);
		}

		iTween.MoveTo(this.gameObject, iTween.Hash(
			"position", new Vector3(posX, this.gameObject.transform.position.y, 0),
			"time", moveSpeed, 
			"oncomplete", "moveChange", 
			"oncompletetarget", gameObject, 
			"easeType", "linear"
		));
	}

	private void moveChange() {
		isRight = !isRight;
		move();
	}

    void OnCollisionExit2D(Collision2D collision){
		if(collision.gameObject.tag == "Chara"){
			this.gameObject.GetComponent<BoxCollider2D>().isTrigger= true;
			StartCoroutine(resetTrigger());
		}
    }
}
