using UnityEngine;
using System.Collections;

public class CharaScript : MonoBehaviour {

    public GameObject gameStartObj;

    //動く床用
    private float moveFloorX = 0;
    private float charaX = 0;
    //動く床フラグ
    private bool isOnMoveFloor = false;

    //GameStartObjectのScript
    NewGameStartScript gameStartScript;

    // Use this for initialization
    void Start () {
        gameStartObj = GameObject.Find("GameStartObj");
        if (gameStartObj != null) {
            gameStartScript = gameStartObj.GetComponent<NewGameStartScript>();
        }
    }

    void OnCollisionEnter2D(Collision2D collision){
        if(collision.gameObject.tag == "Bomb"){
            gameStartScript.damageBomb();
        } else if(collision.gameObject.tag == "Fall"){
            gameStartScript.damageFall();
        } else if(collision.gameObject.tag == "MoveFloor"){
            isOnMoveFloor = true;
            moveFloorX = collision.gameObject.transform.localPosition.x;
            charaX = this.gameObject.transform.localPosition.x;
            //this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        } else if(collision.gameObject.tag == "Floor"){
            if (!isOnMoveFloor) {
                //gameStartScript.correctCharaPositionX();
            }
        } else if(collision.gameObject.tag == "Rock"){
            //gameStartScript.goBack();
        } else if(collision.gameObject.tag == "Goal"){
            gameStartScript.goal();
        } else if(collision.gameObject.tag == "DamageWall"){
            gameStartScript.damageWall();
        }
    }

    void OnCollisionStay2D(Collision2D collision){
        if(collision.gameObject.tag == "MoveFloor"){
            float posX = charaX + (collision.gameObject.transform.localPosition.x - moveFloorX);
            this.gameObject.transform.localPosition = new Vector3(posX, this.gameObject.transform.localPosition.y, 0);
        }
    }

    void OnCollisionExit2D(Collision2D collision){
        if(collision.gameObject.tag == "MoveFloor"){
            Debug.Log("動く床から離れた");
            isOnMoveFloor = false;
            //this.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }
}
