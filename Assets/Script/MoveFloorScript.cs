using UnityEngine;
using System.Collections;

public class MoveFloorScript : MonoBehaviour {

    private float addCubePositionX = 1.05f;
    //フラグ
    private bool isRight = true;

    private float moveSpeed = 0;

    // Use this for initialization
    void Start () {
        StartCoroutine(moveStart());
        moveSpeed = UnityEngine.Random.Range(0.5F, 1.4f);
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
            posX = this.gameObject.transform.localPosition.x + (addCubePositionX * 2.2f);
        } else {
            posX = this.gameObject.transform.localPosition.x - (addCubePositionX * 2.2f);
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
            BattleCharaScript bcs = collision.gameObject.GetComponent<BattleCharaScript>(); 
            if (bcs != null) {
                if (bcs.checkIsMine()) {
                    this.gameObject.GetComponent<BoxCollider2D>().isTrigger= true;
                    StartCoroutine(resetTrigger());
                }
            } else {
                this.gameObject.GetComponent<BoxCollider2D>().isTrigger= true;
                StartCoroutine(resetTrigger());
            }
        }
    }

    public void updateFloorSpeed(float sp) {
        moveSpeed = sp;
    }
}
